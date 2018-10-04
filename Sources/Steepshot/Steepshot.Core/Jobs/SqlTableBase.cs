using SQLite;

namespace Steepshot.Core.Jobs
{
    public abstract class SqlTableBase
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
    }
}
