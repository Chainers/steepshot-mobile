namespace Steemix.Library.Models.Requests
{
    public class UploadImageRequest : TokenRequest
    {
        public UploadImageRequest(string token, string title, byte[] photo) : base(token)
        {
            this.title = title;
            this.photo = photo;
        }

        public string title { get; private set; }
        public byte[] photo { get; private set; }
    }
}