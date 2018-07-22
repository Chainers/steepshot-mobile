using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.CustomViews;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class GalleryFragment : BaseFragment
    {
        #region BindView

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.folders_spinner)] private Spinner _folders;
        [BindView(Resource.Id.coordinator)] private CoordinatorLinearLayout _coordinator;
        [BindView(Resource.Id.photo_preview_container)] private RelativeLayout _previewContainer;
        [BindView(Resource.Id.ratio_switch)] private ImageButton _ratioBtn;
        [BindView(Resource.Id.rotate)] private ImageButton _rotateBtn;
        [BindView(Resource.Id.multiselect)] private ImageButton _multiselectBtn;
        [BindView(Resource.Id.arrow_back)] private ImageButton _backBtn;
        [BindView(Resource.Id.arrow_next)] private ImageButton _nextBtn;
        [BindView(Resource.Id.photo_preview)] private CropView _preview;
        [BindView(Resource.Id.photos_grid)] private CoordinatorRecyclerView _gridView;
#pragma warning restore 0649

        #endregion

        #region Fields

        private const byte MaxPhotosAllowed = 7;

        private GalleryMediaModel[] _gallery;
        private List<GalleryMediaModel> _pickedItems;
        private GalleryMediaModel _prevSelected;
        private bool _multiSelect;
        private string[] _buckets;
        private string _selectedBucket;

        private GalleryGridAdapter _gridAdapter;

        #endregion


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_gallery, null);
                Cheeseknife.Bind(this, InflatedView);
            }

            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            var toolbarHeight = (int)BitmapUtils.DpToPixel(10, Resources);
            _coordinator.LayoutParameters.Height = Resources.DisplayMetrics.WidthPixels + Resources.DisplayMetrics.HeightPixels - toolbarHeight;
            _coordinator.SetTopViewParam(Resources.DisplayMetrics.WidthPixels, toolbarHeight);

            _previewContainer.LayoutParameters = new LinearLayout.LayoutParams(Resources.DisplayMetrics.WidthPixels, Resources.DisplayMetrics.WidthPixels);

            InitBucket();
            InitGalery();

            var foldersAdapter = new SpinnerAdapter(Activity, Android.Resource.Layout.SimpleSpinnerDropDownItem, _buckets);
            _folders.Adapter = foldersAdapter;
            _folders.ItemSelected += FoldersOnItemSelected;
            _folders.SetSelection(0);

            _gridAdapter = new GalleryGridAdapter(Activity);
            _gridAdapter.OnItemSelected += OnItemSelected;

            var gridLayoutManager = new GridLayoutManager(Activity, 3) { SmoothScrollbarEnabled = true };
            _gridView.SetLayoutManager(gridLayoutManager);
            _gridView.SetAdapter(_gridAdapter);
            _gridView.SetCoordinatorListener(_coordinator);

            _ratioBtn.Click += RatioBtnOnClick;
            _rotateBtn.Click += RotateBtnOnClick;
            _multiselectBtn.Click += MultiselectBtnOnClick;
            _backBtn.Click += BackBtnOnClick;
            _nextBtn.Click += NextBtnOnClick;

            _pickedItems = new List<GalleryMediaModel>();
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
            GC.Collect(0);
        }


        private void NextBtnOnClick(object sender, EventArgs eventArgs)
        {
            if (!_preview.IsBitmapReady)
                return;

            if (_pickedItems.Count > 0)
            {
                _pickedItems.Find(x => x.Selected).Parameters = _preview.DrawableImageParameters.Copy();
                foreach (var galleryMediaModel in _pickedItems)
                {
                    var croppedBitmap = _preview.Crop(galleryMediaModel.Path, galleryMediaModel.Parameters);
                    galleryMediaModel.PreparedBitmap = croppedBitmap;
                }
                ((BaseActivity)Activity).OpenNewContentFragment(new PostCreateFragment(_pickedItems));
            }
            else
            {
                Activity.ShowAlert(LocalizationKeys.NoPhotosPicked);
            }
        }

        private void BackBtnOnClick(object sender, EventArgs eventArgs)
        {
            ((BaseActivity)Activity).OnBackPressed();
        }

        private void RatioBtnOnClick(object sender, EventArgs eventArgs)
        {
            _preview.SwitchScale();
        }

        private void RotateBtnOnClick(object sender, EventArgs eventArgs)
        {
            if (!_preview.IsBitmapReady)
                return;

            _preview.Rotate(_preview.DrawableImageParameters.Rotation + 90f);
        }

        private void MultiselectBtnOnClick(object sender, EventArgs eventArgs)
        {
            _multiSelect = !_multiSelect;
            _ratioBtn.Visibility = _multiSelect ? ViewStates.Gone : ViewStates.Visible;
            _preview.UseStrictBounds = _multiSelect;

            foreach (var model in _gallery)
            {
                model.MultySelect = _multiSelect;
                model.SelectionPosition = 0;
            }

            GalleryMediaModel selectedItem = null;
            for (var i = 0; i < _pickedItems.Count; i++)
            {
                if (_pickedItems[i].Selected)
                {
                    selectedItem = _pickedItems[i];
                    if (_multiSelect)
                    {
                        selectedItem.Parameters = _preview.DrawableImageParameters.Copy();
                        continue;
                    }
                }

                _pickedItems[i].Parameters = null;
            }

            _pickedItems.Clear();
            _prevSelected = null;

            if (selectedItem == null)
            {
                selectedItem = _buckets[0].Equals(_selectedBucket)
                    ? _gallery.FirstOrDefault()
                    : _gallery.FirstOrDefault(m => m.Bucket.Equals(_selectedBucket, StringComparison.OrdinalIgnoreCase));
            }

            OnItemSelected(selectedItem, 0);
            _multiselectBtn.SetImageResource(_multiSelect ? Resource.Drawable.ic_multiselect_active : Resource.Drawable.ic_multiselect);

            _gridAdapter.NotifyDataSetChanged();
        }

        private void OnItemSelected(GalleryMediaModel model, int position)
        {
            if (_multiSelect && _pickedItems.Count >= MaxPhotosAllowed && !(model.Selected || model.SelectionPosition > 0))
            {
                Activity.ShowAlert(LocalizationKeys.PickedPhotosLimit, ToastLength.Short);
                return;
            }

            if (_coordinator.SwitchToWhole())
            {
                _gridView.ScrollToPosition(position);
                _gridView.ScrollBy(0, Resources.DisplayMetrics.WidthPixels);
            }

            for (int i = 0; i < _pickedItems.Count; i++)
            {
                var selected = _pickedItems[i];
                if (selected.Selected && selected != model)
                {
                    selected.Selected = false;
                }
            }

            if (_multiSelect)
            {
                if (_prevSelected != null)
                    _prevSelected.Parameters = _preview.DrawableImageParameters.Copy();

                _prevSelected = model;

                if (!_pickedItems.Contains(model))
                {
                    _pickedItems.Add(model);
                    model.SelectionPosition = _pickedItems.Count;
                }
                else if (model.Selected)
                {
                    model.Parameters = null;
                    model.Selected = false;
                    model.SelectionPosition = 0;
                    _prevSelected = null;
                    _pickedItems.Remove(model);

                    for (var index = 0; index < _pickedItems.Count; index++)
                    {
                        var x = _pickedItems[index];
                        x.SelectionPosition = index + 1;
                    }

                    GalleryMediaModel selectedItem;
                    if (_pickedItems.Count > 0)
                    {
                        selectedItem = _pickedItems.Last();
                    }
                    else
                    {
                        selectedItem = _buckets[0].Equals(_selectedBucket)
                            ? _gallery.FirstOrDefault()
                            : _gallery.FirstOrDefault(m => m.Bucket.Equals(_selectedBucket, StringComparison.OrdinalIgnoreCase));
                    }

                    if (!_multiSelect || _pickedItems.Count > 0)
                        OnItemSelected(selectedItem, 0);
                    return;
                }
            }
            else
            {
                _pickedItems.Clear();
                _pickedItems.Add(model);
            }

            model.Selected = true;
            _preview.SetImage(model);
        }

        private void FoldersOnItemSelected(object sender, AdapterView.ItemSelectedEventArgs itemSelectedEventArgs)
        {
            var pos = itemSelectedEventArgs.Position;
            _selectedBucket = _buckets[pos];

            var set = pos == 0
                ? _gallery
                : _gallery.Where(i => i.Bucket.Equals(_selectedBucket, StringComparison.OrdinalIgnoreCase)).ToArray();

            _gridAdapter.SetMedia(set);

            if (set.Length > 0 && _pickedItems.Count == 0 || !_multiSelect)
                OnItemSelected(set[0], 0);
        }

        private void InitBucket()
        {
            string[] columns = { $"DISTINCT {MediaStore.Images.ImageColumns.BucketDisplayName}" };

            var cursor = Activity.ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, columns, null, null, null);
            var count = cursor.Count;
            _buckets = new string[count + 1];
            _buckets[0] = AppSettings.LocalizationManager.GetText(LocalizationKeys.Gallery);

            var dataColumnIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.BucketDisplayName);
            for (var i = 0; i < count; i++)
            {
                cursor.MoveToPosition(i);
                _buckets[i + 1] = cursor.GetString(dataColumnIndex);
            }
            cursor.Close();
        }

        private void InitGalery()
        {
            string[] columns =
            {
                MediaStore.Images.ImageColumns.Id,
                MediaStore.Images.ImageColumns.Data,
                MediaStore.Images.ImageColumns.BucketDisplayName,
                MediaStore.Images.ImageColumns.Orientation
            };

            var orderBy = $"{MediaStore.Images.ImageColumns.DateTaken} DESC";
            var cursor = Activity.ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, columns, null, null, orderBy);

            if (cursor != null)
            {
                var count = cursor.Count;
                var idColumnIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Id);
                var dataColumnIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data);
                var oriColumnIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Orientation);
                var bucketDisplayName = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.BucketDisplayName);
                _gallery = new GalleryMediaModel[count];

                for (var i = 0; i < count; i++)
                {
                    cursor.MoveToPosition(i);
                    _gallery[i] = new GalleryMediaModel
                    {
                        Id = cursor.GetLong(idColumnIndex),
                        Path = cursor.GetString(dataColumnIndex),
                        Bucket = cursor.GetString(bucketDisplayName),
                        Orientation = cursor.GetInt(oriColumnIndex)
                    };
                }
                cursor.Close();
            }
            else
            {
                _gallery = new GalleryMediaModel[0];
            }
        }
    }
}