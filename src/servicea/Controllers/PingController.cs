using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using ServiceA.Configuration;

namespace ServiceA.Controllers;

[ApiController]
[Route("")]
public class PingController : ControllerBase
{
    private readonly ServiceConfig _cfg;
    private readonly ILogger<PingController> _logger;

    public PingController(ServiceConfig cfg, ILogger<PingController> logger)
    {
        _cfg = cfg;
        _logger = logger;
    }

    [HttpPost]
    [Route("ping")]
    public async Task<IActionResult> PingAsync()
    {
        Activity.Current?.AddBaggage("sample.proxy",GetBaseUrl(Request));
        var client = new HttpClient();
        try{
            var response = await client.PostAsync($"http://{_cfg.BackendServiceName}:{_cfg.BackendPort}/ping-internal", null);
            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while sending request to backend service");
        }
        return StatusCode((int)HttpStatusCode.InternalServerError);
        
    }

    private string GetBaseUrl(HttpRequest req)
    {
        var uriBuilder = new UriBuilder(req.Scheme, req.Host.Host, req.Host.Port ?? -1);
        if (uriBuilder.Uri.IsDefaultPort)
        {
            uriBuilder.Port = -1;
        }

        return uriBuilder.Uri.AbsoluteUri;
    }
}
