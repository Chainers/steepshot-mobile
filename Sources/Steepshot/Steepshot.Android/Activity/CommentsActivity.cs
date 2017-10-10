using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;

namespace Steepshot.Activity
{
    [Activity(Label = "CommentsActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CommentsActivity : BaseActivityWithPresenter<CommentsPresenter>
    {
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
                if (_textInput.Text != string.Empty)
                {
                    _sendSpinner.Visibility = ViewStates.Visible;
                    _post.Visibility = ViewStates.Invisible;
                    var resp = await _presenter.TryCreateComment(_textInput.Text, _uid);
                    if (resp.Success)
                    {
                        if (_textInput != null)
                        {
                            _textInput.Text = string.Empty;
                            var errors = await _presenter.TryLoadNextComments(_uid);
                            if (errors != null)
                            {
                                _adapter?.NotifyDataSetChanged();
                                _manager?.ScrollToPosition(errors.Count - 1);
                            }
                        }
                    }
                    else
                    {
                        ShowAlert(Localization.Messages.RapidPosting, ToastLength.Short);
                    }
                    if (_sendSpinner != null)
                        _sendSpinner.Visibility = ViewStates.Invisible;
                    if (_post != null)
                        _post.Visibility = ViewStates.Visible;
                }
            }
            else
            {
                ShowAlert(GetString(Resource.String.need_login), ToastLength.Short);
            }
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_comments);
            Cheeseknife.Inject(this);

            _uid = Intent.GetStringExtra("uid");
            _manager = new LinearLayoutManager(this, LinearLayoutManager.Vertical, false) { StackFromEnd = true };
            _comments.SetLayoutManager(_manager);
            _adapter = new CommentAdapter(this, _presenter);
            if (_comments != null)
            {
                _comments.SetAdapter(_adapter);
                _spinner.Visibility = ViewStates.Gone;
                _adapter.LikeAction += FeedAdapter_LikeAction;
                _adapter.UserAction += FeedAdapter_UserAction;
            }

            var errors = await _presenter.TryLoadNextComments(_uid);
            if (errors == null)
                return;
            if (errors.Any())
                ShowAlert(errors, ToastLength.Short);
            else
                _adapter?.NotifyDataSetChanged();
        }

        void FeedAdapter_UserAction(int position)
        {
            var user = _presenter[position];
            if (user == null)
                return;
            var intent = new Intent(this, typeof(ProfileActivity));
            intent.PutExtra("ID", user.Author);
            StartActivity(intent);
        }

        async void FeedAdapter_LikeAction(int position)
        {
            try
            {
                if (BasePresenter.User.IsAuthenticated)
                {
                    var errors = await _presenter.TryVote(position);
                    if (errors == null)
                        return;

                    if (errors.Any())
                        ShowAlert(errors, ToastLength.Short);
                    else
                        _adapter?.NotifyDataSetChanged();
                }
                else
                {
                    var intent = new Intent(this, typeof(PreSignInActivity));
                    StartActivity(intent);
                }
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        protected override void CreatePresenter()
        {
            _presenter = new CommentsPresenter();
        }
    }
}
