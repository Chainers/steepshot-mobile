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
    public sealed class TestActivity : BaseActivityWithPresenter<TestPresenter>
    {
        private MobileAutoTests _testContainer;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.test_results)] private TextView _testResults;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_auto_test);
            Cheeseknife.Inject(this);
            _testContainer = new MobileAutoTests(Presenter.OpenApi, BasePresenter.User.UserInfo, AppSettings.AppInfo);
            _testContainer.StepFinished += UpdateResult;
        }

        [InjectOnClick(Resource.Id.run_api_tests)]
        private async void RunApiTest(object sender, EventArgs e)
        {
            //TODO: add cancel support
            await _testContainer.RunServerTests();
            await _testContainer.RunDitchApiTests();
        }

        [InjectOnClick(Resource.Id.btn_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        private void UpdateResult(string text)
        {
            if (IsFinishing || IsDestroyed)
                return;

            _testResults.Text = text;
        }
    }
}
