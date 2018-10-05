using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Steepshot.Core.Models.Database
{
    [Table(nameof(UploadMediaContainer))]
    public class UploadMediaContainer : SqlTableBase
    {
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<UploadMediaItem> Items { get; set; }

        public KnownChains Chain { get; set; }

        public string Login { get; set; }


        public UploadMediaContainer() { }

        public UploadMediaContainer(KnownChains chain, string login)
        {
            Chain = chain;
            Login = login;
        }
    }
}