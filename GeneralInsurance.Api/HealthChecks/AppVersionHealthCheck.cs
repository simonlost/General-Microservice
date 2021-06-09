using App.Metrics.Health;
using System.Threading;
using System.Threading.Tasks;

namespace GeneralInsurance.Api.HealthChecks
{
    public class AppVersionHealthCheck : HealthCheck
    {
        public AppVersionHealthCheck() : base("AppVersion")
        {

        }

        protected override ValueTask<HealthCheckResult> CheckAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return new ValueTask<HealthCheckResult>(HealthCheckResult.Ignore(Program.AppVersion));
        }
    }
}