using System;
using System.Threading;
using Android.App;
using Android.OS;
using Android.Widget;
using CheeseBind;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Integration;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class TestActivity : BaseActivityWithPresenter<TestPresenter>
    {
        private MobileAutoTests _testContainer;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.test_results)] private TextView _testResults;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_auto_test);
            Cheeseknife.Bind(this);
            _testContainer = new MobileAutoTests(Presenter.OpenApi, AppSettings.User.UserInfo, AppSettings.AppInfo);
            _testContainer.StepFinished += UpdateResult;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        [OnClick(Resource.Id.run_api_tests)]
        private async void RunApiTest(object sender, EventArgs e)
        {
            //TODO: add cancel support
            var module = new InstagramModule(Presenter.OpenApi.Gateway, AppSettings.User);
            if (module.IsAuthorized())
            {
                module.TryCreateNewPost(CancellationToken.None);
                return;
            }
            module.AuthToInstagram(this);
            //await _testContainer.RunServerTests();
            //await _testContainer.RunDitchApiTests();
        }

        [OnClick(Resource.Id.btn_back)]
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
