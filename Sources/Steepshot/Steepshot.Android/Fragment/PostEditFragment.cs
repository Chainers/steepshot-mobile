using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PostEditFragment : PostPrepareBaseFragment
    {
        private readonly Post _editPost;
        private MediaAdapter _galleryAdapter;

        public PostEditFragment(Post post)
        {
            _editPost = post;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);
            _galleryAdapter = new MediaAdapter(_editPost);

            SetEditPost();

            RatioBtn.Visibility = RotateBtn.Visibility = ViewStates.Gone;
            if (_editPost.Media.Length > 1)
            {
                Photos.Visibility = ViewStates.Visible;
                PreviewContainer.Visibility = ViewStates.Gone;
                Photos.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                Photos.SetAdapter(_galleryAdapter);
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

            SearchTextChangedAsync();
        }

        protected override async Task OnPostAsync()
        {
            Model.Media = _editPost.Media;

            Model.Title = Title.Text;
            Model.Description = Description.Text;
            Model.Tags = LocalTagsAdapter.LocalTags.ToArray();
            TryCreateOrEditPost();
        }

        protected void PreviewOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            DescriptionScrollContainer.OnTouchEvent(touchEventArgs.Event);
        }


        private void SetEditPost()
        {
            Model = new PreparePostModel(App.User.UserInfo, _editPost, App.AppInfo.GetModel());
            Title.Text = _editPost.Title;
            Title.SetSelection(_editPost.Title.Length);
            Description.Text = _editPost.Description;
            Description.SetSelection(_editPost.Description.Length);
            foreach (var editPostTag in _editPost.Tags)
            {
                AddTag(editPostTag);
            }
        }

        public override void OnDetach()
        {
            Photos.SetAdapter(null);
            base.OnDetach();
        }

        #region Adapter

        private class MediaAdapter : RecyclerView.Adapter
        {
            private readonly MediaModel[] _postMedia;

            public MediaAdapter(Post post)
            {
                _postMedia = post.Media;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var galleryHolder = (MediaViewHolder)holder;
                galleryHolder?.Update(_postMedia[position]);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var maxWidth = Style.GalleryHorizontalScreenWidth;
                var maxHeight = Style.GalleryHorizontalHeight;

                var previewSize = BitmapUtils.CalculateImagePreviewSize(_postMedia[0].Size.Width, _postMedia[0].Size.Height, maxWidth, maxHeight);

                var cardView = new CardView(parent.Context)
                {
                    LayoutParameters = new FrameLayout.LayoutParams(previewSize.Width, previewSize.Height),
                    Radius = BitmapUtils.DpToPixel(5, parent.Resources)
                };
                var image = new ImageView(parent.Context)
                {
                    Id = Resource.Id.photo,
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
                };
                image.SetScaleType(ImageView.ScaleType.FitXy);
                cardView.AddView(image);
                return new MediaViewHolder(cardView);
            }

            public override int ItemCount => _postMedia.Length;
        }

        private class MediaViewHolder : RecyclerView.ViewHolder
        {
            private readonly ImageView _image;
            public MediaViewHolder(View itemView) : base(itemView)
            {
                _image = itemView.FindViewById<ImageView>(Resource.Id.photo);
            }

            public void Update(MediaModel model)
            {
                var url = model.Thumbnails.Micro;
                Picasso.With(ItemView.Context).Load(url).Into(_image);
            }
        }        

        #endregion

    }
}
