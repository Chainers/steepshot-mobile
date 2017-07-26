using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Foundation;
using Sweetshot.Library.Models.Requests;
using UIKit;

namespace Steepshot.iOS
{
	public partial class DescriptionViewController : BaseViewController
	{
		protected DescriptionViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public DescriptionViewController()
		{
		}

		public UIImage ImageAsset;

		private TagsCollectionViewSource collectionviewSource;

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			photoView.Image = ImageAsset;
			postPhotoButton.TouchDown += (sender, e) => PostPhoto();
			activeview = descriptionTextField;
			//Collection view initialization
			tagsCollectionView.RegisterClassForCell(typeof(TagCollectionViewCell), nameof(TagCollectionViewCell));
			tagsCollectionView.RegisterNibForCell(UINib.FromName(nameof(TagCollectionViewCell), NSBundle.MainBundle), nameof(TagCollectionViewCell));
			// research flow layout
			/*tagsCollectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
            {
                EstimatedItemSize = new CGSize(100, 50),
                
            }, false);*/
			collectionviewSource = new TagsCollectionViewSource((sender, e) =>
			{
				var myViewController = Storyboard.InstantiateViewController(nameof(PostTagsViewController)) as PostTagsViewController;
				this.NavigationController.PushViewController(myViewController, true);
			});
			collectionviewSource.tagsCollection = new List<string>() { "" }; //UserContext.Instanse.TagsList;
			collectionviewSource.RowSelectedEvent += CollectionTagSelected;
			tagsCollectionView.Source = collectionviewSource;

			UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
				{
					descriptionTextField.ResignFirstResponder();
				});
			this.View.AddGestureRecognizer(tap);
			descriptionTextField.Layer.BorderWidth = 1;
			descriptionTextField.Layer.BorderColor = UIColor.Black.CGColor;
		}

		public override void ViewWillAppear(bool animated)
		{
			NavigationController.NavigationBarHidden = false;
			base.ViewWillAppear(animated);
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			collectionviewSource.tagsCollection.Clear();
			collectionviewSource.tagsCollection.Add("");
			collectionviewSource.tagsCollection.AddRange(UserContext.Instanse.TagsList);
			tagsCollectionView.ReloadData();
			tagsCollectionView.LayoutIfNeeded();
			collectionHeight.Constant = tagsCollectionView.ContentSize.Height;
		}

		public override void ViewDidDisappear(bool animated)
		{
			if (IsMovingFromParentViewController)
				UserContext.Instanse.TagsList.Clear();
			base.ViewDidDisappear(animated);
		}

		private async Task PostPhoto()
		{
			loadingView.Hidden = false;
			postPhotoButton.Enabled = false;

			try
			{
				byte[] photoByteArray;
				using (NSData imageData = photoView.Image.AsJPEG(0.4f))
				{
					photoByteArray = new Byte[imageData.Length];
					Marshal.Copy(imageData.Bytes, photoByteArray, 0, Convert.ToInt32(imageData.Length));
				}

				var request = new UploadImageRequest(UserContext.Instanse.Token, descriptionTextField.Text, photoByteArray, UserContext.Instanse.TagsList.ToArray());
				var imageUploadResponse = await Api.Upload(request);

				if (imageUploadResponse.Success)
				{
					UserContext.Instanse.TagsList.Clear();
					UserContext.Instanse.ShouldProfileUpdate = true;
					this.NavigationController.PopViewController(true);
				}
				else
				{
					ShowAlert(imageUploadResponse.Errors[0]);
					Reporter.SendCrash("Photo upload error: " + imageUploadResponse.Errors[0]);
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
			finally
			{
				loadingView.Hidden = true;
				postPhotoButton.Enabled = true;
			}
		}

		void CollectionTagSelected(int row)
		{
			collectionviewSource.tagsCollection.RemoveAt(row);
			UserContext.Instanse.TagsList.RemoveAt(row - 1);
			tagsCollectionView.ReloadData();
		}

		protected override void CalculateBottom()
		{
			bottom = (activeview.Frame.Y + scrollView.Frame.Y - scrollView.ContentOffset.Y + activeview.Frame.Height + offset);
		}
	}
}
