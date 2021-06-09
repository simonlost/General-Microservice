namespace GeneralInsurance.Api.Models
{
    public class ValidatorOutput
    {
        public bool IsValid { get; set; }
        public string ValidationMessages { get; set; }
        public string FieldName { get; set; }
    }
}