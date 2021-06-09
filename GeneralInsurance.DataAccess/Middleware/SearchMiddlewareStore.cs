using System;
using System.Collections.Generic;
using GeneralInsurance.DataAccess.Middleware.ChannelFactory;
using System.Linq;
using GeneralInsurance.DataAccess.Entities;
using GeneralInsurance.Interfaces.Models;
using Serilog.Events;
using ILogger = Serilog.ILogger;
using customer = GeneralInsurance.Interfaces.Models.Customer;

namespace GeneralInsurance.DataAccess.Middleware
{
    public class SearchMiddlewareStore : ISearchMiddlewareStore
    {
        private readonly IWcfChannelFactory<IReportParty> _wcfChannelFactory;
        private readonly ILogger Log;

        public SearchMiddlewareStore(IWcfChannelFactory<IReportParty> wcfChannelFactory)
        {
            _wcfChannelFactory = wcfChannelFactory;
            Log = Serilog.Log.ForContext<SearchMiddlewareStore>();
        }

        public void DoHealthCheck()
        {
            // this method should throw if healthcheck fails, else return;
            // eg - attempt a call to middleware
            AndoCustomer customer = new AndoCustomer(){CorollationId = new Guid(), CustomerType = "consumer", DateOfBirth = DateTime.Parse("1950=-03-01"), EmailAddress = "Test@Test.com"};
            LocateCustomerByAdvSearchRequest theRequest = new LocateCustomerByAdvSearhcRequest();
            if (customer.CustomerId != null)
            {
                theRequest.SearchKey1 = customer.CustomerId;
            }

            if (!string.IsNullOrEmpty(customer.OrganisationName))
            {
                theRequest.Surname = customer.OrganisationName;
                theRequest.CustomerClass = CustomerClass.Business;
            }
            else
            {
                theRequest.Surname = customer.Surname;
                theRequest.CustomerClass = CustomerClass.Individual;
            }

            if (string.IsNullOrEmpty(customer.OrganisationName) && customer.DateOfBirth != null)
            {
                theRequest.DOB = customer.DateOfBirth;
            }

            theRequest.MaxRows = 10;
            theRequest.OnlyProspect = false;
            theRequest.Organisations = new Organisations[] {Organisations.Kiwibank};

            var result = _wcfChannelFactory.CreateChannel().LocateByAdvSearch(theRequest, Channel.INTERNAL, "Ando");
            if(!result.Success)
            {
                if (result.Errors != null && result.Errors.Length > 0)
                {
                    throw new Exception("Error calling LocateByAdvSearch: " + result.Errors.Forst().Message);
                }

                throw new Exception("Error calling LocateByAdvSearch: Reason unknown");
            }

        }

        public IList<Customer> Search(AndoCustomer customer)
        {
            return LocateCustomerByAdvancedSearch(customer);
        }



        public IList<Customer> Validate(AndoCustomer customer)
        {
            return LocateCustomerByAdvancedSearch(customer);
        }

        private GetCustomerDetailsResponse GetCustomerDetails(string accessNo, Guid corollationId)
        {
            try
            {
                GetCustomerDetailsRequest theRequest = new GetCustomerDetailsRequest()
                {
                    AccessNumber = accessNo,
                    SystemInforRequiredList = new[] {SystemInformation.CUSTOMER},
                    Channel = ChannelType.INTERNAL,
                    Organisations = new []{ Organisations.Kiwibank },
                    RemoteSystemDetails = "Ando",
                    UserId = "Ando"
                };

                Log.Write(LogEventLevel.Information,
                    $"Customer Validation Search: retrieval of Customer Details started at: {DateTime.UtcNow}, for AccessNumber: {accessNo}, with");
                var result = _wcfChannelFactory.CreateChannel().GetCustomerDetails(theRequest);
                Log.Write(LogEventLevel.Information, )
            }
        }

        public int CheckEmailAndPhoneUseage(AndoCustomer customer)
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.IList<customer> Search(AndoCustomer customer)
        {
            throw new NotImplementedException();
        }

        public System.Collections.IList<customer> Validate(AndoCustomer customer)
        {
            throw new NotImplementedException();
        }

        public int CheckEmailAndPhoneUseage(AndoCustomer customer)
        {
            throw new NotImplementedException();
        }
    }
}