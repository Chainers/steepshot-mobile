using SQLite;

namespace Steepshot.Core.Models.Database
{
    public abstract class SqlTableBase
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
    }
}
