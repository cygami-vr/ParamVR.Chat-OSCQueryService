using Microsoft.Extensions.Logging;
using VRC.OSCQuery;

public class Runner
{
    private readonly ILogger<OSCQueryService> _logger;

    public Runner(ILogger<OSCQueryService> logger)
    {
        _logger = logger;
    }

    public ILogger<OSCQueryService> getLogger()
    {
        return _logger;
    }
}