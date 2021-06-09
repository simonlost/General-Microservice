using System;
using System.Dynamic;

namespace GeneralInsurance.Interfaces.Models
{
    public class AndoCustomer
    {
        public int? CustomerId { get; set; }
        public string Surname { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }

        public string EmailAddress { get; set; }
        public string CustomerType { get; set; }
        public string InsuredType { get; set; }
        public string MobilePhone { get; set; }
        public string WorkPhone { get; set; }   
        public string HomePhone { get; set; }
        public string Title { get; set; }
        public string OrganisationName { get; set; }
        public Guid CorollationId { get; set; }

        private int? _middleNameNumber;

        public int NumberOfMiddleNames
        {
            get
            {
                if (_middleNameNumber == null)
                {
                    if (!string.IsNullOrEmpty(MiddleName))
                    {
                        _middleNameNumber = MiddleName.Split(" ").Length;
                    }
                    else
                    {
                        _middleNameNumber = 0;
                    }
                }

                return (int) _middleNameNumber;
            }
        }

        public bool HasMiddleNames => !string.IsNullOrEmpty(MiddleName);

        public string Print()
        {
            return $"Corollation ID: {CorollationId}, Access Number: {CustomerId}, etc";
        }
    }
}