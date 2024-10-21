using JetBrains.Annotations;
using Vostok.Clusterclient.Context;
using Vostok.Clusterclient.Core;
using Vostok.Tracing.Abstractions;
#if NET6_0_OR_GREATER
using OpenTelemetry.Trace;
using Vostok.Clusterclient.Tracing.OpenTelemetry;
#endif

namespace Vostok.Clusterclient.Tracing
{
    [PublicAPI]
    public static class IClusterClientConfigurationExtensions
    {
        /// <summary>
        /// Sets up distributed tracing of HTTP requests using given <paramref name="tracer"/>.
        /// </summary>
        public static void SetupDistributedTracing([NotNull] this IClusterClientConfiguration config, [NotNull] ITracer tracer)
            => SetupDistributedTracing(config, new TracingConfiguration(tracer));

        /// <summary>
        /// Sets up distributed tracing of HTTP requests using given <paramref name="configuration"/>.
        /// </summary>
        public static void SetupDistributedTracing([NotNull] this IClusterClientConfiguration config, [NotNull] TracingConfiguration configuration)
        {
            config.SetupDistributedContext();

            var tracingTransport = new TracingTransport(config.Transport, configuration)
            {
                TargetServiceProvider = () => config.TargetServiceName,
                TargetEnvironmentProvider = () => config.TargetEnvironment
            };

            var tracingModule = new TracingModule(configuration)
            {
                TargetServiceProvider = () => config.TargetServiceName,
                TargetEnvironmentProvider = () => config.TargetEnvironment
            };

            config.Transport = tracingTransport;
            config.AddRequestModule(tracingModule, typeof(DistributedContextModule));
        }
#if NET6_0_OR_GREATER
        /// <summary>
        /// Sets up OTel distributed tracing of HTTP requests.
        /// </summary>
        /// <param name="config">Current <see cref="IClusterClientConfiguration"/></param>
        /// <param name="configuration"><see cref="OpenTelemetryTracingConfiguration"/> for tracing additional configuration</param>
        /// <remarks>Note that you also need to call <see cref="ClusterClientTracerProviderBuilderExtensions.AddClusterClientInstrumentation"/> to enable activities listening.</remarks>
        public static void SetupOpenTelemetryTracing(
            [NotNull] this IClusterClientConfiguration config,
            [CanBeNull] OpenTelemetryTracingConfiguration configuration = null)
        {
            config.SetupDistributedContext();

            configuration ??= new OpenTelemetryTracingConfiguration();

            var tracingTransport = new OpenTelemetryTracingTransport(config.Transport, configuration)
            {
                TargetServiceProvider = () => config.TargetServiceName,
                TargetEnvironmentProvider = () => config.TargetEnvironment
            };

            var tracingModule = new OpenTelemetryTracingModule(configuration)
            {
                TargetServiceProvider = () => config.TargetServiceName,
                TargetEnvironmentProvider = () => config.TargetEnvironment
            };

            config.Transport = tracingTransport;
            config.AddRequestModule(tracingModule, typeof(DistributedContextModule));
        }
#endif
    }
}