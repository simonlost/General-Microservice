using App.Metrics.Health;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using GeneralInsurance.DataAccess;
using Microsoft.IdentityModel.Logging;

namespace GeneralInsurance.Api.HealthChecks
{
    public class StoreConnectionHealthCheck : HealthCheck
    {
        private readonly IDataStore _store;

        public StoreConnectionHealthCheck(IDataStore store) : base("Database")
        {
            _store = store;
        }

        protected override ValueTask<HealthCheckResult> CheckAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                _store.DoHealthCheck();
                return cancellationToken.IsCancellationRequested
                    ? new ValueTask<HealthCheckResult>(HealthCheckResult.Degraded)
                    : new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Healthcheck failed");
                return new ValueTask<HealthCheckResult>(HealthCheckResult.Unhealthy());
            }
        }
    }
}