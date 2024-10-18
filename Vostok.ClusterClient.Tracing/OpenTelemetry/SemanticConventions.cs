namespace Vostok.Clusterclient.Tracing.OpenTelemetry;

internal static class SemanticConventions
{
    // note (ponomaryovigor, 15.10.2024): Attributes copied manually from OTel registry: https://opentelemetry.io/docs/specs/semconv/attributes-registry/.
    // Replace with constants from SemanticConventions package when it will be released.
    public const string AttributeHttpRequestMethod = "http.request.method";
    public const string AttributeHttpResponseStatusCode = "http.response.status_code";
    public const string AttributeUrlFull = "url.full";
    public const string AttributeServerAddress = "server.address";
    public const string AttributeServerPort = "server.port";
    public const string AttributeHttpRequestContentLength = "http.request.header.content-length";
    public const string AttributeHttpResponseContentLength = "http.response.header.content-length";

    // ClusterClient related attributes.
    private const string ClusterClientPrefix = "clusterclient.";

    public const string AttributeClusterRequest = ClusterClientPrefix + "cluster_request";
    public const string AttributeStreaming = ClusterClientPrefix + "response.streaming";
    public const string AttributeRequestStrategy = ClusterClientPrefix + "request.strategy";
    public const string AttributeClusterStatus = ClusterClientPrefix + "cluster_status";
}