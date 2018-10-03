namespace Steepshot.Core.Jobs
{
    public enum JobState
    {
        /// <summary>
        /// New Job
        /// </summary>
        Added,

        /// <summary>
        /// Job skipped, but can be processed later
        /// </summary>
        Skipped,

        /// <summary>
        /// Job skipped, and can`t be processed later
        /// </summary>
        Failed,

        /// <summary>
        /// Job processed successfully
        /// </summary>
        Ready
    }
}