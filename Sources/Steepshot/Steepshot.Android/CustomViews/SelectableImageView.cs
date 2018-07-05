using Android.Content;
using Android.Graphics;
using Android.Provider;
using Android.Widget;
using Steepshot.Utils;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.OS;

namespace Steepshot.CustomViews
{
    public sealed class SelectableImageView : ImageView
    {
        private GalleryMediaModel _model;
        private CancellationTokenSource _cts;
        private readonly Handler _handler = new Handler(Looper.MainLooper);
        private Paint _selectionPaint;
        private Paint _whitePaint;

        private Paint SelectionPaint => _selectionPaint ?? (_selectionPaint = new Paint(PaintFlags.AntiAlias) { Color = Style.R255G81B4, StrokeWidth = BitmapUtils.DpToPixel(6, Context.Resources) });
        private Paint WhitePaint => _whitePaint ?? (_whitePaint = new Paint(PaintFlags.AntiAlias) { Color = Color.White, StrokeWidth = BitmapUtils.DpToPixel(1, Context.Resources), TextSize = BitmapUtils.DpToPixel(16, Context.Resources), TextAlign = Paint.Align.Center });


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

            if (_model.MultySelect)
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
            {
                _model.ModelChanged -= ModelChanged;
                (Drawable as BitmapDrawable)?.Bitmap?.Recycle();
                SetImageDrawable(new ColorDrawable(Style.R245G245B245));
            }

            _model = model;
            _model.ModelChanged += ModelChanged;
            LoadThumbnail(_model);
        }

        private void LoadThumbnail(GalleryMediaModel model)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
                _cts.Cancel();

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(() =>
            {
                if (token.IsCancellationRequested)
                    return;

                var thumbnail = MediaStore.Images.Thumbnails.GetThumbnail(Context.ContentResolver, model.Id, ThumbnailKind.MiniKind, null);

                var matrix = new Matrix();
                matrix.PostRotate(model.Orientation * 45);
                var oriThumbnail = Bitmap.CreateBitmap(thumbnail, 0, 0, thumbnail.Width, thumbnail.Height, matrix, true);

                if (token.IsCancellationRequested)
                {
                    thumbnail.Recycle();
                    oriThumbnail.Recycle();
                    return;
                }

                _handler.Post(() => SetImageBitmap(oriThumbnail));
            }, token);
        }

        private void ModelChanged()
        {
            Invalidate();
        }
    }
}