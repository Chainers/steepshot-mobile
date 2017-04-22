using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
    public partial class DescriptionViewController : BaseViewController
    {
        protected DescriptionViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public UIImage ImageAsset;
       
        private TagsCollectionViewSource collectionviewSource;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            photoView.Image = ImageAsset;
            postPhotoButton.TouchDown += (sender, e) => PostPhoto();

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
            tagsCollectionView.Source = collectionviewSource;

			UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
				{
					descriptionTextField.ResignFirstResponder();
				});
			this.View.AddGestureRecognizer(tap);

        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            collectionviewSource.tagsCollection.Clear();
            collectionviewSource.tagsCollection.Add("");
            collectionviewSource.tagsCollection.AddRange(UserContext.Instanse.TagsList);
            tagsCollectionView.ReloadData();
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
            try
            {
                using (NSData imageData = photoView.Image.AsPNG())
                {
                    Byte[] myByteArray = new Byte[imageData.Length];
                    Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));
                    var request = new UploadImageRequest(UserContext.Instanse.Token, descriptionTextField.Text, myByteArray, UserContext.Instanse.TagsList.ToArray());
                    var imageUploadResponse = await Api.Upload(request);
                    if (imageUploadResponse.Success)
                    {
                        UserContext.Instanse.TagsList.Clear();
						UserContext.Instanse.ShouldProfileUpdate = true;
                        this.NavigationController.PopViewController(true);
                    }
                    else
                    {
                        //show alert + logging
                    }
                }
            }
            catch (Exception ex)
            {
                //show alert + logging
            }
            finally
            {
                loadingView.Hidden = true;
            }
        }
    }
}

