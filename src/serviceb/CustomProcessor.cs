using System.Diagnostics;
using OpenTelemetry;

namespace ServiceB;

internal sealed class CustomProcessor : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        if (IsHealthOrMetricsEndpoint(activity.DisplayName))
        {
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }
    }
    
    private static bool IsHealthOrMetricsEndpoint(string displayName)
    {   
        if (string.IsNullOrEmpty(displayName))
        {
            return false;
        }
        return displayName.StartsWith("/healthz/") ||
               displayName.StartsWith("/metrics");
    }
}
