namespace Steepshot.Core.Models.Responses
{
    public interface IFollowable
    {
        string Key { get; }

        bool HasFollowed { get; set; }
    }
}