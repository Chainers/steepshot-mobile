using System;
using Android.App;
using Android.OS;
using Com.Lilarcor.Cheeseknife;
using Steemix.Droid.ViewModels;

namespace Steemix.Droid.Activities
{
    [Activity]
    public class TermsOfServiceActivity : BaseActivity<SettingsViewModel>
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_terms_of_service);
            Cheeseknife.Inject(this);}

        [InjectOnClick(Resource.Id.go_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            Finish();
        }
    }
}