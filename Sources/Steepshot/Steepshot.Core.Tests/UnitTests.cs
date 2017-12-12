using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Steepshot.Core.Authority;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.Core.Exceptions;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class UnitTests : BaseTests
    {
        [Test]
        public void Vote_Empty_Identifier()
        {
            var user = Users.First().Value;
            var ex = Assert.Throws<UserException>(() =>
            {
                new VoteRequest(user, VoteType.Up, string.Empty);
            });
            Assert.That(ex.ParamName, Is.EqualTo("identifier"));
        }

        [Test]
        public void Follow_Empty_Username()
        {
            var user = Users.First().Value;
            var ex = Assert.Throws<UserException>(() =>
            {
                new FollowRequest(user, FollowType.Follow, string.Empty);
            });
            Assert.That(ex.ParamName, Is.EqualTo("username"));
        }

        [Test]
        public void InfoRequest_Empty_Url()
        {
            var ex = Assert.Throws<UserException>(() =>
            {
                new InfoRequest(string.Empty);
            });
            Assert.That(ex.ParamName, Is.EqualTo("url"));
        }

        [Test]
        public void CreateComment_Empty_Url()
        {
            var user = Users.First().Value;
            var ex = Assert.Throws<UserException>(() =>
            {
                new CommentRequest(user, string.Empty, "test", AppSettings.AppInfo);
            });
            Assert.That(ex.ParamName, Is.EqualTo("url"));
        }

        [Test]
        public void Upload_Base64_Equals_ByteArray()
        {
            var user = Users.First().Value;
            // Arrange
            var file = File.ReadAllBytes(GetTestImagePath());

            // Act
            var requestArray = new UploadImageRequest(user, "cat" + DateTime.UtcNow.Ticks, file, new[] { "cat1", "cat2", "cat3", "cat4" });
            var base64 = Convert.ToBase64String(file);
            var requestBase64 = new UploadImageRequest(user, "cat" + DateTime.UtcNow.Ticks, base64, new[] { "cat1", "cat2", "cat3", "cat4" });

            // Assert
            Assert.That(requestArray.Photo, Is.EqualTo(requestBase64.Photo));
            Assert.That(requestArray.Photo.Length, Is.EqualTo(requestBase64.Photo.Length));
        }

        [Test]
        public void Upload_Empty_Title()
        {
            var user = Users.First().Value;
            var ex = Assert.Throws<UserException>(() =>
            {
                new UploadImageRequest(user, string.Empty, new byte[] { }, new[] { "cat1", "cat2", "cat3", "cat4" });
                new InfoRequest(string.Empty);
            });
            Assert.That(ex.ParamName, Is.EqualTo("title"));
        }
    }
}