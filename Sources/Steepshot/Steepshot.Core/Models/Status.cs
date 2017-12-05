namespace Steepshot.Core.Models
{
    public struct Status
    {
        public bool IsChanged { get; }

        public Status(bool isChanged)
        {
            IsChanged = isChanged;
        }
    }
}
