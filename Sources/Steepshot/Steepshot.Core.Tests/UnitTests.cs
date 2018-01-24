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
        public void Upload_Base64_Equals_ByteArray()
        {
            var user = Users.First().Value;
            // Arrange
            var file = File.ReadAllBytes(GetTestImagePath());

            // Act
            var requestArray = new UploadImageModel(user, "cat" + DateTime.UtcNow.Ticks, file, new[] { "cat1", "cat2", "cat3", "cat4" });
            var base64 = Convert.ToBase64String(file);
            var requestBase64 = new UploadImageModel(user, "cat" + DateTime.UtcNow.Ticks, base64, new[] { "cat1", "cat2", "cat3", "cat4" });

            // Assert
            Assert.That(requestArray.Photo, Is.EqualTo(requestBase64.Photo));
            Assert.That(requestArray.Photo.Length, Is.EqualTo(requestBase64.Photo.Length));
        }

        [Test]
        public void UploadImageRequest_Empty_Title()
        {
            var user = Users.First().Value;
            var request = new UploadImageModel(user, string.Empty, new byte[] { 0 }, new[] { "cat1", "cat2", "cat3", "cat4" });

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
