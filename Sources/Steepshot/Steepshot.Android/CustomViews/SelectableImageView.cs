using Android.Content;
using Android.Graphics;
using Android.Provider;
using Android.Widget;
using Steepshot.Utils;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.OS;
using Handler = Android.OS.Handler;

namespace Steepshot.CustomViews
{
    public sealed class SelectableImageView : ImageView
    {
        private GalleryMediaModel _model;
        private readonly Handler _handler = new Handler(Looper.MainLooper);
        private Paint _selectionPaint;
        private Paint _whitePaint;
        private static readonly Drawable DefaultColor = new ColorDrawable(Style.R245G245B245);
        private readonly BitmapFactory.Options _options;

        private Paint SelectionPaint => _selectionPaint ?? (_selectionPaint = new Paint(PaintFlags.AntiAlias) { Color = Style.R255G81B4, StrokeWidth = BitmapUtils.DpToPixel(6, Context.Resources) });
        private Paint WhitePaint => _whitePaint ?? (_whitePaint = new Paint(PaintFlags.AntiAlias) { Color = Color.White, StrokeWidth = BitmapUtils.DpToPixel(1, Context.Resources), TextSize = BitmapUtils.DpToPixel(16, Context.Resources), TextAlign = Paint.Align.Center });


        public SelectableImageView(Context context) : base(context)
        {
            Clickable = true;
            SetScaleType(ScaleType.CenterCrop);
            _options = new BitmapFactory.Options
            {
                InPreferredConfig = Bitmap.Config.Rgb565
            };
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

        public async Task Bind(GalleryMediaModel model)
        {
            BitmapUtils.ReleaseBitmap(Drawable);
            SetImageDrawable(DefaultColor);

            _model = model;
            _model.ModelChanged = Invalidate;
            await LoadThumbnail(_model);
        }

        private Task LoadThumbnail(GalleryMediaModel model)
        {
            return Task.Run(() =>
            {
                var thumbnail = MediaStore.Images.Thumbnails.GetThumbnail(Context.ContentResolver, model.Id, ThumbnailKind.MiniKind, _options);
                if (thumbnail == null || _model == null || model.Id != _model.Id)
                    return;

                if (model.Orientation == 0)
                {
                    _handler.Post(() =>
                    {
                        if (_model == null || model.Id != _model.Id)
                            return;

                        BitmapUtils.ReleaseBitmap(Drawable);
                        SetImageBitmap(thumbnail);
                    });
                    return;
                }

                var matrix = new Matrix();
                matrix.PostRotate(model.Orientation);
                var oriThumbnail = Bitmap.CreateBitmap(thumbnail, 0, 0, thumbnail.Width, thumbnail.Height, matrix, false);

                BitmapUtils.ReleaseBitmap(thumbnail);

                _handler.Post(() =>
                {
                    if (_model == null || model.Id != _model.Id)
                        return;

                    BitmapUtils.ReleaseBitmap(Drawable);
                    SetImageBitmap(oriThumbnail);
                });
            });
        }
    }
}