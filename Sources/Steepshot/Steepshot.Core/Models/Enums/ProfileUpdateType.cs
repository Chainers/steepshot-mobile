using System;

namespace Steepshot.Core.Models.Enums
{
    [Flags]
    public enum ProfileUpdateType
    {
        None = 1,
        OnlyInfo = 2,
        OnlyPosts = 4,
        Full = OnlyInfo | OnlyPosts
    }
}
