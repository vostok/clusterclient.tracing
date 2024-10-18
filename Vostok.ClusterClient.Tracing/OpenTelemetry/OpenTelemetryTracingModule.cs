﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Tracing.Helpers;

namespace Vostok.Clusterclient.Tracing.OpenTelemetry;

internal class OpenTelemetryTracingModule : IRequestModule
{
    private readonly OpenTelemetryTracingConfiguration config;

    public OpenTelemetryTracingModule([NotNull] OpenTelemetryTracingConfiguration config)
    {
        this.config = config ?? throw new ArgumentNullException(nameof(config));
    }

    [CanBeNull]
    public Func<string> TargetServiceProvider { get; set; }

    [CanBeNull]
    public Func<string> TargetEnvironmentProvider { get; set; }

    public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
    {
        var activity = Instrumentation.ActivitySource.StartActivity(Instrumentation.ClusterSpanInitialName, ActivityKind.Client);

        if (activity?.IsAllDataRequested is true)
            FillRequestAttributes(activity, context);

        var result = await next(context).ConfigureAwait(false);

        if (activity?.IsAllDataRequested is true)
            FillResultAttributes(activity, result);
        else
            activity?.Dispose();

        return result;
    }

    private void FillRequestAttributes(Activity activity, IRequestContext context)
    {
        activity.SetTag(SemanticConventions.AttributeClusterRequest, true);

        var strategy = context.Parameters.Strategy?.ToString();
        if (strategy is not null)
            activity.SetTag(SemanticConventions.AttributeRequestStrategy, strategy);

        activity.FillRequestAttributes(context.Request, TargetServiceProvider, TargetEnvironmentProvider);

        config.EnrichWithRequest?.Invoke(activity, context.Request);
    }

    private ClusterResult FillResultAttributes(Activity activity, ClusterResult result)
    {
        config.EnrichWithClusterResult?.Invoke(activity, result);

        activity.SetTag(SemanticConventions.AttributeClusterStatus, result.Status.ToString());

        var newResponse = activity.FillResponseAttributes(result.Response);

        if (ReferenceEquals(result.Response, newResponse))
            return result;

        // TODO(kungurtsev): handle case when result.Stream is not ProxyStream.
        activity.Dispose();
        return result;
    }
}