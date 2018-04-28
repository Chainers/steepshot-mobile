using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Adapter;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PostEditFragment : PostPrepareBaseFragment
    {
        private readonly Post _editPost;
        private GalleryHorizontalAdapter GalleryAdapter => _galleryAdapter ?? (_galleryAdapter = new GalleryHorizontalAdapter(_editPost));

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

            _ratioBtn.Visibility = _rotateBtn.Visibility = ViewStates.Gone;
            if (_editPost.Media.Length > 1)
            {
                _photos.Visibility = ViewStates.Visible;
                _previewContainer.Visibility = ViewStates.Gone;
                _photos.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                _photos.SetAdapter(GalleryAdapter);
                _photos.AddItemDecoration(new ListItemDecoration((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Resources.DisplayMetrics)));
            }
            else
            {
                _photos.Visibility = ViewStates.Gone;
                _previewContainer.Visibility = ViewStates.Visible;
                var margin = (int)BitmapUtils.DpToPixel(15, Resources);
                var layoutParams = new RelativeLayout.LayoutParams(Resources.DisplayMetrics.WidthPixels - margin * 2, Resources.DisplayMetrics.WidthPixels - margin * 2);
                layoutParams.SetMargins(margin, 0, margin, margin);
                _previewContainer.LayoutParameters = layoutParams;
                _preview.CornerRadius = BitmapUtils.DpToPixel(5, Resources);

                var url = _editPost.Media[0].Thumbnails.Mini;
                Picasso.With(Activity).Load(url).CenterCrop()
                    .Resize(_previewContainer.LayoutParameters.Width, _previewContainer.LayoutParameters.Height)
                    .Into(_preview);

                _preview.Touch += PreviewOnTouch;
            }

            SearchTextChanged();
        }

        protected override async Task OnPostAsync()
        {
            var isConnected = BasePresenter.ConnectionService.IsConnectionAvailable();

            if (!isConnected)
            {
                Activity.ShowAlert(LocalizationKeys.InternetUnavailable);
                EnabledPost();
                return;
            }

            if (string.IsNullOrEmpty(_title.Text))
            {
                Activity.ShowAlert(LocalizationKeys.EmptyTitleField, ToastLength.Long);
                EnabledPost();
                return;
            }

            _model.Media = _editPost.Media;

            _model.Title = _title.Text;
            _model.Description = _description.Text;
            _model.Tags = _localTagsAdapter.LocalTags.ToArray();
            TryCreateOrEditPost();
        }

        protected void PreviewOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            _descriptionScrollContainer.OnTouchEvent(touchEventArgs.Event);
        }


        private void SetEditPost()
        {
            _model = new PreparePostModel(AppSettings.User.UserInfo, _editPost, AppSettings.AppInfo.GetModel());
            _title.Text = _editPost.Title;
            _title.SetSelection(_editPost.Title.Length);
            _description.Text = _editPost.Description;
            _description.SetSelection(_editPost.Description.Length);
            foreach (var editPostTag in _editPost.Tags)
            {
                AddTag(editPostTag);
            }
        }
    }
}
