using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Core.Mediator.Tests
{
    public class MultipleParticipants : 
        IParticipant<MultipleParticipantMessageA>,
        IParticipant<MultipleParticipantMessageB>
    {
        private static readonly List<object> _messagesReceived = new List<object>();

        public int MessageTypeCount(Type type)
        {
            return _messagesReceived.Count(item => item.GetType() == type);
        }

        public void ProcessMessage(IParticipantContext<MultipleParticipantMessageA> context)
        {
            _messagesReceived.Add(context.Message);
        }

        public void ProcessMessage(IParticipantContext<MultipleParticipantMessageB> context)
        {
            _messagesReceived.Add(context.Message);
        }
    }

    public class MultipleParticipantMessageB
    {
    }

    public class MultipleParticipantMessageA
    {
    }
}