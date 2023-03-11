using Grpc.Core;
using Moq;

namespace AyBorg.Agent.Tests.Helpers;

internal static class GrpcCallHelpers
{
    public static AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(TResponse response)
    {
        return new AsyncUnaryCall<TResponse>(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
    }

    public static AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(StatusCode statusCode)
    {
        var status = new Status(statusCode, string.Empty);
        return new AsyncUnaryCall<TResponse>(
            Task.FromException<TResponse>(new RpcException(status)),
            Task.FromResult(new Metadata()),
            () => status,
            () => new Metadata(),
            () => { });
    }

    public static AsyncServerStreamingCall<TResponse> CreateAsyncServerStreamingCall<TResponse>(List<TResponse> values)
    {
        List<TResponse>.Enumerator enumerator = values.GetEnumerator();
        var mockAsyncStreamReader = new Mock<IAsyncStreamReader<TResponse>>();
        mockAsyncStreamReader.Setup(r => r.MoveNext(It.IsAny<CancellationToken>())).ReturnsAsync(() => enumerator.MoveNext());
        mockAsyncStreamReader.Setup(r => r.Current).Returns(() => enumerator.Current);
        return new AsyncServerStreamingCall<TResponse>(
            mockAsyncStreamReader.Object,
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
    }
}
