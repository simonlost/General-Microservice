using GeneralInsurance.Interfaces.Interfaces;

namespace GeneralInsurance.DataAccess.Middleware
{
    public interface ICustomerOrderMiddlewareStore : IDataStore
    {
    void SetMarketingPreferences(AndoCustomer customer);
    }
}