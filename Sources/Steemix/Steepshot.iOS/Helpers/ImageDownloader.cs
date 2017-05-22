using System;
using System.Collections.Generic;
using System.Net;
using Foundation;
using UIKit;

namespace Steepshot.iOS
{
	public static class ImageDownloader
	{
		public static void Download(string uri, UIImageView imageView, UIImage defaultPicture = null, List<WebClient> webClients = null)
		{
			try
			{
				//if(defaultPicture != null)
					imageView.Image = defaultPicture;
				using (var webClient = new WebClient())
				{
					if(webClients != null)
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
				//Loggin     
			}
		}
	}
}
