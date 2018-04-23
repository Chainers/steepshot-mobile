using System.Linq;
using NUnit.Framework;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class ValidateTests : BaseTests
    {
        [Test]
        public void FollowModel_Empty_Username()
        {
            var user = Users.First().Value;

            var request = new FollowModel(user, FollowType.Follow, string.Empty);

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage.Equals(nameof(LocalizationKeys.EmptyUsernameField)));
        }

        [Test]
        public void InfoModel_Empty_Url()
        {
            var request = new InfoModel(string.Empty);

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage.Equals(nameof(LocalizationKeys.EmptyUrlField)));
        }

        [Test]
        public void PreparePostModel_Empty_Title()
        {
            var user = Users.First().Value;
            var request = new PreparePostModel(user, AppSettings.AppInfo.GetModel())
            {
                Media = new MediaModel[1]
            };

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage.Equals(nameof(LocalizationKeys.EmptyTitleField)));
        }

        [Test]
        public void PreparePostModel_Empty_Media()
        {
            var user = Users.First().Value;
            var request = new PreparePostModel(user, AppSettings.AppInfo.GetModel())
            {
                Title = "title"
            };

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage.Equals(nameof(LocalizationKeys.EmptyFileField)));
        }

        [Test]
        public void PreparePostModel_MaxTags()
        {
            var user = Users.First().Value;
            var tags = new string[PreparePostModel.TagLimit + 1];
            for (int i = 0; i < tags.Length; i++)
                tags[i] = "tag_" + i;

            var request = new PreparePostModel(user, AppSettings.AppInfo.GetModel())
            {
                Title = "title",
                Media = new MediaModel[1],
                Tags = tags
            };

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage.Equals(nameof(LocalizationKeys.TagLimitError)));
        }
    }
}
