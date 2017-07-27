using System;
using Steepshot.Core.Authority;

namespace Sweetshot.Library.Models.Requests
{
    public class SessionIdField
    {
        public string SessionId { get; set; }
    }

    public class LoginField
    {
        public string Login { get; set; }

        public string SessionId { get; set; }


        public LoginField() { }

        public LoginField(UserInfo user)
        {
            Login = user.Login;
            SessionId = user.SessionId;
        }
    }

    public class OffsetLimitFields
    {
        public string Offset { get; set; }
        public int Limit { get; set; }

        public OffsetLimitFields() { }

        public OffsetLimitFields(string offset, int limit)
        {
            Offset = offset;
            Limit = limit;
        }
    }

    public class LoginOffsetLimitFields : OffsetLimitFields
    {
        public string Login { get; set; }

        public string SessionId { get; set; }

        public LoginOffsetLimitFields() { }

        public LoginOffsetLimitFields(UserInfo user)
        {
            Login = user.Login;
            SessionId = user.SessionId;
        }

        public LoginOffsetLimitFields(string login, string sessionId)
        {
            Login = login;
            SessionId = sessionId;
        }
    }

    public class LoginRequest
    {
        public string Login { get; set; }

        public string SessionId { get; set; }

        public string PostingKey { get; set; }


        public LoginRequest(UserInfo user)
        {
            if (string.IsNullOrWhiteSpace(user.Login))
                throw new ArgumentNullException(nameof(user.Login));

            Login = user.Login;
            PostingKey = user.PostingKey;
            SessionId = user.SessionId;
        }
    }
}