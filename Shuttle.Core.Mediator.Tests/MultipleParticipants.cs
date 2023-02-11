using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task ProcessMessage(IParticipantContext<MultipleParticipantMessageA> context)
        {
            _messagesReceived.Add(context.Message);

            await Task.CompletedTask;
        }

        public async Task ProcessMessage(IParticipantContext<MultipleParticipantMessageB> context)
        {
            _messagesReceived.Add(context.Message);

            await Task.CompletedTask;
        }
    }

    public class MultipleParticipantMessageB
    {
    }

    public class MultipleParticipantMessageA
    {
    }
}