using System;
using System.Collections.Generic;
using NUnit.Framework;
using Shuttle.Core.Container;

namespace Shuttle.Core.Mediator.Tests
{
    [TestFixture]
    public class MediatorFixture
    {
        [Test]
        public void Should_be_able_to_send_a_message()
        {
            var observer = new WriteMessageObserver();
            var mediator = new Mediator(new DelegatedComponentResolver(type => observer, type => null));

            mediator.Send(new WriteMessage {Text = "hello world!"}).Wait();

            Assert.That(observer.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Should_be_able_publish_a_message()
        {
            var observers = new List<IMessageObserver<MessageWritten>>
            {
                new MessageWrittenObserver(),
                new MessageWrittenObserver()
            };
            var mediator = new Mediator(new DelegatedComponentResolver(type => null, type => observers));

            mediator.Publish(new MessageWritten { Text = "hello world!" }).Wait();

            foreach (var observer in observers)
            {
                Assert.That(((MessageWrittenObserver)observer).CallCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void Should_be_able_to_request_a_response()
        {
            var observer = new RequestObserver();
            var mediator = new Mediator(new DelegatedComponentResolver(type => observer, type => null));

            var response = mediator.Request<ResponseMessage>(new RequestMessage { RequestText = "hello world!" }).Result;

            Console.WriteLine($@"[response] : text = '{response.ResponseText}'");

            Assert.That(observer.CallCount, Is.EqualTo(1));
        }
    }
}