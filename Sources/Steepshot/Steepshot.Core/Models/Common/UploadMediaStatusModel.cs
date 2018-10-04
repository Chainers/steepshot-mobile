using Newtonsoft.Json;

namespace Steepshot.Core.Models.Common
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UploadMediaStatusModel
    {
        [JsonProperty("code")]
        public UploadMediaCode Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public enum UploadMediaCode
    {
        /// <summary>
        /// File is accepted
        /// </summary>
        Accepted = 0,

        /// <summary>
        /// File is processed
        /// </summary>
        Processed = 2,

        /// <summary>
        /// File is uploaded to the storage
        /// </summary>
        UploadedToStorage = 4,

        /// <summary>
        /// Done
        /// </summary>
        Done = 8,

        /// <summary>
        /// Failed to process file
        /// </summary>
        FailedToProcess = 16,

        /// <summary>
        /// Failed to upload file to the storage
        /// </summary>
        FailedToUpload = 18,

        /// <summary>
        /// Failed to save result
        /// </summary>
        FailedToSave = 20
    }

}
