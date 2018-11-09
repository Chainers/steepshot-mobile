using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.CustomViews;
using Steepshot.Utils;
using Steepshot.Utils.Media;

namespace Steepshot.Fragment
{
    public class PostEditFragment : PostPrepareBaseFragment
    {
        #region BindView
#pragma warning disable 0649, 4014

        [BindView(Resource.Id.photos)] protected RecyclerView Photos;
        [BindView(Resource.Id.video_preview)] protected MediaView MediaView;
        [BindView(Resource.Id.media_preview_container)] protected RoundedRelativeLayout PreviewContainer;

#pragma warning restore 0649, 4014
        #endregion

        private readonly Post _editPost;
        private MediaAdapter _galleryAdapter;

        public PostEditFragment(Post post)
        {
            _editPost = post;
        }

        public override async void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);
            _galleryAdapter = new MediaAdapter(_editPost);

            SetEditPost();

            if (_editPost.Media.Length > 1)
            {
                Photos.Visibility = ViewStates.Visible;
                Photos.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                Photos.SetAdapter(_galleryAdapter);
                Photos.AddItemDecoration(new ListItemDecoration((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Resources.DisplayMetrics)));
            }
            else
            {
                MediaView.Visibility = ViewStates.Visible;
                PreviewContainer.Visibility = ViewStates.Visible;
                PreviewContainer.Radius = Style.CornerRadius5;

                var margin = (int)MediaUtils.DpToPixel(15, Resources);
                var previewSize = MediaUtils.CalculateImagePreviewSize(_editPost.Media[0].Size.Width, _editPost.Media[0].Size.Height, Style.ScreenWidth - margin * 2, int.MaxValue);
                var layoutParams = new RelativeLayout.LayoutParams(previewSize.Width, previewSize.Height);
                layoutParams.SetMargins(margin, 0, margin, margin);
                PreviewContainer.LayoutParameters = layoutParams;

                MediaView.MediaSource = _editPost.Media[0];
            }

            await OnTagSearchQueryChanged();
        }

        protected override async Task OnPostAsync()
        {
            Model.Media = _editPost.Media;

            Model.Title = Title.Text;
            Model.Description = Description.Text;
            Model.Tags = LocalTagsAdapter.LocalTags.ToArray();
            await TryCreateOrEditPostAsync();
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

                var previewSize = MediaUtils.CalculateImagePreviewSize(_postMedia[0].Size.Width, _postMedia[0].Size.Height, maxWidth, maxHeight);

                var cardView = new CardView(parent.Context)
                {
                    LayoutParameters = new FrameLayout.LayoutParams(previewSize.Width, previewSize.Height),
                    Radius = Style.CornerRadius5
                };
                var image = new MediaView(parent.Context)
                {
                    Id = Resource.Id.photo,
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
                };
                cardView.AddView(image);
                return new MediaViewHolder(cardView);
            }

            public override int ItemCount => _postMedia.Length;
        }

        private class MediaViewHolder : RecyclerView.ViewHolder
        {
            private readonly MediaView _mediaView;
            public MediaViewHolder(View itemView) : base(itemView)
            {
                _mediaView = itemView.FindViewById<MediaView>(Resource.Id.photo);
            }

            public void Update(MediaModel model)
            {
                _mediaView.MediaSource = model;
            }
        }

        #endregion

    }
}
