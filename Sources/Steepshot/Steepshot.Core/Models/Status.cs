
namespace Steepshot.Core.Models
{
    public struct Status
    {
        public bool IsChanged { get; set; }
        public object ChangedObject { get; set; }

        public Status(bool isChanged)
        {
            IsChanged = isChanged;
            ChangedObject = null;
        }

        public Status(bool isChanged, object obj)
        {
            IsChanged = isChanged;
            ChangedObject = obj;
        }
    }
}
