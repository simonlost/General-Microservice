using System.Collections;
using System.Collections.Generic;
using GeneralInsurance.Interfaces.Models;

namespace GeneralInsurance.Interfaces.Interfaces
{
    public interface ICustomerAccessValidationRepository
    {
        IList<Customer> Search(AndoCustomer customer);
        IList<Customer> Validate(AndoCustomer customer);
        bool Create(AndoCustomer customer);
    }
}