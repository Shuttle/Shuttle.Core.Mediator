using System.Collections.Generic;

namespace Shuttle.Core.Mediator.Tests
{
    public class RegisterMessage
    {
        private readonly List<string> _messages = new List<string>();

        public int TouchCount { get; private set; }

        public IEnumerable<string> Messages => _messages.AsReadOnly();

        public void Touch(string message)
        {
            TouchCount++;

            _messages.Add(message);
        }
    }
}