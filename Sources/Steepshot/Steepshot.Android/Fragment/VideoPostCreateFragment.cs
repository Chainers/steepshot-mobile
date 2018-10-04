using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Steepshot.Core.Models.Common;

namespace Steepshot.Fragment
{
    public class VideoPostCreateFragment : PostPrepareBaseFragment
    {
        private string _path;

        public VideoPostCreateFragment(string path)
        {
            _path = path;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            _photos.Visibility = ViewStates.Gone;
            _preview.Visibility = ViewStates.Gone;
            _videoPreview.Visibility = ViewStates.Visible;

            _videoPreview.MediaSource = new MediaModel
            {
                Url = _path,
                ContentType = "video",
                Size = new FrameSize(720, 720)
            };
        }

        protected override Task OnPostAsync()
        {
            return null;
        }
    }
}