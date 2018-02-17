using Android.OS;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Base;
using Android.App;

namespace Steepshot.Activity
{
    [Activity(Label = "VideoActivity")]
    public class VideoActivity : BaseActivity
    {
        public const string VideoExtraPath = "VideoExtraPath";
        private string path;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.videoPlayer)] private VideoView _videoPlayer;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_video);
            Cheeseknife.Inject(this);

            path = Intent.GetStringExtra(VideoExtraPath);
            _videoPlayer.SetVideoPath(path);
            var mediaController = new MediaController(this);
            _videoPlayer.SetMediaController(mediaController);
            mediaController.SetMediaPlayer(_videoPlayer);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }
    }
}