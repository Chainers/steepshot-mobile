namespace Steepshot.Core.Models.Responses
{
    public class PreparePostResponse
    {
        public string Body { get; set; }

        public object JsonMetadata { get; set; }

        public Beneficiary[] Beneficiaries { get; set; }
    }
}
