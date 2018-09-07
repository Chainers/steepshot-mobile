using System;
using NUnit.Framework;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Extensions;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class FrameTest
    {
        private readonly Device[] _devices = {
            new Device(272, 340, 2.0f),
            new Device(280, 280, 1.5f),
            new Device(312, 390, 2.0f),
            new Device(320, 290, 1.3f),
            new Device(320, 320, 1.5f),
            new Device(320, 325, 1.3f),
            new Device(320, 330, 1.3f),
            new Device(320, 480, 1.0f),
            new Device(480, 854, 1.5f),
            new Device(640, 960, 2.0f),
            new Device(640, 1136, 2.0f),
            new Device(720, 1280, 2.0f),
            new Device(750, 1334, 2.0f),
            new Device(768, 1024, 1.0f),
            new Device(768, 1280, 2.0f),
            new Device(800, 1280, 1.0f),
            new Device(800, 1280, 1.3f),
            new Device(1080, 1920, 2.0f),
            new Device(1080, 1920, 2.6f),
            new Device(1080, 1920, 3.0f),
            new Device(1200, 1920, 2.0f),
            new Device(1366, 768, 1.0f),
            new Device(1440, 2560, 3.0f),
            new Device(1440, 2560, 4.0f),
            new Device(1440, 900, 1.0f),
            new Device(1440, 2560, 3.5f),
            new Device(1440, 2960, 4.0f),
            new Device(1536, 2048, 2.0f),
            new Device(1600, 2560, 2.0f),
            new Device(1920, 1080, 1.5f),
            new Device(1920, 1200, 2.0f),
            new Device(2048, 1536, 2.0f),
            new Device(2160, 1440, 1.5f),
            new Device(2304, 1440, 2.0f),
            new Device(2560, 1440, 1.0f),
            new Device(2560, 1600, 1.0f),
            new Device(2560, 1600, 2.0f),
            new Device(2560, 1700, 2.0f),
            new Device(2732, 2048, 2.0f),
            new Device(2736, 1824, 2.0f),
            new Device(2880, 1800, 2.0f),
            new Device(3000, 2000, 2.0f),
            new Device(5120, 2880, 2.0f),
        };

        [Ignore("longrun")]
        [Test]
        public void Test()
        {
            float topPanelHeightInDp = 50;
            float feedItemHeaderHeightInDp = 70;
            float tabBarHeightInDp = 50;

            for (var i = 0; i < _devices.Length; i++)
            {
                var device = _devices[i];
                var frameWidth = device.Width;
                var maxPostHeight = device.Height -
                                    (topPanelHeightInDp + feedItemHeaderHeightInDp + tabBarHeightInDp + 54) *
                                    device.Density;
                if (maxPostHeight <= 0)
                    continue;

                if (maxPostHeight <= 130 * device.Density)
                    continue;

                for (int imageWidh = Constants.ImageMinWidth; imageWidh <= 8192; imageWidh++)
                {
                    for (int imageHeight = Constants.ImageMinHeight; imageHeight <= 8192; imageHeight++)
                    {
                        var frameSize = new FrameSize(imageHeight, imageWidh);
                        var frameHeight = frameSize.OptimalPhotoSize(frameWidth, 130 * device.Density, maxPostHeight);

                        var dW = (float)frameWidth / imageWidh;
                        var dH = (float)frameHeight / imageHeight;
                        var delta = Math.Max(dW, dH);


                        int x = 0;
                        var newWidh = imageWidh;

                        if (dH > dW)
                        {
                            newWidh = (int)Math.Round(frameWidth * imageHeight / (float)frameHeight);
                            newWidh = Math.Min(newWidh, imageWidh);
                            x = (int)((imageWidh - newWidh) / 2.0);
                        }

                        int y = 0;
                        var newHeight = imageHeight;

                        if (dW > dH)
                        {
                            newHeight = (int)Math.Round(frameHeight * imageWidh / (float)frameWidth);
                            newHeight = Math.Min(newHeight, imageHeight);
                            y = (int)((imageHeight - newHeight) / 2.0);
                        }


                        Assert.IsTrue(newWidh + x <= imageWidh);
                        Assert.IsTrue(Math.Abs(newWidh * delta - frameWidth) <= Math.Max(device.Density, delta));

                        Assert.IsTrue(newHeight + y <= imageHeight);
                        Assert.IsTrue(Math.Abs(newHeight * delta - frameHeight) <= Math.Max(device.Density, delta));
                    }
                }
            }
        }


        public class Device
        {
            public int Width { get; set; }

            public int Height { get; set; }

            public float Density { get; set; }

            public long Count { get; set; }

            public double MaxWidthDelta { get; set; }

            public double MaxHeightDelta { get; set; }

            public Device(int width, int height, float density)
            {
                Width = width;
                Height = height;
                Density = density;
            }
        }


    }
}
