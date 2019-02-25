using System;
using System.Threading;
using System.Threading.Tasks;
using AuthServer.Models;
using Ditch.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AuthServer.Services
{
    public class ApiProvider
    {
        private readonly Ditch.Steem.OperationManager _steem;
        private int _connectingToSteem = 0;

        private readonly Ditch.Golos.OperationManager _golos;
        private int _connectingToGolos = 0;

        private readonly IConfiguration _configuration;


        public ApiProvider(IConfiguration configuration, HttpManager httpManager, WebSocketManager webSocketManager)
        {
            _steem = new Ditch.Steem.OperationManager(httpManager);
            _golos = new Ditch.Golos.OperationManager(webSocketManager);
            _configuration = configuration;
        }


        public async Task<bool> AuthorizeAsync(AuthModel model, CancellationToken token)
        {
            switch (model.AuthType)
            {
                case AuthType.Steem:
                    {
                        if (!await TryConnectToSteem(token))
                            throw new Exception("Enable connect to node");

                        var trx = JsonConvert.DeserializeObject<Ditch.Steem.Models.SignedTransaction>(model.Args, _steem.CondenserJsonSerializerSettings);
                        if (DateTime.Now > trx.Expiration)
                            return false;

                        var result = await _steem.CondenserCustomGetRequestAsync<Ditch.Steem.Models.VerifyAuthorityReturn>(Ditch.Steem.KnownApiNames.CondenserApi, "verify_authority", new[] { trx }, CancellationToken.None);
                        var isValid = !result.IsError && result.Result.Valid;

                        if (isValid)
                        {
                            Ditch.Steem.Operations.BaseOperation op = trx.Operations[0];
                            model.Login = ((Ditch.Steem.Operations.CustomJsonOperation)op).RequiredPostingAuths[0];
                        }

                        return isValid;
                    }
                case AuthType.Golos:
                    {
                        if (!await TryConnectToGolos(token))
                            throw new Exception("Enable connect to node");

                        var trx = JsonConvert.DeserializeObject<Ditch.Golos.Models.SignedTransaction>(model.Args, _golos.JsonSerializerSettings);
                        if (DateTime.Now > trx.Expiration)
                            return false;

                        var result = await _golos.CustomGetRequestAsync<bool>(Ditch.Golos.KnownApiNames.DatabaseApi, "verify_authority", new[] { trx }, CancellationToken.None);
                        var isValid = !result.IsError && result.Result;

                        if (isValid)
                        {
                            Ditch.Golos.Operations.BaseOperation op = trx.Operations[0];
                            model.Login = ((Ditch.Golos.Operations.CustomJsonOperation)op).RequiredPostingAuths[0];
                        }

                        return isValid;
                    }
            }

            return false;
        }


        public async Task<bool> TryConnectToSteem(CancellationToken token)
        {
            do
            {
                if (_steem.IsConnected)
                    return true;

                if (Interlocked.CompareExchange(ref _connectingToSteem, 1, 0) == 0)
                {
                    var isConnected = false;
                    var urls = _configuration.GetSection("SteemUrl").Get<string[]>();

                    foreach (var url in urls)
                    {
                        try
                        {
                            isConnected = await _steem.ConnectToAsync(url, token);
                            if (isConnected)
                                break;
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                            //todo nothing
                        }
                        catch (Exception)
                        {
                            //todo nothing
                        }
                    }

                    _connectingToSteem = 0;
                    return isConnected;
                }

                await Task.Delay(100, token);

            } while (true);
        }

        public async Task<bool> TryConnectToGolos(CancellationToken token)
        {
            do
            {
                if (_golos.IsConnected)
                    return true;

                if (Interlocked.CompareExchange(ref _connectingToGolos, 1, 0) == 0)
                {
                    var isConnected = false;
                    var urls = _configuration.GetSection("GolosUrl").Get<string[]>();

                    foreach (var url in urls)
                    {
                        try
                        {
                            isConnected = await _golos.ConnectToAsync(url, token);
                            if (isConnected)
                                break;
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                            //todo nothing
                        }
                        catch (Exception)
                        {
                            //todo nothing
                        }
                    }

                    _connectingToGolos = 0;
                    return isConnected;
                }

                await Task.Delay(100, token);

            } while (true);
        }
    }
}
