using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace SteemDataScraper.Services
{

    /// <summary>
    /// https://developers.steem.io/quickstart/
    /// https://www.steem.center/index.php?title=Public_Websocket_Servers
    /// https://geo.steem.pl/
    /// http://steemistry.com/nodes/
    /// </summary>
    public class NodeIndoService : BaseDbService
    {
        protected override bool SingleRun => true;

        public NodeIndoService(ILogger<NodeIndoService> logger, IConfiguration configuration)
            : base(logger, configuration)
        {
        }

        protected override async Task DoSomethingAsync(NpgsqlConnection connection, CancellationToken token)
        {
            return;

            //var isAny = await connection.IsNodeInfoExist(token);
            //if (isAny)
            //    return;

            //connection.Close();

            //var client = new HttpClient();

            //var serviceUrl = Configuration.GetConnectionString("AddressesUrl");
            //var msg = await client
            //    .GetAsync(serviceUrl, token)
            //    .ConfigureAwait(false);

            //msg.EnsureSuccessStatusCode();

            //var json = await msg.Content
            //    .ReadAsStringAsync()
            //    .ConfigureAwait(false);

            //client.Dispose();
            //client = new HttpClient
            //{
            //    Timeout = TimeSpan.FromSeconds(5)
            //};

            //var nods = new List<NodeInfo>();
            //var nodeAddresses = JsonConvert.DeserializeObject<NodeAddress[]>(json);
            //for (var i = 0; i < nodeAddresses.Length; i++)
            //{
            //    Logger.LogInformation($"Progress: {i} | {nodeAddresses.Length}");

            //    var item = nodeAddresses[i];
            //    if (!item.IsNode)
            //        continue;

            //    foreach (var node in item.Nodes)
            //    {
            //        var http = node.Value<string>("http_server_address");
            //        if (!string.IsNullOrEmpty(http))
            //        {
            //            var url = $"http://{http}";
            //            var nodeInfo = new NodeInfo(url);
            //            await TestUrl(client, nodeInfo, token);
            //            nods.Add(nodeInfo);
            //        }

            //        var https = node.Value<string>("https_server_address");
            //        if (!string.IsNullOrEmpty(https))
            //        {
            //            var url = $"https://{https}";
            //            var nodeInfo = new NodeInfo(url);
            //            await TestUrl(client, nodeInfo, token);
            //            nods.Add(nodeInfo);
            //        }
            //    }
            //}

            //connection.Open();
            //await connection.InsertNodeInfos(nods, token);
        }


        //private async Task TestUrl(HttpClient client, NodeInfo node, CancellationToken token)
        //{
        //    var start = DateTime.Now;
        //    var isSuccessStatusCode = false;
        //    try
        //    {
        //        var msg = await client
        //            .GetAsync($"{node.Url}/v1/chain/get_info", token)
        //            .ConfigureAwait(false);

        //        if (msg.IsSuccessStatusCode)
        //        {
        //            var text = await msg.Content.ReadAsStringAsync()
        //                .ConfigureAwait(false);
        //            isSuccessStatusCode = text.Contains("last_irreversible_block_num");
        //        }
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        //todo nothing
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.LogWarning(e, "TestUrl");
        //    }
        //    var end = DateTime.Now;
        //    node.Update(end - start, isSuccessStatusCode);
        //}
    }
}
