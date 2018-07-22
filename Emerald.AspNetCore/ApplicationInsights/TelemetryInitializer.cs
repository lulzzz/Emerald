using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Emerald.AspNetCore.ApplicationInsights
{
    internal sealed class TelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _applicationName;

        public TelemetryInitializer(string applicationName)
        {
            _applicationName = applicationName;
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = _applicationName;
        }
    }
}