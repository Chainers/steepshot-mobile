using Android.App;
using Android.OS;
using Com.Lilarcor.Cheeseknife;
using Square.Picasso;
using Steepshot.Presenter;

namespace Steepshot
{
    [Activity(Label = "PostPreviewActivity", ScreenOrientation =Android.Content.PM.ScreenOrientation.Portrait)]
    public class PostPreviewActivity : BaseActivity, PostPreviewView
    {
        PostPreviewPresenter presenter;
        protected override void CreatePresenter()
        {
            presenter = new PostPreviewPresenter(this);
        }

        [InjectView(Resource.Id.photo)]
        ScaleImageView photo;

        string path;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_post_preview);
            Cheeseknife.Inject(this);

            path = Intent.GetStringExtra("PhotoURL");

            Picasso.With(this).Load(path).Into(photo);
        }
    }
}