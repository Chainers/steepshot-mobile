using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class TestActivity : BaseActivityWithPresenter<TestPresenter>
    {

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.test_results)] private TextView _testResults;
#pragma warning restore 0649
        private MobileAutoTests _testContainer;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_auto_test);
            Cheeseknife.Inject(this);
            _testContainer = new MobileAutoTests(_presenter.OpenApi, BasePresenter.User.UserInfo, AppSettings.AppInfo);
            _testContainer.StepFinished += UpdateResult;
        }

        [InjectOnClick(Resource.Id.run_api_tests)]
        private async void RunApiTest(object sender, EventArgs e)
        {
            await _testContainer.RunServerTests();
            await _testContainer.RunDitchApiTests();
        }

        private void UpdateResult(string text)
        {
            RunOnUiThread(() =>
            {
                _testResults.Text = text;
            });
        }

        [InjectOnClick(Resource.Id.btn_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        protected override void CreatePresenter()
        {
            _presenter = new TestPresenter();
        }
    }
}
