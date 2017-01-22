using System;
using NUnit.Framework;
using Sweetshot.Library.Models.Requests;

namespace Sweetshot.Tests
{
    [TestFixture]
    public class UnitTests
    {
        [Test]
        public void Vote_Empty_Identifier()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                var r = new VoteRequest("sessionId", true, "");
            });
            Assert.That(ex.ParamName, Is.EqualTo("identifier"));
        }

        [Test]
        public void Follow_Empty_Username()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                var r = new FollowRequest("sessionId", FollowType.Follow, "");
            });
            Assert.That(ex.ParamName, Is.EqualTo("username"));
        }

        [Test]
        public void Comments_Empty_Url()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                var r = new GetCommentsRequest("");
            });
            Assert.That(ex.ParamName, Is.EqualTo("url"));
        }

        [Test]
        public void CreateComment_Empty_Url()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                var r = new CreateCommentRequest("sessionId", "", "test", "test");
            });
            Assert.That(ex.ParamName, Is.EqualTo("url"));
        }
    }
}