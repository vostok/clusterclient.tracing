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
    // Currently experimental OTel attributes
    public const string AttributeHttpRequestBodySize = "http.request.body.size";
    public const string AttributeHttpResponseBodySize = "http.response.body.size";

    // ClusterClient related attributes.
    private const string ClusterClientPrefix = "clusterclient.";

    public const string AttributeClusterRequest = ClusterClientPrefix + "request.is_cluster";
    public const string AttributeRequestStrategy = ClusterClientPrefix + "request.strategy";
    public const string AttributeStreaming = ClusterClientPrefix + "response.is_streaming";
    public const string AttributeClusterStatus = ClusterClientPrefix + "response.cluster_status";
}