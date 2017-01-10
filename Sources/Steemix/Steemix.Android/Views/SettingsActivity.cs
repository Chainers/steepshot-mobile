using Android.App;
using Android.OS;
using Android.Widget;
using Steemix.Droid.Activity;
using Square.Picasso;

namespace Steemix.Droid.Views
{
    [Activity(NoHistory = true)]
    public class SettingsActivity : BaseActivity<SignInViewModel>
    {
        private ImageView Avatar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_settings);
            Avatar = FindViewById<ImageView>(Resource.Id.avatar);

            //Picasso.With(context).Load(Posts[position].Body).Into(vh.Photo);

            //Picasso.With(mContext)
            //    .load(com.app.utility.Constants.BASE_URL + b.image)
            //    .placeholder(R.drawable.profile)
            //    .error(R.drawable.profile)
            //    .transform(new RoundedTransformation(50, 4))
            //    .resizeDimen(R.dimen.list_detail_image_size, R.dimen.list_detail_image_size)
            //    .centerCrop()
            //    .into(v.im_user);

        }
    }
}