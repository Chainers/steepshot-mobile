using System;
using AVFoundation;
using CoreMedia;
using Foundation;
using ObjCRuntime;
using UIKit;
using PureLayout.Net;
using CoreGraphics;
using System.Threading.Tasks;

namespace Steepshot.iOS.CustomViews
{
    public class VideoView : UIView
    {
        private const string ObserveKey = "status";
        private AVPlayerItem item;
        private NSObject notificationToken;
        private UILabel _timerLabel;
        private bool _isRegistered;
        private bool _shouldPlay;
        private bool _showTimer;
        private bool _looped;
        public AVPlayerLayer PlayerLayer => Layer as AVPlayerLayer;
        public Action OnVideoStop;

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

        public VideoView(bool isLoopNeeded, bool showTime)
        {
            Player = new AVPlayer();
            _looped = isLoopNeeded;
            _showTimer = showTime;
            notificationToken = AVPlayerItem.Notifications.ObserveDidPlayToEndTime(HandleEventHandler);

            if (_showTimer)
                SetupTimer();
        }

        private void SetupTimer()
        {
            _timerLabel = new UILabel();
            _timerLabel.Font = Helpers.Constants.Semibold14;
            _timerLabel.TextColor = UIColor.White;
            _timerLabel.UserInteractionEnabled = false;
            _timerLabel.Hidden = false;
            AddSubview(_timerLabel);

            _timerLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            _timerLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 20);

            var interval = new CMTime(1, 2);
            double timeLeft;
            Player.AddPeriodicTimeObserver(interval, CoreFoundation.DispatchQueue.MainQueue, (time) =>
            {
                if (item.Status == AVPlayerItemStatus.ReadyToPlay)
                {
                    timeLeft = item.Duration.Seconds - item.CurrentTime.Seconds;
                    _timerLabel.Text = TimeSpan.FromSeconds(timeLeft).ToString("mm\\:ss");
                }
            });
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

                if (_looped)
                    Play();
                else
                    Stop();
            }
        }

        public void ChangeItem(string url)
        {
            ChangeItem(NSUrl.FromString(url));
        }

        public void ChangeItem(NSUrl url)
        {
            if (_isRegistered)
            {
                item?.RemoveObserver(this, (NSString)ObserveKey);
                _isRegistered = false;
            }

            if (url != null)
            {
                item = new AVPlayerItem(url);
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
            {
                Player.Play();
            }
        }

        public void Stop()
        {
            OnVideoStop?.Invoke();
            _shouldPlay = false;
            if (Player.CurrentItem?.Status == AVPlayerItemStatus.ReadyToPlay &&
               Player.Status == AVPlayerStatus.ReadyToPlay &&
               PlayerLayer.ReadyForDisplay)
            {
                Player.Pause();
            }
        }

        protected override void Dispose(bool disposing)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(notificationToken);
            notificationToken?.Dispose();
            base.Dispose(disposing);
        }
    }
}
