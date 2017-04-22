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
			LoadImage(url, photoImg, null);
		}

		public void LoadImage(string uri, UIImageView imageView, UIImage defaultPicture)
		{
			try
			{
				imageView.Image = defaultPicture;
				using (var webClient = new WebClient())
				{
					webClients.Add(webClient);
					webClient.DownloadDataCompleted += (sender, e) =>
					{
						try
						{
							using (var data = NSData.FromArray(e.Result))
								imageView.Image = UIImage.LoadFromData(data);

							/*string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                            string localFilename = "downloaded.png";
                            string localPath = Path.Combine(documentsPath, localFilename);
                            File.WriteAllBytes(localPath, bytes); // writes to local storage*/
						}
						catch (Exception ex)
						{
							//Logging
						}
					};
					webClient.DownloadDataAsync(new Uri(uri));
				}
			}
			catch (Exception ex)
			{
				//Logging
			}
		}
   }
}
