namespace Steepshot.Core.Jobs.Upload
{
    public enum UploadState
    {
        None,
        ReadyToUpload,
        ReadyToVerify,
        ReadyToResult,
        Ready,
    }
}