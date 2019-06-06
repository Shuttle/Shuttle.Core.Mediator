using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shuttle.Core.Container;

namespace Shuttle.Core.Mediator.Tests
{
    [TestFixture]
    public class MediatorFixture
    {
        [Test]
        public void Should_be_able_to_send_a_message_to_a_single_observer()
        {
            var observer = new WriteMessageObserver();
            var mediator = new Mediator(new DelegatedComponentResolver(type => null, type => new [] { observer }));

            mediator.Send(new WriteMessage {Text = "hello world!"}).Wait();

            Assert.That(observer.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Should_be_able_send_a_message_to_multiple_observers()
        {
            var observers = new List<IMessageObserver<MessageWritten>>
            {
                new MessageWrittenObserver(),
                new MessageWrittenObserver()
            };
            var mediator = new Mediator(new DelegatedComponentResolver(type => null, type => observers));

            mediator.Send(new MessageWritten { Text = "hello world!" }).Wait();

            foreach (var observer in observers)
            {
                Assert.That(((MessageWrittenObserver)observer).CallCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void Should_be_able_to_perform_pipeline_processing()
        {
            var observers = new List<IMessageObserver<RegisterMessage>>
            {
                new BeforeMessageObserver(),
                new BeforeMessageObserver(),
                new MessageObserver(),
                new MessageObserver(),
                new AfterMessageObserver(),
                new AfterMessageObserver()
            };
            var mediator = new Mediator(new DelegatedComponentResolver(type => null, type => observers));

            var message = new RegisterMessage();

            mediator.Send(message).Wait();

            Assert.That(message.Messages.Count(), Is.EqualTo(6));

            foreach (var observer in observers)
            {
                Assert.That(((Observer)observer).CallCount, Is.EqualTo(1));
            }

            foreach (var text in message.Messages)
            {
                Console.WriteLine(text);
            }
        }
    }
}