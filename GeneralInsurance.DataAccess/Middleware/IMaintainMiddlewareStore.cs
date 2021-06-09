using GeneralInsurance.Interfaces.Interfaces;

namespace GeneralInsurance.DataAccess.Middleware
{
    public interface IMaintainMiddlewareStore : IDataStore
    {
        void CreateContactDetails(AndoCustomer customer);
    }
}