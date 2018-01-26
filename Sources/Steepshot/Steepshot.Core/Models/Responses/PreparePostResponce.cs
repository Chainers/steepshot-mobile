namespace Steepshot.Core.Models.Responses
{
    public class PreparePostResponce
    {
        public string Body { get; set; }

        public string JsonMetadata { get; set; }

        public Beneficiary[] Beneficiaries { get; set; }
    }
}
