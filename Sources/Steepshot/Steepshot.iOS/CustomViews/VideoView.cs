using System;
using AVFoundation;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Steepshot.iOS.CustomViews
{
    public class VideoView : UIView
    {
        public AVPlayerLayer PlayerLayer => Layer as AVPlayerLayer;

        public AVQueuePlayer Player
        {
            get
            {
                return PlayerLayer.Player as AVQueuePlayer;
            }
            set
            {
                PlayerLayer.Player = value;
            }
        }

        [Export("layerClass")]
        public static Class LayerClass()
        {
            return new Class(typeof(AVPlayerLayer));
        }
    }
}
