﻿using System;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Models;
using Steepshot.Activity;
using Android.Content;

namespace Steepshot.Fragment
{
    public class CommentsFragment : BaseFragmentWithPresenter<CommentsPresenter>
    {
        public const string PostExtraPath = "uid";
        public const int RequestCode = 124;
        public const string ResultString = "result";
        public const string CountString = "count";

        private CommentAdapter _adapter;
        private string _uid;
        private LinearLayoutManager _manager;
        private int _counter = 0;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.comments_list)] private RecyclerView _comments;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _spinner;
        [InjectView(Resource.Id.text_input)] private EditText _textInput;
        [InjectView(Resource.Id.btn_post)] private RelativeLayout _post;
        [InjectView(Resource.Id.btn_back)] private ImageButton _backButton;
        [InjectView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [InjectView(Resource.Id.btn_settings)] private ImageButton _settings;
        [InjectView(Resource.Id.profile_login)] private TextView _viewTitle;
        [InjectView(Resource.Id.send_spinner)] private ProgressBar _sendSpinner;
        [InjectView(Resource.Id.btn_post_image)] private ImageView _postImage;
        [InjectView(Resource.Id.message)] private RelativeLayout _messagePanel;
#pragma warning restore 0649

        public CommentsFragment()
        {

        }

        public CommentsFragment(string uid)
        {
            _uid = uid;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_comments, null);
                Cheeseknife.Inject(this, InflatedView);
            }
            ToggleTabBar(true);
            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            _textInput.Typeface = Style.Regular;
            _viewTitle.Typeface = Style.Semibold;
            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += OnBack;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = Localization.Messages.PostComments;

            _post.Click += OnPost;

            _manager = new LinearLayoutManager(Context, LinearLayoutManager.Vertical, false);

            Presenter.SourceChanged += PresenterSourceChanged;
            _adapter = new CommentAdapter(Context, Presenter);
            _adapter.LikeAction += LikeAction;
            _adapter.UserAction += UserAction;
            _adapter.FlagAction += FlagAction;
            _adapter.HideAction += HideAction;
            _adapter.ReplyAction += ReplyAction;

            _comments.SetLayoutManager(_manager);
            _comments.SetAdapter(_adapter);
            if (!BasePresenter.User.IsAuthenticated)
                _messagePanel.Visibility = ViewStates.Gone;

            LoadComments(_uid);
        }

        private void PresenterSourceChanged(Status status)
        {
            if (!IsInitialized)
                return;

            Activity.RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }

        private void OnBack(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

        private async void OnPost(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_textInput.Text))
                return;

            _sendSpinner.Visibility = ViewStates.Visible;
            _post.Enabled = false;
            _postImage.Visibility = ViewStates.Invisible;

            HideKeyboard();

            var resp = await Presenter.TryCreateComment(_textInput.Text, _uid);

            if (!IsInitialized)
                return;

            if (resp != null && resp.Success)
            {
                _textInput.Text = string.Empty;
                _textInput.ClearFocus();

                var errors = await Presenter.TryLoadNextComments(_uid);

                if (!IsInitialized)
                    return;

                Context.ShowAlert(errors, ToastLength.Short);
                _comments.MoveToPosition(Presenter.Count - 1);

                _counter++;

                Activity.Intent.PutExtra(ResultString, _uid);
                Activity.Intent.PutExtra(CountString, _counter);
            }
            else
            {
                Context.ShowAlert(resp, ToastLength.Short);
            }

            _sendSpinner.Visibility = ViewStates.Invisible;
            _post.Enabled = true;
            _postImage.Visibility = ViewStates.Visible;
        }

        private async void LoadComments(string postUrl)
        {
            _spinner.Visibility = ViewStates.Visible;

            var errors = await Presenter.TryLoadNextComments(postUrl);

            if (!IsInitialized)
                return;

            Context.ShowAlert(errors, ToastLength.Short);

            _spinner.Visibility = ViewStates.Gone;
        }

        private void UserAction(Post post)
        {
            if (post == null)
                return;

            if (BasePresenter.User.Login != post.Author)
                ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(post.Author));
        }

        private void ReplyAction(Post post)
        {
            if (post == null)
                return;
            if (!_textInput.Text.StartsWith($"@{post.Author}"))
            {
                _textInput.Text = $"@{post.Author} {_textInput.Text}";
                _textInput.SetSelection(_textInput.Text.Length);
            }

            _textInput.RequestFocus();
            ((BaseActivity)Activity).OpenKeyboard(_textInput);
        }

        private async void LikeAction(Post post)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                var errors = await Presenter.TryVote(post);

                if (!IsInitialized)
                    return;
                Context.ShowAlert(errors, ToastLength.Short);
            }
            else
            {
                var intent = new Intent(Context, typeof(WelcomeActivity));
                StartActivity(intent);
            }
        }

        private async void FlagAction(Post post)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                var errors = await Presenter.TryFlag(post);

                if (!IsInitialized)
                    return;
                Context.ShowAlert(errors, ToastLength.Short);
            }
            else
            {
                var intent = new Intent(Context, typeof(WelcomeActivity));
                StartActivity(intent);
            }
        }

        private void HideAction(Post post)
        {
            Presenter.RemovePost(post);
        }

        protected void HideKeyboard()
        {
            ((BaseActivity)Activity).HideKeyboard();
        }
    }
}