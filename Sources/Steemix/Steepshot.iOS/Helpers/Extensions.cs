using System.Collections.Generic;
using System.Linq;
using Sweetshot.Library.Models.Responses;

namespace Steepshot.iOS
{
	public static class Extensions
	{
		public static void FilterNSFW(this List<Post> list)
		{
			if (!UserContext.Instanse.NSFW)
				list.RemoveAll(p => p.Category.Contains("nsfw") || p.Tags.Any(t => t.Contains("nsfw")));
		}

		public static void FilterHided(this List<Post> list)
		{
			if (list == null || UserContext.Instanse.CurrentAccount == null || UserContext.Instanse.CurrentAccount.Postblacklist == null || UserContext.Instanse.CurrentAccount.Postblacklist.Count == 0)
				return;
			foreach (var blackPost in UserContext.Instanse.CurrentAccount.Postblacklist)
			{
				var lil = list.FirstOrDefault(p => p.Url == blackPost);
				if (lil != null)
				{
					list.Remove(lil);
				}
			}
		}
	}
}
