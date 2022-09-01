using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Shuttle.Core.Mediator.Tests
{
    [TestFixture]
    public class ServiceCollectionFixture
    {
        [Test]
        public void Should_be_able_to_handle_MesageWritten()
        {
            const int count = 100;

            var services = new ServiceCollection();

            services.AddMediator(builder =>
            {
                builder.AddParticipants(GetType().Assembly);
            });

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            var sw = new Stopwatch();

            sw.Start();

            for (var i = 0; i < count; i++)
            {
                mediator.Send(new MessageWritten { Text = "hello participants!" });
            }

            sw.Stop();

            Console.WriteLine($@"Sent {count} messages in {sw.ElapsedMilliseconds} ms.");

            foreach (var participant in provider.GetServices<IParticipant<MessageWritten>>())
            {
                Assert.That(((AbstractParticipant)participant).CallCount, Is.EqualTo(count));
            }
        }

        [Test]
        public void Should_be_able_to_add_participant_with_multiple_implementations()
        {
            var services = new ServiceCollection();

            services.AddMediator(builder =>
            {
                builder.AddParticipants(GetType().Assembly);
            });

            services.AddSingleton<MultipleParticipants, MultipleParticipants>();

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            mediator.Send(new MultipleParticipantMessageA());
            mediator.Send(new MultipleParticipantMessageB());
            mediator.Send(new MultipleParticipantMessageA());

            var multipleParticipants = provider.GetRequiredService<MultipleParticipants>();

            Assert.That(multipleParticipants.MessageTypeCount(typeof(MultipleParticipantMessageA)), Is.EqualTo(2));
            Assert.That(multipleParticipants.MessageTypeCount(typeof(MultipleParticipantMessageB)), Is.EqualTo(1));
        }
    }
}