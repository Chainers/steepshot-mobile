using Android.Graphics;
using Android.Widget;
using System.Threading.Tasks;

namespace Steepshot.Utils
{
    public class AnimationHelper
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
    }
}