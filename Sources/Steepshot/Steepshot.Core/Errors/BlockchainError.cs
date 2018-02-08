using Ditch.Core.Errors;
using Newtonsoft.Json;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Errors
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class BlockchainError : ErrorBase
    {
        public BlockchainError(LocalizationKeys key) : base(key) { }

        public BlockchainError(ResponseError responseError)
            : base(ToMessage(responseError)) { }

        private static string ToMessage(ResponseError responseError)
        {
            switch (responseError.Data.Code)
            {
                case 10: //Assert Exception
                    {
                        //if (responseError.Data.Stack.Any())
                        //{
                        //    if (responseError.Data.Stack[0].Format.Contains("STEEMIT_MAX_VOTE_CHANGES"))
                        //    {
                        //        operationResult.Error = new BlockchainError(LocalizationKeys.MaxVoteChanges);
                        //        break;
                        //    }
                        //    var match = _errorMsg.Match(responseError.Data.Stack[0].Format);
                        //    if (match.Success && !string.IsNullOrWhiteSpace(match.Value))
                        //    {
                        //        operationResult.Error = new BlockchainError(match.Value);
                        //        break;
                        //    }
                        //}
                        goto default;
                    }
                case 13: //unknown key
                    {
                        return $"{responseError.Data.Code} {responseError.Data.Name}: {responseError.Data.Message}";
                        // return nameof(LocalizationKeys.WrongPrivatePostingKey);
                    }
                //case 3000000: "transaction exception"
                //case 3010000: "missing required active authority"
                //case 3020000: "missing required owner authority"
                //case 3030000: "missing required posting authority"
                //case 3040000: "missing required other authority"
                //case 3050000: "irrelevant signature included"
                //case 3060000: "duplicate signature included"
                case 3030000:
                    {
                        //if (t.Name == "LoginResponse")
                        //{
                        //    operationResult.Error = new BlockchainError(LocalizationKeys.WrongPrivatePostingKey);
                        //    break;
                        //}
                        goto default;
                    }
                default:
                    {
                        return responseError.Message;
                        //operationResult.Error = new BlockchainError(LocalizationKeys.ServeRejectRequest, responseError.Data.Code, responseError.Data.Message);
                    }
            }
        }
    }
}