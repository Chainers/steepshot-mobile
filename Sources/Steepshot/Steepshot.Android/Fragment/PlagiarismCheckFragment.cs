using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Method;
using Android.Text.Style;
using Android.Util;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public sealed class PlagiarismCheckFragment : BaseFragmentWithPresenter<PostDescriptionPresenter>
    {
        private readonly List<GalleryMediaModel> _media;
        private readonly Plagiarism _model;
        private GalleryMediaAdapter _adapter;
        private Android.Net.Uri _guidelinesUri;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.btn_back)] private ImageButton _backButton;
        [BindView(Resource.Id.page_title)] private TextView _pageTitle;
        [BindView(Resource.Id.guidelines)] private TextView _guidelines;
        [BindView(Resource.Id.scroll_container)] private ScrollView _descriptionScrollContainer;
        [BindView(Resource.Id.photos_layout)] private RelativeLayout _photosContainer;
        [BindView(Resource.Id.photos)] private RecyclerView _photos;
        [BindView(Resource.Id.photo_preview_container)] private RelativeLayout _previewContainer;
        [BindView(Resource.Id.photo_preview)] private ImageView _preview;
        [BindView(Resource.Id.plagiarism_title)] private TextView _plagiarismTitle;
        [BindView(Resource.Id.plagiarism_description)] private TextView _plagiarismDescription;
        [BindView(Resource.Id.btn_cancel)] private Button _cancelPublishing;
        [BindView(Resource.Id.cancel_loading_spinner)] private ProgressBar _cancelSpinner;
        [BindView(Resource.Id.btn_continue)] private Button _continuePublishing;
        [BindView(Resource.Id.continue_loading_spinner)] private ProgressBar _continueSpinner;
        [BindView(Resource.Id.toolbar)] private LinearLayout _topPanel;
        [BindView(Resource.Id.root_layout)] private RelativeLayout _rootLayout;


        public PlagiarismCheckFragment(List<GalleryMediaModel> media, Plagiarism model)
        {
            _media = media;
            _model = model;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_plagiarism, null);
                Cheeseknife.Bind(this, InflatedView);
            }
            _adapter = new GalleryMediaAdapter(_media);

            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            _pageTitle.Text = App.Localization.GetText(LocalizationKeys.PlagiarismTitle);
            _guidelines.Text = App.Localization.GetText(LocalizationKeys.GuidelinesForPlagiarism);
            _cancelPublishing.Text = App.Localization.GetText(LocalizationKeys.CancelPublishing);
            _continuePublishing.Text = App.Localization.GetText(LocalizationKeys.ContinuePublishing);

            _pageTitle.Typeface = Style.Semibold;
            _guidelines.Typeface = Style.Semibold;
            _plagiarismTitle.Typeface = Style.Regular;
            _plagiarismDescription.Typeface = Style.Regular;
            _cancelPublishing.Typeface = Style.Semibold;
            _continuePublishing.Typeface = Style.Semibold;

            _guidelinesUri = Android.Net.Uri.Parse(Constants.Guide);

            _topPanel.BringToFront();

            _backButton.Click += (sender, e) => { ((BaseActivity)Activity).OnBackPressed(); };
            _rootLayout.Click += (sender, e) => { ((BaseActivity)Activity).HideKeyboard(); };

            _guidelines.Click += OpenGuidelines;

            _cancelPublishing.Click += CancelPublishing;
            _continuePublishing.Click += ContinuePublishing;

            if (_media.Count > 1)
            {
                _photos.Visibility = ViewStates.Visible;
                _previewContainer.Visibility = ViewStates.Gone;
                _photos.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                _photos.SetAdapter(_adapter);
                _photos.AddItemDecoration(new ListItemDecoration((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Resources.DisplayMetrics)));
            }
            else
            {
                _photos.Visibility = ViewStates.Gone;
                _previewContainer.Visibility = ViewStates.Visible;
                _preview.ClipToOutline = true;

                var margin = (int)BitmapUtils.DpToPixel(15, Resources);

                var model = _media[0];
                if (!string.IsNullOrEmpty(model.TempPath))
                {
                    Bitmap bitmap = MimeTypeHelper.IsVideo(model.MimeType)
                        ? ThumbnailUtils.CreateVideoThumbnail(model.TempPath, ThumbnailKind.FullScreenKind)
                        : BitmapUtils.DecodeSampledBitmapFromFile(Context, Android.Net.Uri.Parse(model.TempPath), Style.ScreenWidth - margin * 2, Style.ScreenWidth - margin * 2);
                    _preview.SetImageBitmap(bitmap);
                }
            }

            var similarText = App.Localization.GetText(LocalizationKeys.SimilarPhoto).ToLower();
            var photoText = App.Localization.GetText(LocalizationKeys.Photo).ToLower();
            var plagiarismText = string.Empty;

            CustomClickableSpan clickableSpan;
            SpannableString spannableTitle;

            if (_model.PlagiarismUsername == App.User.Login)
            {
                plagiarismText = App.Localization.GetText(LocalizationKeys.SelfPlagiarism, similarText);

                clickableSpan = new CustomClickableSpan();
                clickableSpan.Click += OpenSimilar;

                spannableTitle = SpannableText(plagiarismText, similarText, clickableSpan);
                _plagiarismTitle.SetText(spannableTitle, TextView.BufferType.Spannable);
                _plagiarismDescription.Visibility = ViewStates.Gone;
            }
            else
            {
                var author = $"@{_model.PlagiarismUsername}";
                plagiarismText = App.Localization.GetText(LocalizationKeys.PhotoPlagiarism, similarText, author);

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

                var descriptionText = App.Localization.GetText(LocalizationKeys.PlagiarismDescription, photoText);

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
            var browserIntent = new Intent(Intent.ActionView, _guidelinesUri);
            StartActivity(browserIntent);
        }

        private void OpenSimilar(View obj)
        {
            var link = $"@{_model.PlagiarismUsername}/{_model.PlagiarismPermlink}";

            ((BaseActivity)Activity).OpenNewContentFragment(new PostViewFragment(link));
        }

        private void OpenProfile(View obj)
        {
            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(_model.PlagiarismUsername));
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

        public override void OnDetach()
        {
            _photos.SetAdapter(null);
            base.OnDetach();
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
