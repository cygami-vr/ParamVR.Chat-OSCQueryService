using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VRC.OSCQuery;

namespace Chat.ParamVr {
    public class ParamVrChatOscQueryService {

        private readonly ILogger<OSCQueryService> _logger;
        private OSCQueryService? _service;
        private readonly OscQueryServiceData _serviceData = new();

        public ParamVrChatOscQueryService(ILogger<OSCQueryService> logger) {
            _logger = logger;
        }

        public void Init() {
            AddDomainHandlers();
            _logger.LogTrace("ParamVR.Chat-OSCQueryService starting with PID {pid}", Environment.ProcessId);
            _serviceData.Pid = Environment.ProcessId;
            InitService();
            WriteServiceData();
            StartServiceListenerThread();
        }
        private void InitService() {

            var tcpPort = Extensions.GetAvailableTcpPort();
            var udpPort = Extensions.GetAvailableUdpPort();
            _serviceData.OscPortIn = udpPort;

            _service = new OSCQueryServiceBuilder()
                .WithTcpPort(tcpPort)
                .WithUdpPort(udpPort)
                .WithServiceName("ParamVRChat")
                .WithLogger(_logger)
                .StartHttpServer()
                .AdvertiseOSCQuery()
                .AdvertiseOSC()
                .Build();

            _service.AddEndpoint("/avatar", "s", Attributes.AccessValues.WriteOnly);
        }

        private void WriteServiceData() {
            var oscqueryJson = JsonConvert.SerializeObject(_serviceData);
            _logger.LogTrace("Writing JSON {oscqueryJson}", oscqueryJson);
            var appdata = Environment.GetEnvironmentVariable("APPDATA");
            using var writer = new StreamWriter(appdata + "/ParamVR.Chat/ParamVR.Chat-OSCQueryService.json");
            writer.WriteLine(oscqueryJson);
        }

        private void AddDomainHandlers() {

            AppDomain.CurrentDomain.ProcessExit += (s, e) => {
                _logger.LogTrace("ParamVR.Chat-OSCQueryService is exiting");
                _service?.Dispose();
                NLog.LogManager.Shutdown();
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                _logger.LogError((Exception)e.ExceptionObject, "Unhandled exception");
            };
        }

        public void KeepAlive() {
            _logger.LogTrace("Begin KeepAlive.");
            while (!"Exit".Equals(Console.ReadLine())) {
                Thread.Sleep(1000);
            }
            _logger.LogTrace("End KeepAlive.");
        }
    
        private void StartServiceListenerThread() {

            var thread = new Thread(() => {
                while (true) {
                    GetOscServices();
                    Thread.Sleep(5000);
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        private void GetOscServices() {
            if (_service != null) {
                var services = _service.GetOSCServices();

                foreach (var service in services) {

                    if (service.name.StartsWith("VRChat-Client")) {
                        if (_serviceData.OscPortOut != service.port) {

                            _logger.LogTrace("VRChat OSC service found on port {port}", service.port);
                            _serviceData.OscPortOut = service.port;
                            WriteServiceData();
                        }
                    }
                }
            }
        }
    }
}