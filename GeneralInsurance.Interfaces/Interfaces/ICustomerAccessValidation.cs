using GeneralInsurance.Interfaces.Models;

namespace GeneralInsurance.Interfaces.Interfaces
{
    public interface ICustomerAccessValidation
    {
        // SEARCH
        SearchResult Search(AndoCustomer customer);
        // VALIDATION
        SearchResult Validate(AndoCustomer customer);
        // CREATE
        SearchResult Create(AndoCustomer customer);
    }
}