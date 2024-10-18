using System.Diagnostics;
using System.Reflection;

namespace Vostok.Clusterclient.Tracing.OpenTelemetry;

internal static class Instrumentation
{
    private static readonly AssemblyName AssemblyName = typeof(Instrumentation).Assembly.GetName();

    public static readonly string ActivitySourceName = AssemblyName.Name;
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName, AssemblyName.Version?.ToString());
}