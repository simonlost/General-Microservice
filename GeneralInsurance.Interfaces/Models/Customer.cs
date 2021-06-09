using System;

namespace GeneralInsurance.Interfaces.Models
{
    public class Customer
    {
        public int AccessNumber { get; set; }
        public string Title { get; set; }
        public string ForeNames { get; set; }
        public string Surname { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public double FirstNameDLDistance { get; set; }
        public double MiddleNameDLDistance { get; set; }
        public bool HasMiddleNames { get; set; }
        public int NumberOfMiddleNames { get; set; }

        public string Print(string CorollationId)
        {
            return $"Corollation ID: {CorollationId}, Access Number: {AccessNumber}, etc";
        }
    }
}