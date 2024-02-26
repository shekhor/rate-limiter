using Microsoft.AspNetCore.Mvc;

namespace RateLimiterApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RateLimiterController : ControllerBase
{

    private readonly ILogger<RateLimiterController> _logger;

    public RateLimiterController(ILogger<RateLimiterController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "RateLimiter")]
    public RateLimiter Get()
    {
        return new RateLimiter
        {
            Status = "Approved"

        };
  
    }
}

