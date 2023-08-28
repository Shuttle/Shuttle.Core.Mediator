using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Core.Mediator.Tests
{
    public class MultipleParticipants : 
        IParticipant<MultipleParticipantMessageA>,
        IParticipant<MultipleParticipantMessageB>
    {
        private static readonly List<object> MessagesReceived = new List<object>();

        public int MessageTypeCount(Type type)
        {
            return MessagesReceived.Count(item => item.GetType() == type);
        }

        public void ProcessMessage(IParticipantContext<MultipleParticipantMessageA> context)
        {
            MessagesReceived.Add(context.Message);
        }

        public void ProcessMessage(IParticipantContext<MultipleParticipantMessageB> context)
        {
            MessagesReceived.Add(context.Message);
        }
    }
}