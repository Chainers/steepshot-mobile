using System.Net;
using Newtonsoft.Json;
using Steepshot.Core.Localization;
using System.Text.RegularExpressions;
using Ditch.Core.Errors;
using System;

namespace Steepshot.Core.Errors
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ServerError : ErrorBase
    {
        private static readonly Regex ErrorHtml = new Regex(@"<[^>]+>");
        private static readonly Regex ErrorJson = new Regex("(?<=^{\"[a-z_0-9]*\":\\[\").*(?=\"]}$)");
        private static readonly Regex ErrorJson2 = new Regex("(?<=^{\"[a-z_0-9]*\":\").*(?=\"}$)");


        public ServerError(ErrorInfo error)
        { }

        public ServerError(HttpStatusCode responseStatusCode, string content)
            : base(ToMessage(responseStatusCode, content)) { }


        private static string ToMessage(HttpStatusCode responseStatusCode, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return nameof(LocalizationKeys.EmptyResponseContent);

            if (ErrorHtml.IsMatch(content))
                return content;

            var match = ErrorJson.Match(content);
            if (match.Success)
                return match.Value.Replace("\",\"", Environment.NewLine);

            match = ErrorJson2.Match(content);
            if (match.Success)
                return match.Value;

            return nameof(LocalizationKeys.UnexpectedError);
        }
    }
}