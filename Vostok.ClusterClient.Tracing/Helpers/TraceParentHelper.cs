using System;
using Vostok.Clusterclient.Core.Model;
using Vostok.Tracing.Abstractions;

namespace Vostok.Clusterclient.Tracing.Helpers;

/// <summary>
/// Temporary helper for duplicating tracing context in traceparent header. Partially copied from Singular
/// </summary>
internal static class TraceParentHelper
{
    private static readonly char[] Format = ['n'];
    private const string TraceParentHeader = "traceparent";
    private const int HeaderLength = 2 + 1 + 32 + 1 + 16 + 1 + 2;

    public static Request AddHeader(Request request, TraceContext traceContext)
    {
        if (!TrySerialize(traceContext.TraceId, traceContext.SpanId, out var serializedTraceParent))
            return request;

        return request.WithHeader(TraceParentHeader, serializedTraceParent);
    }

    private static bool TrySerialize(Guid traceId, Guid spanId, out string traceParentHeaderValue)
    {
        try
        {
            if (traceId == Guid.Empty || spanId == Guid.Empty)
            {
                traceParentHeaderValue = null;
                return false;
            }
#if NETSTANDARD2_0
            traceParentHeaderValue = $"00-{traceId:N}-{spanId.ToString("N").Substring(0, 16)}-01";
#else
            traceParentHeaderValue = string.Create(HeaderLength,
                (traceId, spanId),
                (span, tuple) => FillTraceParentHeader(tuple, span));
#endif
            return true;
        }
        catch
        {
            traceParentHeaderValue = null;
            return false;
        }
    }

#if !NETSTANDARD2_0
    private static void FillTraceParentHeader((Guid traceId, Guid spanId) tuple, Span<char> traceParent)
    {
        var (traceId, spanId) = tuple;
        var guidFormat = Format.AsSpan();

        traceParent[0] = '0';
        traceParent[1] = '0';
        traceParent[2] = '-';

        if (!traceId.TryFormat(traceParent.Slice(3), out var charsWritten, guidFormat) || charsWritten != 32)
            throw new Exception("Bug in code!");

        traceParent[35] = '-';

        Span<char> spanIdStringValue = stackalloc char[32];
        if (!spanId.TryFormat(spanIdStringValue, out charsWritten, guidFormat) || charsWritten != 32)
            throw new Exception("Bug in code!");
        spanIdStringValue.Slice(0, 16).CopyTo(traceParent.Slice(36));

        traceParent[52] = '-';
        traceParent[53] = '0';
        //We sample 100% of traces!
        traceParent[54] = '1';
    }
#endif
}