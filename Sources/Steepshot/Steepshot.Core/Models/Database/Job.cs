using System;
using System.Threading;
using SQLite;

namespace Steepshot.Core.Models.Database
{
    [Table(nameof(Job))]
    public class Job : SqlTableBase
    {
        public string CommandId { get; set; }

        public int DataId { get; set; }

        public JobState State { get; set; }

        [Ignore]
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public DateTime LastStartTime { get; set; }


        public Job() { }

        public Job(string commandId)
        {
            CommandId = commandId;
        }
    }
}