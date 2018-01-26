using System.Linq;
using NUnit.Framework;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class UnitTests : BaseTests
    {
        [Test]
        public void CreateCommentModel_Empty_Url()
        {
            var user = Users.First().Value;
            var request = new CreateCommentModel(user, string.Empty, "test", AppSettings.AppInfo);

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage == Localization.Errors.EmptyUrlField);
        }

        [Test]
        public void VoteModel_Empty_Identifier()
        {
            var user = Users.First().Value;
            var request = new VoteModel(user, VoteType.Up, string.Empty);
            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage == Localization.Errors.EmptyUrlField);
        }

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
                Media = new Media[1]
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
                Media = new Media[1],
                Tags = new string[PreparePostModel.TagLimit + 1]
            };

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage == Localization.Errors.TagLimitError);
        }
    }
}
