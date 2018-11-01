﻿using System;
using System.Collections.Generic;
using System.Linq;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;
using Steepshot.CustomViews;
using Steepshot.Utils;
using Steepshot.Utils.Media;

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
        [BindView(Resource.Id.video_preview)] private EditMediaView _vpreview;
        [BindView(Resource.Id.photos_grid)] private CoordinatorRecyclerView _gridView;
#pragma warning restore 0649

        #endregion

        #region Fields

        private const byte MaxPhotosAllowed = 7;

        private List<GalleryMediaModel> _gallery;
        private List<GalleryMediaModel> _pickedItems;
        private GalleryMediaModel _prevSelected;
        private bool _multiSelect;
        private List<string> _buckets;
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
            _coordinator.LayoutParameters.Height = Style.ScreenWidth + Resources.DisplayMetrics.HeightPixels - toolbarHeight;
            _coordinator.SetTopViewParam(Style.ScreenWidth, toolbarHeight);
            _previewContainer.LayoutParameters = new LinearLayout.LayoutParams(Style.ScreenWidth, Style.ScreenWidth);

            InitBucket();
            InitGalery();

            var foldersAdapter = new SpinnerAdapter(Activity, Android.Resource.Layout.SimpleSpinnerDropDownItem, _buckets);
            _folders.Adapter = foldersAdapter;
            _folders.ItemSelected += FoldersOnItemSelected;
            _folders.SetSelection(0);

            _gridAdapter = new GalleryGridAdapter();
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

        private void NextBtnOnClick(object sender, EventArgs eventArgs)
        {
            if (!_preview.IsBitmapReady)
                return;

            if (_pickedItems.Count > 0)
            {
                for (int i = 0; i < _pickedItems.Count; i++)
                {
                    var itm = _pickedItems[i];
                    if (itm.Selected)
                        itm.Parameters = _preview.DrawableImageParameters.Copy();
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
            if (!_preview.IsBitmapReady)
                return;

            _preview.SwitchScale();
        }

        private void RotateBtnOnClick(object sender, EventArgs eventArgs)
        {
            if (!_preview.IsBitmapReady)
                return;

            _preview.Rotate(_preview.DrawableImageParameters.Rotation + 90);
        }

        private void MultiselectBtnOnClick(object sender, EventArgs eventArgs)
        {
            if (!_preview.IsBitmapReady)
                return;

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

            var isVideo = MimeTypeHelper.IsVideo(model.MimeType);
            if (isVideo)
            {
                if (_multiSelect)
                    return;

                _ratioBtn.Visibility = _rotateBtn.Visibility = _multiselectBtn.Visibility = ViewStates.Gone;

                _vpreview.Stop();
                _vpreview.MediaSource = new MediaModel
                {
                    Url = model.Path,
                    ContentType = model.MimeType
                };
                _vpreview.Play();
                _preview.Visibility = ViewStates.Gone;
                _vpreview.Visibility = ViewStates.Visible;
            }
            else
            {
                _ratioBtn.Visibility = _rotateBtn.Visibility = _multiselectBtn.Visibility = ViewStates.Visible;
                _preview.Visibility = ViewStates.Visible;
                _vpreview.Visibility = ViewStates.Gone;
            }

            if (!isVideo && !_preview.IsBitmapReady)
                return;

            if (_coordinator.SwitchToWhole())
            {
                _gridView.ScrollToPosition(position);
                _gridView.ScrollBy(0, Style.ScreenWidth);
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
                : _gallery.FindAll(i => i.Bucket.Equals(_selectedBucket, StringComparison.OrdinalIgnoreCase));

            set.Sort((x, y) => -x.DateTaken.CompareTo(y.DateTaken));

            _gridAdapter.SetMedia(set);

            if (set.Count > 0 && _pickedItems.Count == 0 || !_multiSelect)
                OnItemSelected(set[0], 0);
        }

        private void InitBucket()
        {
            string[] columns = { $"DISTINCT {MediaStore.Images.ImageColumns.BucketDisplayName}" };

            var imagesCursor = Activity.ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, columns, null, null, null);
            var videosCursor = Activity.ContentResolver.Query(MediaStore.Video.Media.ExternalContentUri, columns, null, null, null);
            _buckets = new List<string> { App.Localization.GetText(LocalizationKeys.Gallery) };
            MergeBuckets(imagesCursor, videosCursor);
        }

        private void MergeBuckets(params ICursor[] cursors)
        {
            foreach (var cursor in cursors)
            {
                var count = cursor.Count;
                var dataColumnIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.BucketDisplayName);
                for (var i = 0; i < count; i++)
                {
                    cursor.MoveToPosition(i);
                    var bucket = cursor.GetString(dataColumnIndex);
                    if (!_buckets.Contains(bucket))
                        _buckets.Add(bucket);
                }
                cursor.Close();
            }
        }

        private void InitGalery()
        {
            _gallery = new List<GalleryMediaModel>();

            string[] imageColumns =
            {
                MediaStore.Images.ImageColumns.Id,
                MediaStore.Images.ImageColumns.Data,
                MediaStore.Images.ImageColumns.DateTaken,
                MediaStore.Images.ImageColumns.BucketDisplayName,
                MediaStore.Images.ImageColumns.MimeType,
                MediaStore.Images.ImageColumns.Orientation
            };

            var imagesCursor = Activity.ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, imageColumns, null, null, null);

            string[] videoColumns =
            {
                MediaStore.Video.VideoColumns.Id,
                MediaStore.Video.VideoColumns.Data,
                MediaStore.Video.VideoColumns.DateTaken,
                MediaStore.Video.VideoColumns.BucketDisplayName,
                MediaStore.Video.VideoColumns.MimeType,
                MediaStore.Video.VideoColumns.Duration
            };

            var videosCursor = Activity.ContentResolver.Query(MediaStore.Video.Media.ExternalContentUri, videoColumns, null, null, null);

            MergeMediaCursors(imagesCursor, videosCursor);
        }

        private void MergeMediaCursors(params ICursor[] cursors)
        {
            foreach (var cursor in cursors)
            {
                if (cursor?.Count > 0)
                {
                    var count = cursor.Count;
                    var idColumnIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Id);
                    var dataColumnIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data);
                    var dateTakenColumnIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.DateTaken);
                    var oriColumnIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Orientation);
                    var durationColumnIndex = cursor.GetColumnIndex(MediaStore.Video.VideoColumns.Duration);
                    var bucketDisplayNameIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.BucketDisplayName);
                    var mimeTypeIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.MimeType);

                    for (var i = 0; i < count; i++)
                    {
                        cursor.MoveToPosition(i);

                        var id = cursor.GetLong(idColumnIndex);
                        var path = cursor.GetString(dataColumnIndex);
                        var date = new DateTime(cursor.GetLong(dateTakenColumnIndex));
                        var bucket = cursor.GetString(bucketDisplayNameIndex);
                        var mimeType = cursor.GetString(mimeTypeIndex);
                        var isVideo = MimeTypeHelper.IsVideo(mimeType);
                        var orientation = isVideo ? 0 : cursor.GetInt(oriColumnIndex);
                        var duration = TimeSpan.FromMilliseconds(isVideo ? cursor.GetLong(durationColumnIndex) : 0);
                        _gallery.Add(new GalleryMediaModel(id, date, path, mimeType, bucket, orientation, duration));
                    }

                    cursor.Close();
                }
            }

            _gallery.Sort((x, y) =>
                -x.DateTaken.CompareTo(y.DateTaken));
        }

        public override void OnDetach()
        {
            _gridView.SetAdapter(null);
            base.OnDetach();
        }
    }
}