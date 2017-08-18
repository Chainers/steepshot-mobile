using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;

namespace Steepshot.Activity
{
    [Activity(Label = "CommentsActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CommentsActivity : BaseActivity
    {
        CommentsPresenter _presenter;
        List<Post> _posts;
        CommentAdapter _adapter;
        string _uid;
        LinearLayoutManager _manager;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.comments_list)] RecyclerView _comments;
        [InjectView(Resource.Id.loading_spinner)] ProgressBar _spinner;
        [InjectView(Resource.Id.text_input)] EditText _textInput;
        [InjectView(Resource.Id.btn_post)] ImageButton _post;
        [InjectView(Resource.Id.send_spinner)] ProgressBar _sendSpinner;
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
                    if (_textInput.Text != string.Empty)
                    {
                        _sendSpinner.Visibility = ViewStates.Visible;
                        _post.Visibility = ViewStates.Invisible;
                        var resp = await _presenter.CreateComment(_textInput.Text, _uid);
                        if (resp.Success)
                        {
                            if (_textInput != null)
                            {
                                _textInput.Text = string.Empty;
                                var posts = await _presenter.GetComments(_uid);
                                _adapter?.Reload(posts);
                                _manager?.ScrollToPosition(posts.Count - 1);
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
                if (_sendSpinner != null && _post != null)
                {
                    _sendSpinner.Visibility = ViewStates.Invisible;
                    _post.Visibility = ViewStates.Visible;
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

            _uid = Intent.GetStringExtra("uid");
            _manager = new LinearLayoutManager(this, LinearLayoutManager.Vertical, false);
            _manager.StackFromEnd = true;
            _comments.SetLayoutManager(_manager);
            _posts = await _presenter.GetComments(_uid);
            _adapter = new CommentAdapter(this, _posts);
            if (_comments != null)
            {
                _comments.SetAdapter(_adapter);
                _spinner.Visibility = ViewStates.Gone;
                _adapter.LikeAction += FeedAdapter_LikeAction;
                _adapter.UserAction += FeedAdapter_UserAction;
            }
        }

        void FeedAdapter_UserAction(int position)
        {
            var intent = new Intent(this, typeof(ProfileActivity));
            intent.PutExtra("ID", _presenter.Posts[position].Author);
            StartActivity(intent);
        }

        async void FeedAdapter_LikeAction(int position)
        {
            try
            {
                if (BasePresenter.User.IsAuthenticated)
                {
                    var response = await _presenter.Vote(_presenter.Posts[position]);

                    if (response.Success)
                    {
                        _presenter.Posts[position].Vote = !_presenter.Posts[position].Vote;
                        _adapter?.NotifyDataSetChanged();
                    }
                    else
                    {
                        Toast.MakeText(this, response.Errors[0], ToastLength.Short).Show();
                    }
                }
                else
                {
                    var intent = new Intent(this, typeof(PreSignInActivity));
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
            _presenter = new CommentsPresenter();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }
    }
}