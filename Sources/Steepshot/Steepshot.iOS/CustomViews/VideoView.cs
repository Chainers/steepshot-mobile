using System;
using AVFoundation;
using CoreMedia;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Steepshot.iOS.CustomViews
{
    public class VideoView : UIView
    {
        private const string ObserveKey = "status";
        private AVPlayerItem item;
        private bool _isRegistered;
        private NSObject notificationToken;
        private bool _shouldPlay;
        public AVPlayerLayer PlayerLayer => Layer as AVPlayerLayer;

        public AVPlayer Player
        {
            get
            {
                return PlayerLayer.Player;
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

        public VideoView(bool isLoopNeeded)
        {
            Player = new AVPlayer();
            if (isLoopNeeded)
                notificationToken = AVPlayerItem.Notifications.ObserveDidPlayToEndTime(HandleEventHandler);
        }

        private async void HandleNotification(NSNotification obj)
        {
            await Player.SeekAsync(CMTime.Zero);
        }

        private async void HandleEventHandler(object sender, NSNotificationEventArgs e)
        {
            if (e.Notification?.Object?.Handle == item?.Handle)
            {
                await Player.SeekAsync(CMTime.Zero);
                Play();
            }
        }

        public void ChangeItem(string url)
        {
            if (_isRegistered)
            {
                item?.RemoveObserver(this, (NSString)ObserveKey);
                _isRegistered = false;
            }
            if (!string.IsNullOrEmpty(url))
            {
                item = new AVPlayerItem(NSUrl.FromString(url)); ;
                item.AddObserver(this, (NSString)ObserveKey, NSKeyValueObservingOptions.OldNew, Handle);
                _isRegistered = true;
                Player.ReplaceCurrentItemWithPlayerItem(item);
            }
            else
                Player.ReplaceCurrentItemWithPlayerItem(null);
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (_isRegistered)
            {
                item?.RemoveObserver(this, (NSString)ObserveKey);
                _isRegistered = false;
            }

            if (_shouldPlay)
                Player.Play();
        }

        public void Play()
        {
            _shouldPlay = true;
            if (Player.CurrentItem?.Status == AVPlayerItemStatus.ReadyToPlay &&
               Player.Status == AVPlayerStatus.ReadyToPlay &&
               PlayerLayer.ReadyForDisplay)
                Player.Play();
        }

        public void Stop()
        {
            _shouldPlay = false;
            if (Player.CurrentItem?.Status == AVPlayerItemStatus.ReadyToPlay &&
               Player.Status == AVPlayerStatus.ReadyToPlay &&
               PlayerLayer.ReadyForDisplay)
                Player.Pause();
        }

        protected override void Dispose(bool disposing)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(notificationToken);
            notificationToken?.Dispose();
            base.Dispose(disposing);
        }
    }
}
