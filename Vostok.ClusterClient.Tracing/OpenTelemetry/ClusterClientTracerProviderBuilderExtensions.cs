using Vostok.Clusterclient.Tracing.OpenTelemetry;

// ReSharper disable once CheckNamespace
namespace OpenTelemetry.Trace;

internal static class ClusterClientTracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddClusterClientInstrumentation(this TracerProviderBuilder builder) =>
        builder.AddSource(Instrumentation.ActivitySourceName);
}