using Shuttle.Core.Contract;

namespace Shuttle.Core.Mediator;

public class RequestResponseMessage<TRequest, TResponse> : RequestMessage<TRequest>
{
    public RequestResponseMessage(TRequest request) : base(request)
    {
    }

    public RequestResponseMessage(TRequest request, TResponse response) : base(request)
    {
        if (!typeof(TResponse).IsValueType)
        {
            Guard.AgainstNull(response);
        }

        Response = response;
    }

    public TResponse? Response { get; private set; }

    public RequestResponseMessage<TRequest, TResponse> WithResponse(TResponse response)
    {
        Guard.AgainstNull(response);

        Response = response;

        return this;
    }
}