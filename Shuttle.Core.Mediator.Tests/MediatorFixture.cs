using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Container;

namespace Shuttle.Core.Mediator.Tests
{
    [TestFixture]
    public class MediatorFixture
    {
        [Test]
        public void Should_be_able_to_send_a_message_to_a_single_participant()
        {
            var resolver = new Mock<IComponentResolver>();
            var participant = new WriteParticipant();

            resolver.Setup(m => m.ResolveAll(typeof(IParticipant<WriteMessage>)))
                .Returns(new List<IParticipant<WriteMessage>> { participant });

            var mediator = new Mediator(resolver.Object);

            mediator.Send(new WriteMessage { Text = "hello world!" });

            Assert.That(participant.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Should_be_able_send_a_message_to_multiple_participants()
        {
            var resolver = new Mock<IComponentResolver>();
            var observers = new List<IParticipant<MessageWritten>>
            {
                new WrittenParticipantA(),
                new WrittenParticipantB()
            };

            resolver.Setup(m => m.ResolveAll(typeof(IParticipant<MessageWritten>)))
                .Returns(observers);

            var mediator = new Mediator(resolver.Object);

            mediator.Send(new MessageWritten { Text = "hello participants!" });

            foreach (var observer in observers)
            {
                Assert.That(((AbstractObserver)observer).CallCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void Should_be_able_to_perform_pipeline_processing()
        {
            var resolver = new Mock<IComponentResolver>();
            var beforeA = new BeforeRegisterParticipant();
            var beforeB = new BeforeRegisterParticipant();
            var registerA = new RegisterParticipant();
            var registerB = new RegisterParticipant();
            var afterA = new AfterRegisterParticipant();
            var afterB = new AfterRegisterParticipant();

            var observers = new List<IParticipant<RegisterMessage>>
            {
                beforeA,
                beforeB,
                registerA,
                registerB,
                afterA,
                afterB
            };

            resolver.Setup(m => m.ResolveAll(typeof(IParticipant<RegisterMessage>)))
                .Returns(observers);

            var mediator = new Mediator(resolver.Object);
            var message = new RegisterMessage();

            mediator.Send(message);

            Assert.That(message.Messages.Count(), Is.EqualTo(6));

            foreach (var observer in observers)
            {
                Assert.That(((AbstractObserver)observer).CallCount, Is.EqualTo(1));
            }

            Assert.That(beforeB.WhenCalled, Is.GreaterThan(beforeA.WhenCalled));
            Assert.That(registerA.WhenCalled, Is.GreaterThan(beforeB.WhenCalled));
            Assert.That(registerB.WhenCalled, Is.GreaterThan(registerA.WhenCalled));
            Assert.That(afterA.WhenCalled, Is.GreaterThan(registerB.WhenCalled));
            Assert.That(afterB.WhenCalled, Is.GreaterThan(afterA.WhenCalled));

            foreach (var text in message.Messages)
            {
                Console.WriteLine(text);
            }
        }
    }
}