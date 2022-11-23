using System.Diagnostics;
using OpenTelemetry;

internal sealed class FilteringProcessor : BaseProcessor<Activity>
{

  private readonly Func<Activity, bool> filter;
    public FilteringProcessor(Func<Activity, bool> filter)
    {
        this.filter = filter ?? throw new ArgumentNullException(nameof(filter));
    }

    public override void OnEnd(Activity activity)
    {
        // Bypass export if the Filter returns false.
        if (this.filter(activity))
        {
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }
    }

}
