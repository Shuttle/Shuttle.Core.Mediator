﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator;

public class Mediator : IMediator
{
    private static readonly Type ParticipantType = typeof(IParticipant<>);

    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Dictionary<Type, ContextConstructorInvoker> _constructorCache = new();
    private readonly Dictionary<string, ContextMethodInvokerAsync> _methodCacheAsync = new();
    private readonly Dictionary<Type, List<ParticipantDelegate>> _delegates;
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider, IParticipantDelegateProvider participantDelegateProvider)
    {
        _serviceProvider = Guard.AgainstNull(serviceProvider);
        _delegates = Guard.AgainstNull(Guard.AgainstNull(participantDelegateProvider).Delegates).ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public event EventHandler<SendEventArgs>? Sending;
    public event EventHandler<SendEventArgs>? Sent;

    public async Task SendAsync(object message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var onSendEventArgs = new SendEventArgs(message, cancellationToken);

        Sending?.Invoke(this, onSendEventArgs);

        var messageType = message.GetType();
        var interfaceType = ParticipantType.MakeGenericType(messageType);
        var participants = _serviceProvider.GetServices(interfaceType).ToList();

        var hasParticipants = participants.Any();
        var hasDelegates = _delegates.TryGetValue(messageType, out var delegates);

        if (!hasParticipants && !hasDelegates)
        {
            throw new InvalidOperationException(string.Format(Resources.MissingParticipantException, messageType));
        }

        ContextConstructorInvoker? contextConstructor;

        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (!_constructorCache.TryGetValue(messageType, out contextConstructor))
            {
                contextConstructor = new(messageType);

                _constructorCache.Add(messageType, contextConstructor);
            }
        }
        finally
        {
            _lock.Release();
        }

        var participantContext = contextConstructor.CreateParticipantContext(message, cancellationToken);

            foreach (var participant in participants)
            {
                if (participant == null)
                {
                    continue;
                }

                await (await GetContextMethodInvokerAsync(participant.GetType(), messageType, interfaceType)).Invoke(participant, participantContext).ConfigureAwait(false);
            }

        if (delegates != null)
        {
            foreach (var participantDelegate in delegates)
            {
                if (participantDelegate.HasParameters)
                {
                    await (Task)participantDelegate.Handler.DynamicInvoke(participantDelegate.GetParameters(_serviceProvider, participantContext))!;
                }
                else
                {
                    await (Task)participantDelegate.Handler.DynamicInvoke()!;
                }
            }
        }

        Sent?.Invoke(this, onSendEventArgs);
    }

    private async Task<ContextMethodInvokerAsync> GetContextMethodInvokerAsync(Type participantType, Type messageType, Type interfaceType)
    {
        var key = $"{participantType.Name}:{messageType.Name}";

        await _lock.WaitAsync();

        try
        {
            if (!_methodCacheAsync.TryGetValue(key, out var contextMethod))
            {
                var methodInfo = participantType.GetInterfaceMap(interfaceType).TargetMethods.SingleOrDefault();

                if (methodInfo == null)
                {
                    throw new InvalidOperationException(string.Format(Resources.ProcessMessageMethodMissingException, participantType.FullName, messageType.FullName));
                }

                contextMethod = new(methodInfo);

                _methodCacheAsync.Add(key, contextMethod);
            }

            return contextMethod;
        }
        finally
        {
            _lock.Release();
        }
    }
}