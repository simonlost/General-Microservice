using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneralInsurance.Api.Validators
{
    public class StringArrayComparer : IStringArrayComparer
    {
        private readonly IEnumerable<string> _arrayItemsToCompare;

        public StringArrayComparer(IEnumerable<string> arrayItemsToCompare)
        {
            _arrayItemsToCompare = arrayItemsToCompare;
        }

        public bool CheckValidity(string stringToCompare)
        {
            return !string.IsNullOrEmpty(stringToCompare) &&
                   _arrayItemsToCompare.Any(a => a.Equals(stringToCompare.Trim(), StringComparison.CurrentCultureIgnoreCase));
        }
    }
}