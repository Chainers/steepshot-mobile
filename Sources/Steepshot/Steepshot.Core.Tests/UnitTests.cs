using System.Linq;
using NUnit.Framework;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class UnitTests : BaseTests
    {
        [Test]
        public void FollowModel_Empty_Username()
        {
            var user = Users.First().Value;

            var request = new FollowModel(user, FollowType.Follow, string.Empty);

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage == Localization.Errors.EmptyUsernameField);
        }

        [Test]
        public void InfoModel_Empty_Url()
        {
            var request = new InfoModel(string.Empty);

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage == Localization.Errors.EmptyUrlField);
        }

        [Test]
        public void PreparePostModel_Empty_Title()
        {
            var user = Users.First().Value;
            var request = new PreparePostModel(user)
            {
                Media = new MediaModel[1]
            };

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage == Localization.Errors.EmptyTitleField);
        }

        [Test]
        public void PreparePostModel_Empty_Media()
        {
            var user = Users.First().Value;
            var request = new PreparePostModel(user)
            {
                Title = "title"
            };

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage == Localization.Errors.EmptyFileField);
        }

        [Test]
        public void PreparePostModel_MaxTags()
        {
            var user = Users.First().Value;
            var request = new PreparePostModel(user)
            {
                Title = "title",
                Media = new MediaModel[1],
                Tags = new string[PreparePostModel.TagLimit + 1]
            };

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage == Localization.Errors.TagLimitError);
        }
    }
}
