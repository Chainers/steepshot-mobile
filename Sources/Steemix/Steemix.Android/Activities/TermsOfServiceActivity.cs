using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steemix.Droid.ViewModels;

namespace Steemix.Droid.Activities
{
    [Activity(NoHistory = true)]
    public class TermsOfServiceActivity : BaseActivity<TermsOfServiceViewModel>
    {
        private TextView _termsOfService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_terms_of_service);
            Cheeseknife.Inject(this);

            _termsOfService = FindViewById<TextView>(Resource.Id.tv_terms_of_service);
            LoadText();
        }

        private async void LoadText()
        {
            var info = await ViewModelLocator.Api.TermsOfService();
            if (info.Success && !string.IsNullOrEmpty(info.Result.Text))
            {
                _termsOfService.Text = info.Result.Text;
            }
        }

        [InjectOnClick(Resource.Id.go_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            Finish();
        }
    }
}