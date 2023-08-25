using Chat.ParamVr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using VRC.OSCQuery;

var logger = InitLogging();
var service = new ParamVrChatOscQueryService(logger);
service.Init();
service.KeepAlive();

ILogger<OSCQueryService> InitLogging()
{
    var config = new ConfigurationBuilder()
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

    using var servicesProvider = new ServiceCollection()
        .AddTransient<Runner>()
        .AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(LogLevel.Trace);
            loggingBuilder.AddNLog(config);
        }).BuildServiceProvider();

    var runner = servicesProvider.GetRequiredService<Runner>();
    return runner.getLogger();
}
