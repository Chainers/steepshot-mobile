using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Steemix.Library.Models.Requests;
using System.Threading.Tasks;
using Steemix.Library.Models.Responses;
using Android.Content.PM;

namespace Steemix.Android.Activity
{
    [Activity(Label = "SteepShot", MainLauncher=true,Icon = "@mipmap/ic_launcher",ScreenOrientation = ScreenOrientation.Portrait)]
    public class FeedActivity : BaseActivity, View.IOnScrollChangeListener
	{
        RecyclerView FeedList;
        ProgressBar Bar;
        Adapter.FeedAdapter FeedAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_feed);

            FeedList = FindViewById<RecyclerView>(Resource.Id.feed_list);
            Bar = FindViewById<ProgressBar>(Resource.Id.loading_spinner);
            FeedList.SetLayoutManager(new LinearLayoutManager(this));
            var follow = FindViewById<TextView>(Resource.Id.Title);
            follow.Clickable = true;

            follow.Click += (sender, e) => {
                StartActivity(typeof(SignInActivity));
            };

            FeedAdapter = new Adapter.FeedAdapter(this);
            FeedList.SetAdapter(FeedAdapter);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            GetPosts(string.Empty);
            FeedList.SetOnScrollChangeListener(this);
        }

        public void GetPosts(string offset)
        {
            Task<UserPostResponse>.Factory.StartNew(() =>
            {
               var request = new TopPostRequest(offset, 10);
               var response = ApiClient.GetTopPosts(request);
               return response;
            }).ContinueWith((arg) => {
                RunOnUiThread(() =>
                {
                    Bar.Visibility = ViewStates.Gone;
					if (arg.Status == TaskStatus.Faulted)
                    {
                        ShowAlert("Posts not loaded. Try again");
                    }
                    else
                    {
                        FeedAdapter.AddPosts(arg.Result.Results);
                    }
                });
            });
        }

        public void ShowAlert(string message)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Error");
            alert.SetMessage(message);
            alert.SetPositiveButton("Ok", ((senderAlert, args) => { }));
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        int prevPos=0;
        public void OnScrollChange(View v, int scrollX, int scrollY, int oldScrollX, int oldScrollY)
        {
           int pos = ((LinearLayoutManager)FeedList.GetLayoutManager()).FindLastCompletelyVisibleItemPosition();
            if (pos > prevPos && pos != prevPos)
            {
                if (pos == FeedList.GetAdapter().ItemCount - 1)
                {
                    if (pos < FeedAdapter.ItemCount)
                    {
                        GetPosts(FeedAdapter.GetItem(FeedAdapter.ItemCount-1).Url);
                        prevPos = pos;
                    }
                }
            }
        }
    }
}