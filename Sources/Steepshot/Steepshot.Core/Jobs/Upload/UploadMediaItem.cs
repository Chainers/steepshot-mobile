using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Steepshot.Core.Jobs.Upload
{
    [Table(nameof(UploadMediaItem))]
    public class UploadMediaItem : SqlTableBase
    {
        public string FilePath { get; set; }

        public UploadState State { get; set; }

        [MaxLength(36)]
        public string Uuid { get; set; }

        public string ResultJson { get; set; }

        [ForeignKey(typeof(UploadMediaContainer))]
        public int ParentId { get; set; }

        [ManyToOne]
        public UploadMediaContainer Parent { get; set; }


        public UploadMediaItem()
        {
            //for SqlLite
        }

        public UploadMediaItem(string filePath)
        {
            FilePath = filePath;
            State = UploadState.ReadyToUpload;
        }
    }
}