
namespace Steepshot.Core.Models.Responses
{
    //POST /api/v1_1/media/upload
    //input:
    //  trx - подписанная транзакция
    //  file - сам файл
    //  generate_thumbnail - bool, default = true
    //output:
    //  {
    //    "url": "https://steepshot.org/image/91c295c7-87c1-4e2f-b704-e7a7367be331.jpeg",
    //    "ipfs_hash": "QmWoneyLm6wfgr55STuC8rnDzeCWQpwH38v2jyiWocK2Kq",
    //    "size": {
    //        "width": 460,
    //        "height": 291
    //    }
    //  }
    public class UploadMediaResponse
    {
        public string Url { get; set; }

        public string IpfsHash { get; set; }

        public FrameSize Size { get; set; }
    }
}
