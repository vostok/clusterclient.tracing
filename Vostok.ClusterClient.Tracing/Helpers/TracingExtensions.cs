using System.Linq;
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

            builder.SetResponseDetails(result.Response);

            // TODO(kungurtsev): handle case when result.Response is not ProxyStream.

            return result;
        }

        public static Response SetResponseDetails(this IHttpRequestSpanBuilder builder, Response response)
        {
            if (response.HasStream)
            {
                builder.SetAnnotation(Constants.StreamingAnnotation, true);
                builder.SetAnnotation(WellKnownAnnotations.Http.Response.Code, (int)response.Code);

                if (response.Stream is ProxyStream proxyStream)
                {
                    proxyStream.AddAdditionalBuilder(builder);
                    return response;
                }

                return response.WithStream(new ProxyStream(response.Stream, builder));
            }

            builder.SetResponseDetails((int)response.Code, GetContentLength(response));
            builder.Dispose();

            return response;
        }

        private static long? GetContentLength(Response response) =>
            response.HasContent ? (long?)response.Content.Length : null;
    }
}