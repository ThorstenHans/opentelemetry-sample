using System.Diagnostics.Metrics;
using System.Reflection;
using OpenTelemetry.Metrics;

public class CustomMetrics
 {
    
    public static readonly Meter Default = new("ServiceB", 
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0");
    
    public  static readonly Counter<long> Pings = Default.CreateCounter<long>("thorstenhans_serviceb_pings", description: "Total number of pings");
    public static readonly Histogram<int> PingDelay = Default.CreateHistogram<int>("thorstenhans_serviceb_ping_delay", "ms", "Think time in ms for a ping");

    public static readonly ExplicitBucketHistogramConfiguration PingDelayView = 
        new ExplicitBucketHistogramConfiguration{
            Boundaries = new double[] { 25, 50, 60, 70, 80, 90, 100, 125 }
        };
        
}
