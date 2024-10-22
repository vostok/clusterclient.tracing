#if NET6_0_OR_GREATER
using System;
using System.Diagnostics;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Extensions.Http;

namespace Vostok.Clusterclient.Tracing.Helpers;

/// <summary>
/// A small helper wrapper to pass <see cref="Activity"/> into <see cref="ProxyStream"/>
/// </summary>
internal sealed class OpenTelemetryHelperSpanBuilder : IHttpRequestSpanBuilder
{
    private readonly Activity activity;

    public OpenTelemetryHelperSpanBuilder(Activity activity) =>
        this.activity = activity;

    public void SetAnnotation(string key, object value, bool allowOverwrite = true) =>
        activity.SetTag(key, value);

    public void Dispose() => activity.Dispose();

    public void SetRequestDetails(Uri url, string method, long? bodySize) {}
    public void SetRequestDetails(string url, string method, long? bodySize)  {}
    public void SetResponseDetails(int responseCode, long? bodySize)  {}
    public void SetBeginTimestamp(DateTimeOffset timestamp) {}
    public void SetEndTimestamp(DateTimeOffset? timestamp) {}
    public ISpan CurrentSpan => throw new NotSupportedException();
}
#endif