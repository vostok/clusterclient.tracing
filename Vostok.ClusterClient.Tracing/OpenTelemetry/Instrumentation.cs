using System.Diagnostics;

namespace Vostok.Clusterclient.Tracing.OpenTelemetry;

internal static class Instrumentation
{
    public const string ActivitySourceName = "Vostok.ClusterClient";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName, typeof(Instrumentation).Assembly.GetName().Version!.ToString());
}