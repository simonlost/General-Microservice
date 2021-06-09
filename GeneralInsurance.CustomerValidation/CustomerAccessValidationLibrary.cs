using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.Extensions.Configuration;
using System.Linq;
using F23.StringSimilarity;
using GeneralInsurance.DataAccess.Middleware;
using GeneralInsurance.Interfaces.Interfaces;
using GeneralInsurance.Interfaces.Models;
using System.Text.RegularExpressions;
using Serilog.Events;
using ILogger = Serilog.Iloggger;

namespace GeneralInsurance.CustomerValidation
{
    public class CustomerAccessValidationLibrary : ICustomerAccessValidation
    {
        private ISearchMiddlewareStore _repository;
        private ICustomerMiddlewareStore _createRepository;
        private IMaintainMiddlewareStore _maintainRepository;
        private ICustomerOrderMiddlwareStore _customerOrderRepository;
        private IConfiguration _config;
        private Damerau _DAAlgorithm;

        private readonly ILogger Log;

        private const int FIRSTNAME_TOLERANCE = 1;
        private const int MIDDLENAMES_TOLERANCE = 1;

        public CustomerAccessValidationLibrary(IConfiguration configuration, ISearchMiddlewareStore repository,
            ICustomerMiddlewareStore createRepository, IMaintainMiddlewareStore maintainRepository,
            ICustomerOrderMiddlwareStore customerOrderRepository)
        {
            _config = configuration;
            _repository = repository;
            _createRepository = createRepository;
            _maintainRepository = maintainRepository;
            _customerOrderRepository = customerOrderRepository;

            _DAAlgorithm = new Damerau();
            Log = Serilog.Log.ForContext<CustomerAccessValidationLibrary>();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public SearchResult Search(AndoCustomer customer)
        {
            Log.ForContext("RequestUtcDateTime", DateTime.UtcNow);
            Log.ForContext("CorrelationId", customer.CorrolationId);
            Log.Write(LogEventLevel.Information, "Customer Validation Search: Ando Customer to Search against: {0}",
                customer.Print());

            SearchResult theResult = new SearchResult();
            theResult.CorollationId = customer.CorollationId;
            try
            {
                IList<Customer> searchResult = _repository.Search(customer);

                foreach (var cust in searchResult)
                {
                    Log.Write(LogEventLevel.Information,
                        $"Customer Validation Search: Customer to compare against: {cust.Print(customer.CorollationId.ToString())}");

                    if (searchResult.Any())
                    {
                        // if individual rather then organisation
                        if (String.IsNullOrEmpty(customer.OrganisationName))
                        {
                            CalculateDistances(ref searchResult, customer);
                            // check first names distance
                            List<Customer> toRemoveList = new List<Customer>();
                            Customer toRemove;
                            foreach (Customer current in searchResult)
                            {
                                toRemove = CheckIndividualSearchResult(current, customer);
                                if (toRemove != null)
                                {
                                    toRemoveList.Add(toRemove);
                                }
                            }

                            // remove those that do not have a close enough first name match
                            foreach (Customer remove in ToRemoveList)
                            {
                                searchResult.Remove(remove);
                            }

                            // if no acceptable matches left after first name check return error
                            if (!searchResult.Any())
                            {
                                theResult.CustomerFound = false;
                                theResult.Errors = new List<Error>(new Error()
                                    {Code = 1, Description = "No acceptable match found"});
                                Log.Write(LogEventLevel.Information,
                                    $"Customer Validation Search: Search Failed: {LogResult(theResult)}");
                                return theResult;
                            }

                            if (searchResult.Count > 1)
                            {
                                theResult.CustomerFound = false;
                                theResult.Errors = new List<Error>(new Error()
                                    {Code = 2, Description = "Multiple matches found"});
                                Log.Write(LogEventLevel.Information,
                                    $"Customer Validation Search: Search Failed: {LogResult(theResult)}");
                                return theResult;
                            }

                            // if only 1 result - then return that customer
                            theResult.AccessNumber = searchResult[0].AccessNumber;
                            theResult.CustomerFound = true;
                            Log.Write(LogEventLevel.Information,
                                $"Customer Validation Search: Search succeeded: {LogResult(theResult)}");
                            return theResult;
                        }
                        else
                        {
                            // Can only limit on Organisation Name which is a wild card search
                            List<Customer> toRemoveList = new List<Customer>();
                            Customer toRemove;
                            foreach (Customer current in searchResult)
                            {
                                toRemove = CheckOrganisationSearchResult(current, customer);
                                if (toRemove != null)
                                {
                                    toRemoveList.Add(toRemove);
                                }
                            }

                            // remove those that do not have a close enough first name match
                            foreach (Customer remove in ToRemoveList)
                            {
                                searchResult.Remove(remove);
                            }

                            // if no acceptable matches left after first name check return error
                            if (!searchResult.Any())
                            {
                                theResult.CustomerFound = false;
                                theResult.Errors = new List<Error>(new Error()
                                    {Code = 1, Description = "No acceptable match found"});
                                Log.Write(LogEventLevel.Information,
                                    $"Customer Validation Search: Search Failed: {LogResult(theResult)}");
                                return theResult;
                            }

                            if (searchResult.Count > 1)
                            {
                                theResult.CustomerFound = false;
                                theResult.Errors = new List<Error>(new Error()
                                    {Code = 2, Description = "Multiple matches found"});
                                Log.Write(LogEventLevel.Information,
                                    $"Customer Validation Search: Search Failed: {LogResult(theResult)}");
                                return theResult;
                            }

                            // if only 1 result - then return that customer
                            theResult.AccessNumber = searchResult[0].AccessNumber;
                            theResult.CustomerFound = true;
                            Log.Write(LogEventLevel.Information,
                                $"Customer Validation Search: Search succeeded: {LogResult(theResult)}");
                            return theResult;
                        }
                    }
                    else
                    {
                        // If no results on base search assume that does not exist and create it ...
                        SearchResult theReslt;
                        if (customer.customerType.ToLower() == "consumer")
                        {
                            theReslt = Create(customer);
                        }
                        else
                        {
                            theReslt = new SearchResult()
                            {
                                CustomerFound = false, AccessNumber = null, CorollationId = customer.CorollationId,
                                Created = false,
                                Errors = new List<Error>()
                                {
                                    Code = 3,
                                    Description = $"Customer Validation Search: Searched Failed: No Customer Found"
                                }
                            };
                        }

                        Log.Write(LogEventLevel.Information,
                            "Customer Validation Search: Search succeeded: Customer Created: {0}",
                            LogResult(theResult));
                        return theReslt;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    $"Customer Validation Search: Error performing customer Validation Search: {ex.Message}, CorollationId: {customer.CorollationId}");
                theResult.CustomerFound = false;
                theResult.AccessNumber = null;
                theResult.Created = false;
                theResult.Errors = new List<Error>()
                    {new Error() {Code = 3, Description = "Error processing request, try again later"}};
                return theResult;
            }
        }

        private SearchResult Create(AndoCustomer customer)
        {
            throw new NotImplementedException();
        }

        private Customer CheckOrganisationSearchResult(Customer toCheck, AndoCustomer fromAndo)
        {
            // If first name is not acceptable then return it to be removed
            if (toCheck.Surname.ToLower() != fromAndo.OrganisationName.ToLower())
            {
                return toCheck;
            }

            return null;
        }

        private string LogResult(SearchResult theResult)
        {
            string logInfo =
                $"Calculated result, CorollationId: {theResult.CorollationId}, CustomerFound: {theResult.CustomerFound}, AccessNumber: {theResult.AccessNumber}, Error Code: ";
            if (theResult.Errors != null && theResult.Errors.Count > 0)
            {
                logInfo += theResult.Errors.First().Code;
            }
            return logInfo;
        }

        private Customer CheckIndividualSearchResult(Customer toCheck, AndoCustomer fromAndo)
        {
            // If First name is not acceptable then reutrn it to be removed
            if (toCheck.FirstNameDLDistance > FIRSTNAME_TOLERANCE)
            {
                return toCheck;
            }
            // If middle name supplied but none in bank records then error (Bank records should be more accurate)
            if (fromAndo.HasMiddleNames && !toCheck.HasMiddleNames)
            {
                return toCheck;
            }

            // If Middle names was supplied and had more then 1 error per name, then go to further checking, otherwise return match was successful
            if (!fromAndo.HasMiddleNames || !(toCheck.MiddleNamesDLDistance > MIDDLENAMES_TOLERANCE)) return null;
            // If middle name distance is too great then return error
            if (toCheck.MiddleNamesDLDistance > (1 + 1 * toCheck.NumberOfMiddleNames))
            {
                return toCheck;
            }

            bool emailOrPhoneChecked = false;
            // else check email addresses to match if possible
            if (!string.IsNullOrEmpty(fromAndo.EmailAddress) && !string.IsNullOrEmpty(toCheck.Email) &&
                fromAndo.EmailAddress.ToLower().Trim() != toCheck.Email.ToLower().Trim())
            {
                return toCheck;
            }
            else
            {
                if (!string.IsNullOrEmpty(fromAndo.EmailAddress) && !string.IsNullOrEmpty(toCheck.Email))
                {
                    emailOrPhoneChecked = true;
                }
            }
            // if mobile numebr exists and match return no error
            if (!string.IsNullOrEmpty(fromAndo.MobilePhone) && !string.IsNullOrEmpty(toCheck.Mobile) &&
                fromAndo.MobilePhone.Replace(" " ,"").Replace("+","").Replace("-","").ToLower().Trim() != toCheck.Mobile.Replace(" ", "").Replace("+", "").Replace("-", "").ToLower().Trim())
            {
                return toCheck;
            }
            else
            {
                if (!string.IsNullOrEmpty(fromAndo.MobilePhone) && !string.IsNullOrEmpty(toCheck.Mobile))
                {
                    emailOrPhoneChecked = true;
                }
            }

            if (emailOrPhoneChecked)
            {
                return null;
            }

            return toCheck;
        }

        private void CalculateDistances(ref IList<Customer> searchResult, AndoCustomer customer)
        {
            foreach (Customer current in searchResult)
            {
                string[] names = current.ForeNames.Split(" ");
                current.FirstNameDLDistance = _DAAlgorithm.Distance(names[0].ToLower(), customer.FirstName.ToLower());
                current.MiddleNames = names.Length > 1;

                if ((names.Length > 1) && !string.IsNullOrEmpty(customer.MiddleName))
                {
                    // allow for 1 error per middle name
                    string[] andoMiddleNames = customer.MiddleName.Split(" ");
                    current.NumberOfMiddleNames = names.Length - 1;
                    for (int i = 1; i < names.Length; i++)
                    {
                        if (andoMiddleNames.Length > (i - 1))
                        {
                            current.MiddleNamesDLDistance +=
                                _DAAlgorithm.Distance(names[i].ToLower(), andoMiddleNames[i - 1].ToLower());
                        }

                        if (names.Length < andoMiddleNames.Length)
                        {
                            current.MiddleNamesDLDistance += (andoMiddleNames.Length - (names.Length - 1)) * 2 + 1;
                        }
                    }
                }
                else
                {
                    current.MiddleNamesDLDistance = -1; // there is no middle name supplied to check or the bank has no middle name(s) for this customer
                }
            }
        }

        public SearchResult Validate(AndoCustomer customer)
        {
            SearchResult theResult = new SearchResult(CorrollationId = customer.CorollationId);
            try
            {
                var searchResult = _repository.Validate(customer);
                if (searchResult.Any())
                {
                    if (searchResult.Count > 1)
                    {
                        theResult.CustomerFound = false;
                        theResult.Errors = new List<Error>
                            {new Error() {Code = 2, Description = "Multiple matches found"}};
                        Log.Write(LogEventLevel.Information, "Customer Validation Validate: Searched Failed: {0}",
                            LogResult(theResult));
                        return theResult;
                    }

                    CustomerAccessValidationLibrary retrievedCustomer = searchResult.First();
                    if (string.IsNullOrEmpty(customer.OrganisationName) &&
                        retrievedCustomer.Surname != customer.Surname ||
                        retrievedCustomer.DateOfBirth != customer.DateOfBirth)
                    {
                        theResult.CustomerFound = false;
                        theResult.Errors = new List<Error>
                            {new Error() {Code = 5, Description = "Surname or Date of Birth supplied do not match that of those recorded against this AccessNumber"}};
                        Log.Write(LogEventLevel.Information, "Customer Validation Validate: Searched Failed: {0}",
                            LogResult(theResult));
                        return theResult;
                    }
                    else if (!string.IsNullOrEmpty(customer.OrganisationName) &&
                             retrievedCustomer.Surname != customer.OrganisationName)
                    {
                        theResult.CustomerFound = false;
                        theResult.Errors = new List<Error>
                            {new Error() {Code = 5, Description = "Organisation name supplied do not match that recorded against this AccessNumber"}};
                        Log.Write(LogEventLevel.Information, "Customer Validation Validate: Searched Failed: {0}",
                            LogResult(theResult));
                        return theResult;
                    }

                    theResult.AccessNumber = searchResult[0].AccessNumber;
                    theResult.CustomerFound = true;
                    Log.Write(LogEventLevel.Information, "Customer Validation Validate: Searched Succeeded: {0}",
                        LogResult(theResult));
                    return theResult;
                }
                theResult.CustomerFound = false;
                theResult.Errors = new List<Error>
                    {new Error() {Code = 1, Description = "No acceptable match found"}};
                Log.Write(LogEventLevel.Information, "Customer Validation Validate: Searched Failed: {0}",
                    LogResult(theResult));
                return theResult;
            }
            catch (Exception ex)
            {
                theResult.CustomerFound = false;
                theResult.Errors = new List<Error>
                    {new Error() {Code = 3, Description = "Error processing request, please try again later"}};
                Log.Error(ex, $"Customer Validation Validate: Error performing Customer Validation Validate, CorrollationId: {customer.CorollationId}",
                    LogResult(theResult));
                return theResult;
            }
        }

        public SearchResult Create(AndoCustomer customer)
        {
            SearchResult theResult = new SearchResult{ CustomerFound = false, CorollationId = customer.CorollationId, Created = false};
            if (_repository.CheckEmailAndPhoneUseage(customer) > 0)
            {
                theResult.Errors = new List<Error>();
                theResult.Errors.Add(new Error()
                    {Code = 4, Description = "Email Address or Mobile phone number is already in use"});
                Log.Write(LogEventLevel.Information,
                    "Customer Validation Search: Customer Creation Failed as Email address or Mobile phone number are already in use: {0}",
                    LogResult(theResult));
                return theResult;
            }

            try
            {
                if (!string.IsNullOrEmpty(customer.MobilePhone))
                {
                    // if mobile phone number is a number but clearly not a valid NZ mobile number then save it as a home phone contract number instead,
                    // and save nothing against the mobile phone number
                    Regex regex = new Regex(@"^02\d{7,9}$");
                    Match match = regex.Match(customer.MobilePhone);
                    if (!match.Success)
                    {
                        customer.HomePhone = customer.MobilePhone;
                        customer.MobilePhone = null;
                    }

                    Log.Write(LogEventLevel.Information,
                        $"Customer Validation Search: Customer Creation Started, CorollationId: {customer.CorollationId}");
                    var newCustomer = _createRepository.StoreCustomer(customer);
                    customer.CustomerId = newCustomer.AccessNumber;
                    _maintainRepository.CreateContactDetails(customer);
                    if (!customer.InsuredType.Equals("primary", StringComparison.CurrentCultureIgnoreCase))
                    {
                        _customerOrderRepository.SetMarketingPreferences(customer);
                    }

                    theResult.AccessNumber = newCustomer.AccessNumber;
                    theResult.CustomerFound = true;
                    theResult.Created = true;
                    Log.Write(LogEventLevel.Information, "Customer Validation Search: Customer Creation Finished: {0}",
                        LogResult(theResult));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Customer Validation Search: Error performing Customer Validation Search: During Customer Creation: {ex.Message}, CorollationId: {customer.CorollationId}");
                throw;
            }
        }
    }
}