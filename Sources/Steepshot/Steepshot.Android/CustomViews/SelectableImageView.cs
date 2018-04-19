using Android.Content;
using Android.Graphics;
using Android.Provider;
using Android.Widget;
using Steepshot.Utils;
using System.Collections.Generic;
using Android.Net;

namespace Steepshot.CustomViews
{
    public sealed class SelectableImageView : ImageView
    {
        private GalleryMediaModel _model;
        private static Dictionary<long, string> _thumbnails;
        private Paint _selectionPaint;
        private Paint _whitePaint;

        private Paint SelectionPaint => _selectionPaint ?? (_selectionPaint = new Paint(PaintFlags.AntiAlias) { Color = Style.R255G81B4, StrokeWidth = BitmapUtils.DpToPixel(6, Context.Resources) });
        private Paint WhitePaint => _whitePaint ?? (_whitePaint = new Paint(PaintFlags.AntiAlias) { Color = Color.White, StrokeWidth = BitmapUtils.DpToPixel(1, Context.Resources), TextSize = BitmapUtils.DpToPixel(16, Context.Resources), TextAlign = Paint.Align.Center });
        private Dictionary<long, string> Thumbnails => _thumbnails ?? (_thumbnails = BitmapUtils.GetMediaThumbnailsPaths(Context.ContentResolver, ThumbnailKind.MiniKind));


        public SelectableImageView(Context context) : base(context)
        {
            Clickable = true;
            SetScaleType(ScaleType.CenterCrop);
        }


        public override void Draw(Canvas canvas)
        {
            base.Draw(canvas);
            if (_model.Selected)
            {
                SelectionPaint.SetStyle(Paint.Style.Stroke);
                canvas.DrawRect(0, 0, Width, Height, SelectionPaint);
            }

            if (_model.SelectionPosition >= 0)
            {
                SelectionPaint.SetStyle(Paint.Style.Fill);
                var radius = BitmapUtils.DpToPixel(15, Context.Resources);
                var offset = BitmapUtils.DpToPixel(5, Context.Resources);
                var x = (int)(Width - radius - offset);
                var y = (int)(radius + offset);
                if (_model.SelectionPosition > 0)
                {
                    canvas.DrawCircle(x, y, radius, SelectionPaint);
                    WhitePaint.StrokeWidth = 1;
                    WhitePaint.SetStyle(Paint.Style.Fill);
                    canvas.DrawText(_model.SelectionPosition.ToString(), x, y - (WhitePaint.Descent() + WhitePaint.Ascent()) / 2f, WhitePaint);
                }
                else
                {
                    WhitePaint.StrokeWidth = 5;
                    WhitePaint.SetStyle(Paint.Style.Stroke);
                    canvas.DrawCircle(x, y, radius, WhitePaint);
                }
            }
        }

        public void Bind(GalleryMediaModel model)
        {
            if (_model != null)
                _model.ModelChanged -= ModelChanged;

            _model = model;
            _model.ModelChanged += ModelChanged;

            if (Thumbnails.ContainsKey(model.Id))
            {
                var path = Thumbnails[model.Id];
                SetImageURI(Uri.Parse(path));
                return;
            }

            var thumbnail = MediaStore.Images.Thumbnails.GetThumbnail(Context.ContentResolver, model.Id, ThumbnailKind.MiniKind, null);
            SetImageBitmap(thumbnail);

            var cursor = MediaStore.Images.Thumbnails.QueryMiniThumbnail(Context.ContentResolver, model.Id, ThumbnailKind.MiniKind, new[] { MediaStore.Images.Thumbnails.Data });
            if (cursor != null && cursor.Count > 0)
            {
                cursor.MoveToFirst();
                var thumbUri = cursor.GetString(0);
                Thumbnails.Add(model.Id, thumbUri);
                cursor.Close();
            }
        }

        private void ModelChanged()
        {
            Invalidate();
        }
    }
}