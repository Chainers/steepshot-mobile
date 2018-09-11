using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Adapter;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PostEditFragment : PostPrepareBaseFragment
    {
        private readonly Post _editPost;
        private GalleryHorizontalAdapter _galleryAdapter;

        protected override GalleryHorizontalAdapter GalleryAdapter => _galleryAdapter ?? (_galleryAdapter = new GalleryHorizontalAdapter(_editPost));

        public PostEditFragment(Post post)
        {
            _editPost = post;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            SetEditPost();

            RatioBtn.Visibility = RotateBtn.Visibility = ViewStates.Gone;
            if (_editPost.Media.Length > 1)
            {
                Photos.Visibility = ViewStates.Visible;
                PreviewContainer.Visibility = ViewStates.Gone;
                Photos.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                Photos.SetAdapter(GalleryAdapter);
                Photos.AddItemDecoration(new ListItemDecoration((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Resources.DisplayMetrics)));
            }
            else
            {
                Photos.Visibility = ViewStates.Gone;
                PreviewContainer.Visibility = ViewStates.Visible;
                var margin = (int)BitmapUtils.DpToPixel(15, Resources);
                var previewSize = BitmapUtils.CalculateImagePreviewSize(_editPost.Media[0].Size.Width, _editPost.Media[0].Size.Height, Style.ScreenWidth - margin * 2, int.MaxValue);
                var layoutParams = new RelativeLayout.LayoutParams(previewSize.Width, previewSize.Height);
                layoutParams.SetMargins(margin, 0, margin, margin);
                PreviewContainer.LayoutParameters = layoutParams;
                Preview.CornerRadius = Style.CornerRadius5;

                var url = _editPost.Media[0].Thumbnails.Mini;
                Picasso.With(Activity).Load(url).CenterCrop()
                    .Resize(PreviewContainer.LayoutParameters.Width, PreviewContainer.LayoutParameters.Height)
                    .Into(Preview);

                Preview.Touch += PreviewOnTouch;
            }

            SearchTextChanged();
        }

        protected override async Task OnPostAsync()
        {
            Model.Media = _editPost.Media;

            Model.Title = Title.Text;
            Model.Description = Description.Text;
            Model.Tags = LocalTagsAdapter.LocalTags.ToArray();
            TryCreateOrEditPost(false);
        }

        protected void PreviewOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            DescriptionScrollContainer.OnTouchEvent(touchEventArgs.Event);
        }


        private void SetEditPost()
        {
            Model = new PreparePostModel(AppSettings.User.UserInfo, _editPost, AppSettings.AppInfo.GetModel());
            Title.Text = _editPost.Title;
            Title.SetSelection(_editPost.Title.Length);
            Description.Text = _editPost.Description;
            Description.SetSelection(_editPost.Description.Length);
            foreach (var editPostTag in _editPost.Tags)
            {
                AddTag(editPostTag);
            }
        }
    }
}
