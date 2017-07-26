using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Com.Lilarcor.Cheeseknife;
using Android.Support.V7.Widget;
using Sweetshot.Library.Models.Responses;
using Android.Widget;
using Android.Views;

namespace Steepshot
{
    [Activity(Label = "CommentsActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CommentsActivity : BaseActivity, CommentsView
    {
        CommentsPresenter presenter;
        List<Post> posts;
        CommentAdapter Adapter;
        string uid;
        LinearLayoutManager manager;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.comments_list)] RecyclerView comments;
        [InjectView(Resource.Id.loading_spinner)] ProgressBar spinner;
        [InjectView(Resource.Id.text_input)] EditText textInput;
        [InjectView(Resource.Id.btn_post)] ImageButton post;
        [InjectView(Resource.Id.send_spinner)] ProgressBar sendSpinner;
#pragma warning restore 0649

        [InjectOnClick(Resource.Id.btn_back)]
        public void OnBack(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        [InjectOnClick(Resource.Id.btn_post)]
        public async void OnPost(object sender, EventArgs e)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                try
                {
                    if (textInput.Text != string.Empty)
                    {
                        sendSpinner.Visibility = Android.Views.ViewStates.Visible;
                        post.Visibility = Android.Views.ViewStates.Invisible;
                        var resp = await presenter.CreateComment(textInput.Text, uid);
                        if (resp?.Result != null && resp.Result.IsCreated)
                        {
                            if (textInput != null)
                            {
                                textInput.Text = string.Empty;
                                var posts = await presenter.GetComments(uid);
                                Adapter?.Reload(posts);
                                manager?.ScrollToPosition(posts.Count - 1);
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, "You post so fast. Try it later", ToastLength.Short).Show();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.SendCrash(ex, BasePresenter.User.Login, BasePresenter.AppVersion);
                    Toast.MakeText(this, "Unknown error. Try again", ToastLength.Short).Show();
                }
                if (sendSpinner != null && post != null)
                {
                    sendSpinner.Visibility = ViewStates.Invisible;
                    post.Visibility = ViewStates.Visible;
                }
            }
            else
            {
                Toast.MakeText(this, GetString(Resource.String.need_login), ToastLength.Short).Show();
            }
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_comments);
            Cheeseknife.Inject(this);

            uid = Intent.GetStringExtra("uid");
            manager = new LinearLayoutManager(this, LinearLayoutManager.Vertical, false);
            manager.StackFromEnd = true;
            comments.SetLayoutManager(manager);
            posts = await presenter.GetComments(uid);
            Adapter = new CommentAdapter(this, posts);
            if (comments != null)
            {
                comments.SetAdapter(Adapter);
                spinner.Visibility = ViewStates.Gone;
                Adapter.LikeAction += FeedAdapter_LikeAction;
                Adapter.UserAction += FeedAdapter_UserAction;
            }
        }

        void FeedAdapter_UserAction(int position)
        {
            Intent intent = new Intent(this, typeof(ProfileActivity));
            intent.PutExtra("ID", presenter.Posts[position].Author);
            StartActivity(intent);
        }

        async void FeedAdapter_LikeAction(int position)
        {
            try
            {
                if (BasePresenter.User.IsAuthenticated)
                {
                    var response = await presenter.Vote(presenter.Posts[position]);

                    if (response.Success)
                    {
                        presenter.Posts[position].Vote = !presenter.Posts[position].Vote;
                        Adapter?.NotifyDataSetChanged();
                    }
                    else
                    {
                        Toast.MakeText(this, response.Errors[0], ToastLength.Short).Show();
                    }
                }
                else
                {
                    var intent = new Intent(this, typeof(SignInActivity));
                    StartActivity(intent);
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, BasePresenter.User.Login, BasePresenter.AppVersion);
            }
        }

        protected override void CreatePresenter()
        {
            presenter = new CommentsPresenter(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }
    }
}