using JetBrains.Annotations;
using Vostok.Clusterclient.Tracing;
using Vostok.Clusterclient.Tracing.OpenTelemetry;

// ReSharper disable once CheckNamespace
namespace OpenTelemetry.Trace;

/// <summary>
/// Extension methods to simplify registering of <see cref="Vostok.Clusterclient.Core.IClusterClient" /> instrumentation.
/// </summary>
[PublicAPI]
public static class ClusterClientTracerProviderBuilderExtensions
{
    /// <summary>
    /// Enables the outgoing requests automatic data collection for <see cref="Vostok.Clusterclient.Core.IClusterClient" />.
    /// </summary>
    /// <param name="builder"><see cref="TracerProviderBuilder" /> being configured.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    /// <remarks>Note that you also need to call <see cref="IClusterClientConfigurationExtensions.SetupOpenTelemetryTracing"/> to enable traces emitting for certain <see cref="Vostok.Clusterclient.Core.IClusterClient"/>.</remarks>
    public static TracerProviderBuilder AddClusterClientInstrumentation(this TracerProviderBuilder builder) =>
        builder.AddSource(Instrumentation.ActivitySourceName);
}