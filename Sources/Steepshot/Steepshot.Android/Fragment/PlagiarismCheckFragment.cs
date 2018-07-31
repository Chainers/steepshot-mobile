using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    public class PlagiarismCheckFragment : BaseFragmentWithPresenter<PostDescriptionPresenter>
    {
        private readonly List<GalleryMediaModel> media;
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
        [BindView(Resource.Id.IPFS_text)] protected TextView _IPFSText;
        [BindView(Resource.Id.IPFS_lyt)] protected RelativeLayout _IPFSButton;
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
            _plagiarismDescription.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PlagiarismDescription);
            _IPFSText.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.IPFSLink);
            _cancelPublishing.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.CancelPublishing);
            _continuePublishing.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ContinuePublishing);

            _pageTitle.Typeface = Style.Semibold;
            _guidelines.Typeface = Style.Semibold;
            _plagiarismTitle.Typeface = Style.Regular;
            _plagiarismDescription.Typeface = Style.Regular;
            _IPFSText.Typeface = Style.Semibold;
            _cancelPublishing.Typeface = Style.Semibold;
            _continuePublishing.Typeface = Style.Semibold;

            guidelinesUri = Android.Net.Uri.Parse(Constants.Guide);

            _topPanel.BringToFront();

            _backButton.Click += (sender, e) => { ((BaseActivity)Activity).OnBackPressed(); };
            _rootLayout.Click += (sender, e) => { ((BaseActivity)Activity).HideKeyboard(); };

            _guidelines.Click += OpenGuidelines;
            _IPFSButton.Click += IPFSLink;

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
                    var previewSize = Utils.ViewUtils.CalculateImagePreviewSize(media[0].PreparedBitmap.Width,
                        media[0].PreparedBitmap.Height, Resources.DisplayMetrics.WidthPixels - margin * 2,
                        int.MaxValue);
                    var layoutParams = new RelativeLayout.LayoutParams(previewSize.Width, previewSize.Height);
                    layoutParams.SetMargins(margin, 0, margin, margin);
                    _previewContainer.LayoutParameters = layoutParams;

                    _preview.SetImageBitmap(media[0].PreparedBitmap);
                }
            }

            if (model.PlagiarismUsername == AppSettings.User.Login)
                _plagiarismTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.SelfPlagiarism);
            else
                _plagiarismTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PhotoPlagiarism);
        }

        private void OpenGuidelines(object sender, System.EventArgs e)
        {
            var browserIntent = new Intent(Intent.ActionView, guidelinesUri);
            StartActivity(browserIntent);
        }

        private void IPFSLink(object sender, System.EventArgs e)
        {
            
        }
    }
}
