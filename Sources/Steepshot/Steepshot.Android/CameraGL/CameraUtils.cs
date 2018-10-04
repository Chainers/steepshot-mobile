#pragma warning disable 618
using Android.Hardware;

namespace Steepshot.CameraGL
{
    public static class CameraUtils
    {
        public static void ChoosePreviewSize(Camera.Parameters parms, int width, int height)
        {
            var ppsfv = parms.PreferredPreviewSizeForVideo;

            foreach (var size in parms.SupportedPreviewSizes)
            {
                if (size.Width == width && size.Height == height)
                {
                    parms.SetPreviewSize(width, height);
                    return;
                }
            }

            if (ppsfv != null)
            {
                parms.SetPreviewSize(ppsfv.Width, ppsfv.Height);
            }
        }

        /**
         * Attempts to find a fixed preview frame rate that matches the desired frame rate.
         * <p>
         * It doesn't seem like there's a great deal of flexibility here.
         * <p>
         * TODO: follow the recipe from http://stackoverflow.com/questions/22639336/#22645327
         *
         * @return The expected frame rate, in thousands of frames per second.
         */
        public static int ChooseFixedPreviewFps(Camera.Parameters parms, int desiredThousandFps)
        {
            var supported = parms.SupportedPreviewFpsRange;

            foreach (var entry in supported)
            {
                if (entry[0] == entry[1] && (entry[0] == desiredThousandFps))
                {
                    parms.SetPreviewFpsRange(entry[0], entry[1]);
                    return entry[0];
                }
            }

            int[] tmp = new int[2];
            parms.GetPreviewFpsRange(tmp);
            int guess;
            if (tmp[0] == tmp[1])
            {
                guess = tmp[0];
            }
            else
            {
                guess = tmp[1] / 2;     // shrug
            }

            return guess;
        }
    }
}