using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Tracing.Helpers;

namespace Vostok.Clusterclient.Tracing.OpenTelemetry;

internal class OpenTelemetryTracingTransport : ITransport
{
    private readonly ITransport transport;
    private readonly OpenTelemetryTracingConfiguration config;

    public OpenTelemetryTracingTransport([NotNull] ITransport transport, [NotNull] OpenTelemetryTracingConfiguration config)
    {
        this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
        this.config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public TransportCapabilities Capabilities => transport.Capabilities;

    [CanBeNull]
    public Func<string> TargetServiceProvider { get; set; }

    [CanBeNull]
    public Func<string> TargetEnvironmentProvider { get; set; }

    public async Task<Response> SendAsync(Request request, TimeSpan? connectionTimeout, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var activity = Instrumentation.ActivitySource.StartActivity(Instrumentation.ClientSpanInitialName, ActivityKind.Client);

        if (activity?.IsAllDataRequested is true)
            request = FillRequestAttributes(activity, request);

        request = PropagateContext(activity, request);

        var response = await transport
            .SendAsync(request, connectionTimeout, timeout, cancellationToken)
            .ConfigureAwait(false);

        if (activity?.IsAllDataRequested is true)
            response = FillResponseAttributes(activity, response);
        else
            activity?.Dispose();

        return response;
    }

    private Request FillRequestAttributes(Activity activity, Request request)
    {
        activity.FillRequestAttributes(request, TargetServiceProvider, TargetEnvironmentProvider);

        if (config.AdditionalRequestTransformation is not null)
            request = config.AdditionalRequestTransformation(request, activity.Context);

        config.EnrichWithRequest?.Invoke(activity, request);

        return request;
    }

    private Response FillResponseAttributes(Activity activity, Response response)
    {
        config.EnrichWithResponse?.Invoke(activity, response);

        return activity.FillResponseAttributes(response);
    }

    private static Request PropagateContext(Activity activity, Request request)
    {
        var contextToPropagate = activity?.Context ?? Activity.Current?.Context;
        if (!contextToPropagate.HasValue)
            return request;

        var propagator = Propagators.DefaultTextMapPropagator;

        propagator.Inject(new PropagationContext(contextToPropagate.Value, Baggage.Current),
            string.Empty,
            (_, key, value) =>
                request = request.WithHeader(key, value));

        return request;
    }
}