namespace Steemix.Library.Models.Requests
{
    public class ImageUploadResponse : BaseResponse
    {
        public bool IsUploaded => string.IsNullOrEmpty(error);
    }
}