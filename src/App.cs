using System;
using Autodesk.Revit.UI;
using Loam.Revit.Connector.Mcp;
using Loam.Revit.Connector.RevitBridge;

namespace Loam.Revit.Connector
{
    public sealed class App : IExternalApplication
    {
        private McpServer _server;
        private RevitContext _ctx;

        public Result OnStartup(UIControlledApplication application)
        {
            _ctx = new RevitContext(application);

            var listen = Environment.GetEnvironmentVariable("LOAM_REVIT_LISTEN")
                         ?? "http://127.0.0.1:47100/mcp";
            var token  = Environment.GetEnvironmentVariable("LOAM_REVIT_TOKEN");

            _server = new McpServer(listen, token, _ctx);
            _server.Start();
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            _server?.Stop();
            return Result.Succeeded;
        }
    }
}
