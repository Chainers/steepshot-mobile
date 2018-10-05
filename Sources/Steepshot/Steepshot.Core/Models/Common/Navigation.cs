using System.Collections.Generic;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Models.Common
{
    public sealed class Navigation
    {
        public static int SelectedTab { get; set; }

        public Dictionary<string, TabOptions> TabSettings { get; set; } = new Dictionary<string, TabOptions>();
    }

    public sealed class TabOptions
    {
        public bool IsGridView { get; set; } = false;

        public PostType PostType { get; set; } = PostType.Hot;
    }
}