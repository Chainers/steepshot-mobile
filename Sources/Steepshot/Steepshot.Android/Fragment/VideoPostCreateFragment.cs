using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Database;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class VideoPostCreateFragment : PreviewPostCreateFragment
    {
        private readonly string _path;

        public VideoPostCreateFragment(string path) : base(new GalleryMediaModel { Path = path })
        {
            _path = path;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            InitData();
        }

        protected override void InitData()
        {
            Photos.Visibility = ViewStates.Gone;
            PreviewContainer.Visibility = ViewStates.Gone;
            VideoPreviewContainer.Visibility = ViewStates.Visible;
            VideoPreviewContainer.Radius = (int)Style.CornerRadius8;

            var margin = Style.Margin15;

            var layoutParams = new RelativeLayout.LayoutParams(Style.ScreenWidth - margin * 2, Style.ScreenWidth - margin * 2);
            layoutParams.SetMargins(margin, 0, margin, margin);
            VideoPreviewContainer.LayoutParameters = layoutParams;

            VideoPreview.MediaSource = new MediaModel
            {
                Url = _path,
                ContentType = "video",
                Size = new FrameSize(720, 720)
            };

            CheckOnSpam();
        }

        protected override async Task OnPostAsync()
        {
            Media[0].TempPath = Media[0].Path;
            Media[0].UploadState = UploadState.ReadyToUpload;

            await base.OnPostAsync();
        }
    }
}