using System;
using System.Collections.Generic;
using System.Net;
using Foundation;
using Photos;
using UIKit;

namespace Steepshot.iOS
{
    public partial class PhotoCollectionViewCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString("PhotoCollectionViewCell");
        public static readonly UINib Nib;
        public PHAsset Asset;
		public UIImage Image {
			get {
				return photoImg.Image;
			}
		}
		private List<WebClient> webClients = new List<WebClient>();

        static PhotoCollectionViewCell()
        {
            Nib = UINib.FromName("PhotoCollectionViewCell", NSBundle.MainBundle);
        }

        protected PhotoCollectionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public void UpdateImage(UIImage photo, PHAsset asset)
        {
            photoImg.Image = photo;
            Asset = asset;
        }

		public void UpdateImage(string url)
		{
			foreach (var webClient in webClients)
            {
                if (webClient != null)
                {
                    webClient.CancelAsync();
                    webClient.Dispose();
                }
            }
			ImageDownloader.Download(url, photoImg, null, webClients);
		}
   }
}
