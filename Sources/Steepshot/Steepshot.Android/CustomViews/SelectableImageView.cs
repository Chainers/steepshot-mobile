using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Net;
using Android.Widget;
using Steepshot.Utils;

namespace Steepshot.CustomViews
{
    public sealed class SelectableImageView : ImageView
    {
        private GalleryMediaModel _model;

        public void Bind(GalleryMediaModel model)
        {
            if (_model != null)
                _model.ModelChanged -= ModelChanged;
            _model = model;
            _model.ModelChanged += ModelChanged;
            if (!string.IsNullOrEmpty(model.Thumbnail))
                SetImageURI(Uri.Parse(model.Thumbnail));
            else
                SetImageBitmap(null);
        }

        private void ModelChanged()
        {
            Invalidate();
        }

        public SelectableImageView(Context context) : base(context)
        {
            Clickable = true;
            SetScaleType(ScaleType.CenterCrop);
        }

        private Paint _selectionPaint;
        private Paint SelectionPaint => _selectionPaint ?? (_selectionPaint = new Paint(PaintFlags.AntiAlias) { Color = Resources.GetColor(Resource.Color.rgb255_81_4), StrokeWidth = BitmapUtils.DpToPixel(6, Context.Resources) });

        private Paint _whitePaint;
        private Paint WhitePaint => _whitePaint ?? (_whitePaint = new Paint(PaintFlags.AntiAlias) { Color = Color.White, StrokeWidth = BitmapUtils.DpToPixel(1, Context.Resources), TextSize = BitmapUtils.DpToPixel(16, Context.Resources), TextAlign = Paint.Align.Center });

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
    }
}