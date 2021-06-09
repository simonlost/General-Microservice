using System.Collections.Generic;

namespace GeneralInsurance.Api.Validators
{
    public interface IValidationItemsFactory
    {
        IStringArrayComparer GetStringArrayComparer(IEnumerable<string> constantItems);
    }
    public class ValidationItemsFactory : IValidationItemsFactory
    {
        public IStringArrayComparer GetStringArrayComparer(IEnumerable<string> constantItems)
        {
            return StringArrayComparer(constantItems);
        }
    }
}