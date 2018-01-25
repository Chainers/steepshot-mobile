using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class UnitTests : BaseTests
    {
        [Test]
        public void Vote_Empty_Identifier()
        {
            var user = Users.First().Value;
            var request = new VoteModel(user, VoteType.Up, string.Empty);
            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage == Localization.Errors.EmptyUrlField);
        }

        [Test]
        public void Follow_Empty_Username()
        {
            var user = Users.First().Value;

            var request = new FollowModel(user, FollowType.Follow, string.Empty);

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage == Localization.Errors.EmptyUsernameField);
        }

        [Test]
        public void InfoRequest_Empty_Url()
        {
            var request = new InfoModel(string.Empty);

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage == Localization.Errors.EmptyUrlField);
        }

        [Test]
        public void CreateComment_Empty_Url()
        {
            var user = Users.First().Value;
            var request = new CreateCommentModel(user, string.Empty, "test", AppSettings.AppInfo);

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage == Localization.Errors.EmptyUrlField);
        }

        [Test]
        public void UploadImageRequest_Empty_Title()
        {
            var user = Users.First().Value;
            var request = new PreparePostModel(user);

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage == Localization.Errors.EmptyTitleField);
        }

        [Test]
        public void InfoRequest_Empty_Title()
        {
            var request = new InfoModel(string.Empty);

            var result = Validate(request);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorMessage == Localization.Errors.EmptyUrlField);
        }
    }
}
