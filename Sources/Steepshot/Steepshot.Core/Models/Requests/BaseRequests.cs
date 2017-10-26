using Steepshot.Core.Exceptions;

namespace Steepshot.Core.Models.Requests
{
    public class NamedRequest
    {
        public string Login { get; set; }
    }

    public class OffsetLimitFields
    {
        public const int ServerMaxCount = 20;

        public string Offset { get; set; } = string.Empty;
        public int Limit { get; set; } = 10;
    }

    public class NamedRequestWithOffsetLimitFields : NamedRequest
    {
        public string Offset { get; set; }
        public int Limit { get; set; }
    }

    public class NamedInfoRequest : CensoredNamedRequestWithOffsetLimitFields
    {
        public NamedInfoRequest(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new UserException(Localization.Errors.EmptyUrlField);

            Url = url;
        }

        public string Url { get; }
    }

    public class InfoRequest : NamedRequestWithOffsetLimitFields
    {
        public InfoRequest(string url) : base()
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new UserException(Localization.Errors.EmptyUrlField);
            Url = url;
        }

        public string Url { get; }
    }
}
