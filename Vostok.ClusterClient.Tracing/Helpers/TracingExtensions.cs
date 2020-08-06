using Vostok.Clusterclient.Core.Model;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Extensions.Http;

namespace Vostok.Clusterclient.Tracing.Helpers
{
    internal static class TracingExtensions
    {
        public static void SetRequestDetails(this IHttpRequestSpanBuilder builder, Request request)
        {
            builder.SetRequestDetails(
                request.Url,
                request.Method,
                request.Content?.Length ??
                request.CompositeContent?.Length ??
                request.StreamContent?.Length);
        }

        public static ClusterResult SetResponseDetails(this IHttpRequestClusterSpanBuilder builder, ClusterResult result)
        {
            builder.SetClusterStatus(result.Status.ToString());

            var newResponse = builder.SetResponseDetails(result.Response);

            if (ReferenceEquals(result.Response, newResponse))
                return result;

            return new ClusterResult(
                result.Status,
                result.ReplicaResults,
                newResponse,
                result.Request);
        }

        public static Response SetResponseDetails(this IHttpRequestSpanBuilder builder, Response response)
        {
            if (response.HasStream)
            {
                builder.SetAnnotation(Constants.StreamingAnnotation, true);
                builder.SetAnnotation(WellKnownAnnotations.Http.Response.Code, (int)response.Code);

                return new Response(
                    response.Code,
                    null,
                    response.Headers,
                    new ProxyStream(response.Stream, builder));
            }

            builder.SetResponseDetails((int)response.Code, GetContentLength(response));
            builder.Dispose();

            return response;
        }

        private static long? GetContentLength(Response response) =>
            response.HasContent ? (long?)response.Content.Length : null;
    }
}