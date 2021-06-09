using GeneralInsurance.Interfaces.Interfaces;
using System.Collections;

namespace GeneralInsurance.DataAccess.Middleware
{
    public interface ISearchMiddlewareStore : IDataStore
    {
        IList<Customer> Search(AndoCustomer customer);
        IList<Customer> Validate(AndoCustomer customer);
        int CheckEmailAndPhoneUseage(AndoCustomer customer);
    }
}