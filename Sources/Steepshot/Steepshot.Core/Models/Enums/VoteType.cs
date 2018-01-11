using System.ComponentModel.DataAnnotations;

namespace Steepshot.Core.Models.Enums
{
    public enum VoteType
    {
        [Display(Description = "upvote")]
        Up,

        [Display(Description = "downvote")]
        Down,

        [Display(Description = "flag")]
        Flag
    }
}