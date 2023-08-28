using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuttle.Core.Mediator.Tests
{
    public class MultipleAsyncParticipants : 
        IAsyncParticipant<MultipleParticipantMessageA>,
        IAsyncParticipant<MultipleParticipantMessageB>
    {
        private static readonly List<object> MessagesReceived = new List<object>();

        public int MessageTypeCount(Type type)
        {
            return MessagesReceived.Count(item => item.GetType() == type);
        }

        public async Task ProcessMessage(IParticipantContext<MultipleParticipantMessageA> context)
        {
            MessagesReceived.Add(context.Message);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public async Task ProcessMessage(IParticipantContext<MultipleParticipantMessageB> context)
        {
            MessagesReceived.Add(context.Message);

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}