using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
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
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;

namespace Steepshot.Fragment
{
    public class GalleryFragment : BaseFragment
    {
        private const byte MaxPhotosAllowed = 7;

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

        private bool IsSdCardAvailable => Environment.ExternalStorageState.Equals(Environment.MediaMounted);
        private string _selectedBucket;
        private string BucketSelection => _selectedBucket.Equals(AppSettings.LocalizationManager.GetText(LocalizationKeys.Gallery)) ? null : MediaStore.Images.ImageColumns.BucketDisplayName + $" = \"{_selectedBucket}\"";
        private Dictionary<string, string> _media;
        private Dictionary<string, string> _thumbnails;
        private Dictionary<string, List<GalleryMediaModel>> _gallery;
        private List<GalleryMediaModel> _pickedItems;
        private GalleryMediaModel _prevSelected;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _multiSelect;

        private string[] _buckets;
        private string[] Buckets
        {
            get
            {
                if (_buckets == null)
                {
                    string[] columns = { "Distinct " + MediaStore.Images.ImageColumns.BucketDisplayName };

                    var cursor = Activity.ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, columns, null, null, null);
                    int count = cursor.Count;
                    _buckets = new string[count + 1];
                    _buckets[0] = AppSettings.LocalizationManager.GetText(LocalizationKeys.Gallery);

                    for (int i = 0; i < count; i++)
                    {
                        cursor.MoveToPosition(i);
                        int dataColumnIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.BucketDisplayName);
                        _buckets[i + 1] = cursor.GetString(dataColumnIndex);
                    }

                    cursor.Close();
                }

                return _buckets;
            }
        }

        private GalleryGridAdapter _gridAdapter;
        private GalleryGridAdapter GridAdapter => _gridAdapter ?? (_gridAdapter = new GalleryGridAdapter(Activity));

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

            var foldersAdapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleSpinnerDropDownItem, Buckets);
            _folders.Adapter = foldersAdapter;
            _folders.ItemSelected += FoldersOnItemSelected;
            _folders.SetSelection(0);

            var gridLayoutManager = new GridLayoutManager(Activity, 3) { SmoothScrollbarEnabled = true };
            GridAdapter.OnItemSelected += OnItemSelected;
            _gridView.SetLayoutManager(gridLayoutManager);
            _gridView.SetAdapter(GridAdapter);
            _gridView.SetCoordinatorListener(_coordinator);

            _ratioBtn.Click += RatioBtnOnClick;
            _rotateBtn.Click += RotateBtnOnClick;
            _multiselectBtn.Click += MultiselectBtnOnClick;
            _backBtn.Click += BackBtnOnClick;
            _nextBtn.Click += NextBtnOnClick;

            _gallery = new Dictionary<string, List<GalleryMediaModel>>();
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
            if (!_preview.IsBitmapReady) return;
            if (_pickedItems.Count > 0)
            {
                _pickedItems.Last().Parameters = _preview.DrawableImageParameters.Copy();
                foreach (var galleryMediaModel in _pickedItems)
                {
                    var croppedBitmap = _preview.Crop(Uri.Parse(galleryMediaModel.Path), galleryMediaModel.Parameters);
                    galleryMediaModel.PreparedBitmap = croppedBitmap;
                }
                ((BaseActivity)Activity).OpenNewContentFragment(new PostCreateFragment(_pickedItems));
            }
            else
            {
                Activity.ShowAlert(LocalizationKeys.NoPhotosPicked);
            }
        }
        private void BackBtnOnClick(object sender, EventArgs eventArgs) => ((BaseActivity)Activity).OnBackPressed();
        private void RatioBtnOnClick(object sender, EventArgs eventArgs) => _preview.SwitchScale();
        private void RotateBtnOnClick(object sender, EventArgs eventArgs) => _preview.Rotate(_preview.DrawableImageParameters.Rotation + 90f);
        private void MultiselectBtnOnClick(object sender, EventArgs eventArgs)
        {
            _multiSelect = !_multiSelect;
            _ratioBtn.Visibility = _multiSelect ? ViewStates.Gone : ViewStates.Visible;
            _preview.UseStrictBounds = _multiSelect;
            _gallery[_selectedBucket].ForEach(x => x.SelectionPosition = _multiSelect ? (int)GallerySelectionType.Multi : (int)GallerySelectionType.None);
            GalleryMediaModel selectedItem = null;
            _pickedItems.ForEach(x =>
            {
                if (x.Selected) selectedItem = x;
                x.Parameters = null;
            });
            _pickedItems.Clear();
            _prevSelected = null;
            OnItemSelected(selectedItem ?? _gallery[_selectedBucket][0]);
            _multiselectBtn.SetImageResource(_multiSelect ? Resource.Drawable.ic_multiselect_active : Resource.Drawable.ic_multiselect);
        }
        private void OnItemSelected(GalleryMediaModel model)
        {
            if (_pickedItems.Count >= MaxPhotosAllowed && model.SelectionPosition == (int)GallerySelectionType.Multi)
            {
                Activity.ShowAlert(LocalizationKeys.PickedPhotosLimit);
                return;
            }
            var selected = _pickedItems.Find(x => x.Selected && x != model);
            if (selected != null) selected.Selected = false;
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
                    _pickedItems.Remove(model);
                    _pickedItems.ForEach(x => x.SelectionPosition = _pickedItems.IndexOf(x) + 1);
                    model.Selected = false;
                    model.SelectionPosition = (int)GallerySelectionType.Multi;
                    if (_pickedItems.Count > 0)
                        OnItemSelected(_pickedItems.Last());
                    return;
                }
            }
            else
            {
                _pickedItems.Clear();
                _pickedItems.Add(model);
            }

            model.Selected = true;
            _preview.SetImageUri(Uri.Parse(model.Path), model.Parameters);
        }
        private void FoldersOnItemSelected(object sender, AdapterView.ItemSelectedEventArgs itemSelectedEventArgs)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            _selectedBucket = Buckets[itemSelectedEventArgs.Position];
            _media = GetMediaPaths();
            _thumbnails = GetMediaThumbnailsPaths();
            if (!_gallery.ContainsKey(_selectedBucket))
                _gallery.Add(_selectedBucket, GetMediaWithThumbnails());
            _gallery[_selectedBucket].ForEach(x => x.SelectionPosition = _multiSelect ? (int)GallerySelectionType.Multi : (int)GallerySelectionType.None);

            GridAdapter.SetMedia(_gallery[_selectedBucket]);
            OnItemSelected(_gallery[_selectedBucket][0]);
        }

        private List<GalleryMediaModel> GetMediaWithThumbnails()
        {
            var result = new List<GalleryMediaModel>();
            foreach (var tuple in _media)
            {
                var galleryModel = new GalleryMediaModel
                {
                    Id = tuple.Key,
                    Path = "file://" + tuple.Value,
                    Thumbnail = _thumbnails.ContainsKey(tuple.Key) ? _thumbnails[tuple.Key] : string.Empty,
                    Selected = false
                };
                if (string.IsNullOrEmpty(galleryModel.Thumbnail))
                    GenerateThumbnail(_selectedBucket, galleryModel, _cancellationTokenSource.Token);
                result.Add(galleryModel);
            }
            return result;
        }
        private Dictionary<string, string> GetMediaPaths()
        {
            string[] columns =
                {
                    MediaStore.Images.ImageColumns.Data,
                    MediaStore.Images.ImageColumns.Id
                };

            string orderBy = MediaStore.Images.ImageColumns.DateTaken;
            var cursor = Activity.ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, columns, BucketSelection, null, orderBy);
            int count = cursor.Count;
            var result = new Dictionary<string, string>();

            for (int i = 0; i < count; i++)
            {
                cursor.MoveToPosition(i);
                int dataColumnIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data);
                int idColumnIndex = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Id);
                result.Add(cursor.GetString(idColumnIndex), cursor.GetString(dataColumnIndex));
            }

            cursor.Close();
            return result;
        }
        private Dictionary<string, string> GetMediaThumbnailsPaths()
        {
            string[] columns =
            {
                    MediaStore.Images.Thumbnails.Data,
                    MediaStore.Images.Thumbnails.ImageId
                };

            var cursor = Activity.ContentResolver.Query(MediaStore.Images.Thumbnails.ExternalContentUri, columns, null, null, null);
            int count = cursor.Count;
            var result = new Dictionary<string, string>();

            for (int i = 0; i < count; i++)
            {
                cursor.MoveToPosition(i);
                int dataColumnIndex = cursor.GetColumnIndex(MediaStore.Images.Thumbnails.Data);
                int idColumnIndex = cursor.GetColumnIndex(MediaStore.Images.Thumbnails.ImageId);
                var key = cursor.GetString(idColumnIndex);
                if (result.ContainsKey(key))
                    result.Add(key, cursor.GetString(dataColumnIndex));
            }

            cursor.Close();
            return result;
        }
        private void GenerateThumbnail(string bucket, GalleryMediaModel model, CancellationToken ct) => Task.Run(() =>
        {
            var thumbnail = MediaStore.Images.Thumbnails.GetThumbnail(Activity.ContentResolver, long.Parse(model.Id),
                ThumbnailKind.MiniKind, null);

            var values = new ContentValues(4);
            values.Put(MediaStore.Images.Thumbnails.Kind, (int)ThumbnailKind.MiniKind);
            values.Put(MediaStore.Images.Thumbnails.ImageId, long.Parse(model.Id));
            values.Put(MediaStore.Images.Thumbnails.Height, thumbnail.Height);
            values.Put(MediaStore.Images.Thumbnails.Width, thumbnail.Width);

            if (ct.IsCancellationRequested)
                return;

            var uri = Activity.ContentResolver.Insert(MediaStore.Images.Thumbnails.ExternalContentUri, values);

            using (var thumbOut = Activity.ContentResolver.OpenOutputStream(uri))
            {
                thumbnail.Compress(Bitmap.CompressFormat.Jpeg, 100, thumbOut);
            }

            var cursor = MediaStore.Images.Thumbnails.QueryMiniThumbnail(Activity.ContentResolver, long.Parse(model.Id),
                ThumbnailKind.MiniKind, new[] { MediaStore.Images.Thumbnails.Data });

            if (cursor != null && cursor.Count > 0)
            {
                cursor.MoveToFirst();
                var thumbUri = cursor.GetString(0);
                model.Thumbnail = thumbUri;
                Activity.RunOnUiThread(() =>
                GridAdapter.NotifyItemChanged(_gallery[bucket].IndexOf(model)));
                cursor.Close();
            }
        }, ct);
    }
}