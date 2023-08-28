﻿using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator.Tests
{
    public class WrittenParticipantB : AbstractParticipant, IParticipant<MessageWritten>
    {
        private readonly Guid _id = Guid.NewGuid();

        public void ProcessMessage(IParticipantContext<MessageWritten> context)
        {
            Guard.AgainstNull(context, nameof(context));

            Console.WriteLine($@"[event-{_id}] : text = '{context.Message.Text}'");

            Call();
        }
    }
}