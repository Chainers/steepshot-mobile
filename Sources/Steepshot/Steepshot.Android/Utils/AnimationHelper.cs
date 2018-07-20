using Android.Graphics;
using Android.Widget;
using System.Threading.Tasks;
using Android.OS;

namespace Steepshot.Utils
{
    public sealed class AnimationHelper
    {
        public static async Task PulseGridItem(ImageView imageView)
        {
            var animatedValue = 0;
            var loop = 3;
            do
            {
                imageView.SetColorFilter(Color.Argb(animatedValue, 255, 255, 255), PorterDuff.Mode.Multiply);
                animatedValue += 40;
                await Task.Delay(100);
                if (animatedValue > 100)
                {
                    animatedValue = 0;
                    loop--;
                }
            } while (loop > 0);
            imageView.ClearColorFilter();
        }

        public static async Task PulseLike(ImageButton likeOrFlag, bool isFlag, CancellationSignal signal)
        {
            try
            {
                likeOrFlag.ScaleX = 0.7f;
                likeOrFlag.ScaleY = 0.7f;

                if (isFlag)
                    likeOrFlag.SetImageResource(Resource.Drawable.ic_flag_active);
                else
                    likeOrFlag.SetImageResource(Resource.Drawable.ic_new_like_filled);

                var tick = 0;
                do
                {
                    if (signal.IsCanceled)
                        return;

                    tick++;

                    var mod = tick % 6;
                    if (mod != 5)
                    {
                        likeOrFlag.ScaleX += 0.05f;
                        likeOrFlag.ScaleY += 0.05f;
                    }
                    else
                    {
                        likeOrFlag.ScaleX = 0.7f;
                        likeOrFlag.ScaleY = 0.7f;
                    }

                    await Task.Delay(100);

                } while (true);
            }
            catch
            {
                //todo nothing
            }
        }
    }
}