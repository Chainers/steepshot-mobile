namespace Steepshot.Core.Models
{
    public struct Status
    {
        public bool IsChanged { get; set; }

        public Status(bool isChanged)
        {
            IsChanged = isChanged;
        }
    }
}
