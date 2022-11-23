using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ServiceB.Controllers;

[ApiController]
[Route("")]
public class PingController : ControllerBase
{
    private readonly ILogger<PingController> _logger;
    public PingController(ILogger<PingController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [Route("ping-internal")]
    public async Task<IActionResult> PingInternalAsync()
    {
        CustomMetrics.Pings.Add(1);  
        
        var proxy = Activity.Current?.GetBaggageItem("sample.proxy");
        if (string.IsNullOrWhiteSpace(proxy))
            _logger.LogWarning("/ping-internal called without proxy!!");
        else
            _logger.LogInformation("/ping-internal called from {Proxy}", proxy);


        var random = new Random().Next(50, 100);
        CustomMetrics.PingDelay.Record(random);

        using (var activity = CustomTraces.Default.StartActivity("BuildPingResult"))
        {
            activity?.SetTag("http.method", Request.Method);
            activity?.SetTag("http.url", Request.Path);
            activity?.SetTag("http.host", Request.Host.Value);
            activity?.SetTag("http.scheme", Request.Scheme);
            await Task.Delay(random);
            activity?.SetTag("serviceb.delay", random);
        }
        return Ok(new { Status =  "ok"});
    }
}
