﻿#if NET6_0_OR_GREATER
using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Tracing.OpenTelemetry;
using Vostok.Commons.Helpers.Url;
using Vostok.Tracing.Abstractions;

namespace Vostok.Clusterclient.Tracing.Helpers;

internal static class OpenTelemetryTracingExtensions
{
    public static void FillRequestAttributes(
        this Activity activity,
        Request request,
        [CanBeNull] Func<string> targetServiceProvider,
        [CanBeNull] Func<string> targetEnvironmentProvider)
    {
        var url = request.Url;

        activity.DisplayName = $"{request.Method} {UrlNormalizer.NormalizePath(url)}";

        FillTargetAttributes(activity, targetServiceProvider?.Invoke(), targetEnvironmentProvider?.Invoke());

        activity.SetTag(SemanticConventions.AttributeUrlFull, url.ToStringWithoutQuery());
        activity.SetTag(SemanticConventions.AttributeHttpRequestMethod, request.Method);

        if (url.IsAbsoluteUri)
        {
            activity.SetTag(SemanticConventions.AttributeServerAddress, url.Host);
            activity.SetTag(SemanticConventions.AttributeServerPort, url.Port);
        }

        var bodySize = request.Content?.Length ??
                       request.CompositeContent?.Length ??
                       request.StreamContent?.Length;
        if (bodySize.HasValue)
            activity.SetTag(SemanticConventions.AttributeHttpRequestBodySize, bodySize.Value);
    }

    public static Response FillResponseAttributes(this Activity activity, Response response)
    {
        var responseCode = (int)response.Code;
        activity.SetTag(SemanticConventions.AttributeHttpResponseStatusCode, responseCode);

        if (IsErrorResult(responseCode))
            activity.SetStatus(ActivityStatusCode.Error);

        if (response.HasStream)
        {
            activity.SetTag(SemanticConventions.AttributeStreaming, true);

            var builder = new OpenTelemetryHelperSpanBuilder(activity);
            if (response.Stream is ProxyStream proxyStream)
            {
                proxyStream.AddAdditionalBuilder(builder);
                return response;
            }

            return response.WithStream(new ProxyStream(response.Stream, builder, SemanticConventions.AttributeHttpResponseBodySize));
        }

        long? contentLength = response.HasContent ? response.Content.Length : null;
        if (contentLength.HasValue)
            activity.SetTag(SemanticConventions.AttributeHttpResponseBodySize, contentLength.Value);

        activity.Dispose();
        return response;
    }

    private static void FillTargetAttributes(
        Activity activity,
        [CanBeNull] string targetService,
        [CanBeNull] string targetEnvironment)
    {
        if (targetService is not null)
            activity.SetTag(WellKnownAnnotations.Http.Request.TargetService, targetService);
        if (targetEnvironment is not null)
            activity.SetTag(WellKnownAnnotations.Http.Request.TargetEnvironment, targetEnvironment);
    }

    private static bool IsErrorResult(int statusCode) =>
        statusCode >= 400;
}
#endif