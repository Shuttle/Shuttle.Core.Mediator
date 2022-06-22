using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Shuttle.Core.Mediator.Tests
{
    [TestFixture]
    public class MediatorFixture
    {
        [Test]
        public void Should_be_able_to_send_a_message_to_a_single_participant()
        {
            var services = new ServiceCollection();

            services.AddMediator(options =>
            {
                options.AddParticipant<WriteParticipant>();
            });

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            mediator.Send(new WriteMessage { Text = "hello world!" });

            Assert.That(((AbstractParticipant)provider.GetRequiredService<IParticipant<WriteMessage>>()).CallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Should_be_able_send_a_message_to_multiple_participants()
        {
            var services = new ServiceCollection();

            services.AddMediator(options =>
            {
                options.AddParticipant<WrittenParticipantA>();
                options.AddParticipant<WrittenParticipantB>();
            });

            var provider = services.BuildServiceProvider();
            var mediator = new Mediator(provider);

            mediator.Send(new MessageWritten { Text = "hello participants!" });

            foreach (var participant in provider.GetServices<IParticipant<MessageWritten>>())
            {
                Assert.That(((AbstractParticipant)participant).CallCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void Should_be_able_to_perform_pipeline_processing()
        {
            var services = new ServiceCollection();

            var beforeA = new BeforeRegisterParticipant();
            var beforeB = new BeforeRegisterParticipant();
            var registerA = new RegisterParticipant();
            var registerB = new RegisterParticipant();
            var afterA = new AfterRegisterParticipant();
            var afterB = new AfterRegisterParticipant();

            var participants = new List<IParticipant<RegisterMessage>>
            {
                beforeA,
                beforeB,
                registerA,
                registerB,
                afterA,
                afterB
            };

            services.AddMediator(options =>
            {
                foreach (var participant in participants)
                {
                    options.AddParticipant(participant);
                }
            });

            var provider = services.BuildServiceProvider();
            var mediator = new Mediator(provider);
            var message = new RegisterMessage();

            mediator.Send(message);

            Assert.That(message.Messages.Count(), Is.EqualTo(6));

            foreach (var observer in participants)
            {
                Assert.That(((AbstractParticipant)observer).CallCount, Is.EqualTo(1));
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