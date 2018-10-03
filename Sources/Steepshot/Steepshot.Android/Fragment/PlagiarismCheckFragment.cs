using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Method;
using Android.Text.Style;
using Android.Util;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PlagiarismCheckFragment : BaseFragmentWithPresenter<PostDescriptionPresenter>
    {
        private readonly List<GalleryMediaModel> media;
        private readonly PreparePostModel postModel;
        private readonly Plagiarism model;
        private RecyclerView.Adapter adapter;
        private Android.Net.Uri guidelinesUri;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.btn_back)] protected ImageButton _backButton;
        [BindView(Resource.Id.page_title)] protected TextView _pageTitle;
        [BindView(Resource.Id.guidelines)] protected TextView _guidelines;
        [BindView(Resource.Id.scroll_container)] protected ScrollView _descriptionScrollContainer;
        [BindView(Resource.Id.photos_layout)] protected RelativeLayout _photosContainer;
        [BindView(Resource.Id.photos)] protected RecyclerView _photos;
        [BindView(Resource.Id.photo_preview_container)] protected RelativeLayout _previewContainer;
        [BindView(Resource.Id.photo_preview)] protected ImageView _preview;
        [BindView(Resource.Id.plagiarism_title)] protected TextView _plagiarismTitle;
        [BindView(Resource.Id.plagiarism_description)] protected TextView _plagiarismDescription;
        [BindView(Resource.Id.btn_cancel)] protected Button _cancelPublishing;
        [BindView(Resource.Id.cancel_loading_spinner)] protected ProgressBar _cancelSpinner;
        [BindView(Resource.Id.btn_continue)] protected Button _continuePublishing;
        [BindView(Resource.Id.continue_loading_spinner)] protected ProgressBar _continueSpinner;
        [BindView(Resource.Id.toolbar)] protected LinearLayout _topPanel;
        [BindView(Resource.Id.root_layout)] protected RelativeLayout _rootLayout;


        public PlagiarismCheckFragment(List<GalleryMediaModel> media, RecyclerView.Adapter adapter, Plagiarism model)
        {
            this.media = media;
            this.adapter = adapter;
            this.model = model;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_plagiarism, null);
                Cheeseknife.Bind(this, InflatedView);
            }

            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            _pageTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PlagiarismTitle);
            _guidelines.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.GuidelinesForPlagiarism);
            _cancelPublishing.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.CancelPublishing);
            _continuePublishing.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ContinuePublishing);

            _pageTitle.Typeface = Style.Semibold;
            _guidelines.Typeface = Style.Semibold;
            _plagiarismTitle.Typeface = Style.Regular;
            _plagiarismDescription.Typeface = Style.Regular;
            _cancelPublishing.Typeface = Style.Semibold;
            _continuePublishing.Typeface = Style.Semibold;

            guidelinesUri = Android.Net.Uri.Parse(Constants.Guide);

            _topPanel.BringToFront();

            _backButton.Click += (sender, e) => { ((BaseActivity)Activity).OnBackPressed(); };
            _rootLayout.Click += (sender, e) => { ((BaseActivity)Activity).HideKeyboard(); };

            _guidelines.Click += OpenGuidelines;

            _cancelPublishing.Click += CancelPublishing;
            _continuePublishing.Click += ContinuePublishing;

            if (media.Count > 1)
            {
                _photos.Visibility = ViewStates.Visible;
                _previewContainer.Visibility = ViewStates.Gone;
                _photos.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                _photos.SetAdapter(adapter);
                _photos.AddItemDecoration(new ListItemDecoration((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Resources.DisplayMetrics)));
            }
            else
            {
                _photos.Visibility = ViewStates.Gone;
                _previewContainer.Visibility = ViewStates.Visible;
                _preview.ClipToOutline = true;

                var margin = (int)BitmapUtils.DpToPixel(15, Resources);

                if (media[0].PreparedBitmap != null)
                {
                    var previewSize = BitmapUtils.CalculateImagePreviewSize(media[0].Parameters, Style.ScreenWidth - margin * 2);
                    var layoutParams = new RelativeLayout.LayoutParams(previewSize.Width, previewSize.Height);
                    layoutParams.SetMargins(margin, 0, margin, margin);
                    _previewContainer.LayoutParameters = layoutParams;

                    _preview.SetImageBitmap(media[0].PreparedBitmap);
                }
            }

            var similarText = AppSettings.LocalizationManager.GetText(LocalizationKeys.SimilarPhoto).ToLower();
            var photoText = AppSettings.LocalizationManager.GetText(LocalizationKeys.Photo).ToLower();
            var plagiarismText = string.Empty;

            CustomClickableSpan clickableSpan;
            SpannableString spannableTitle;

            if (model.PlagiarismUsername == AppSettings.User.Login)
            {
                plagiarismText = AppSettings.LocalizationManager.GetText(LocalizationKeys.SelfPlagiarism, similarText);

                clickableSpan = new CustomClickableSpan();
                clickableSpan.Click += OpenSimilar;

                spannableTitle = SpannableText(plagiarismText, similarText, clickableSpan);
                _plagiarismTitle.SetText(spannableTitle, TextView.BufferType.Spannable);
                _plagiarismDescription.Visibility = ViewStates.Gone;
            }
            else
            {
                var author = $"@{model.PlagiarismUsername}";
                plagiarismText = AppSettings.LocalizationManager.GetText(LocalizationKeys.PhotoPlagiarism, similarText, author);

                spannableTitle = new SpannableString(plagiarismText);
                var similarIndex = plagiarismText.IndexOf(similarText, StringComparison.Ordinal);
                clickableSpan = new CustomClickableSpan();
                var authorIndex = plagiarismText.IndexOf(author, StringComparison.Ordinal);
                var clickableAuthor = new CustomClickableSpan();

                clickableSpan.Click += OpenSimilar;
                clickableAuthor.Click += OpenProfile;

                spannableTitle.SetSpan(new ForegroundColorSpan(Resources.GetColor(Resource.Color.rgb255_34_5)), similarIndex, similarIndex + similarText.Length, 0);
                spannableTitle.SetSpan(clickableSpan, similarIndex, similarIndex + similarText.Length, SpanTypes.ExclusiveExclusive);

                spannableTitle.SetSpan(new ForegroundColorSpan(Resources.GetColor(Resource.Color.rgb255_34_5)), authorIndex, authorIndex + author.Length, 0);
                spannableTitle.SetSpan(clickableAuthor, authorIndex, authorIndex + author.Length, SpanTypes.ExclusiveExclusive);
                _plagiarismTitle.SetText(spannableTitle, TextView.BufferType.Spannable);

                var descriptionText = AppSettings.LocalizationManager.GetText(LocalizationKeys.PlagiarismDescription, photoText);

                var clickablePhoto = new CustomClickableSpan();
                clickablePhoto.Click += OpenSimilar;

                var spannableDescription = SpannableText(descriptionText, photoText, clickablePhoto);
                _plagiarismDescription.Visibility = ViewStates.Visible;
                _plagiarismDescription.SetText(spannableDescription, TextView.BufferType.Spannable);
                _plagiarismDescription.MovementMethod = new LinkMovementMethod();
            }

            _plagiarismTitle.MovementMethod = new LinkMovementMethod();
        }

        private SpannableString SpannableText(string text, string word, CustomClickableSpan clickableSpan)
        {
            var spannableText = new SpannableString(text);
            var index = text.IndexOf(word, StringComparison.Ordinal);

            spannableText.SetSpan(new ForegroundColorSpan(Resources.GetColor(Resource.Color.rgb255_34_5)), index, index + word.Length, 0);
            spannableText.SetSpan(clickableSpan, index, index + word.Length, SpanTypes.ExclusiveExclusive);

            return spannableText;
        }

        private void OpenGuidelines(object sender, EventArgs e)
        {
            var browserIntent = new Intent(Intent.ActionView, guidelinesUri);
            StartActivity(browserIntent);
        }

        private void OpenSimilar(View obj)
        {
            var link = $"@{model.PlagiarismUsername}/{model.PlagiarismPermlink}";

            ((BaseActivity)Activity).OpenNewContentFragment(new PostViewFragment(link));
        }

        private void OpenProfile(View obj)
        {
            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(model.PlagiarismUsername));
        }

        private void CancelPublishing(object sender, EventArgs e)
        {
            TargetFragment.OnActivityResult(0, (int)Result.Canceled, null);
            ((BaseActivity)Activity).OnBackPressed();
        }

        private void ContinuePublishing(object sender, EventArgs e)
        {
            TargetFragment.OnActivityResult(0, (int)Result.Ok, null);
            ((BaseActivity)Activity).OnBackPressed();
        }
    }

    public class CustomClickableSpan : ClickableSpan
    {
        public Action<View> Click;

        public override void OnClick(View widget)
        {
            if (Click != null)
                Click(widget);
        }

        public override void UpdateDrawState(TextPaint ds)
        {
            ds.UnderlineText = false;
        }
    }
}
