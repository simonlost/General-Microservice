using GeneralInsurance.Interfaces.Interfaces;

namespace GeneralInsurance.DataAccess.Middleware
{
    public interface ICustomerMiddlewareStore : IDataStore
    {
        Customer StoreCustomer(AndoCustomer customer);
    }
}