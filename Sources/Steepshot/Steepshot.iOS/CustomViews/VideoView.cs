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
        private const string ObserveBufferEmptyKey = "playbackBufferEmpty";
        private const string ObserveLikelyToKeepUpKey = "playbackLikelyToKeepUp";
        private const string ObserveBufferFullKey = "playbackBufferFull";
        private AVPlayerItem item;
        private NSObject notificationToken;
        private UILabel _timerLabel;
        private UIActivityIndicatorView _videoLoader;
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

        public VideoView(CGRect frame, bool isLoopNeeded, bool showTime) : this (isLoopNeeded, showTime)
        {
            Frame = frame;
        }

        private void SetupTimer()
        {
            _timerLabel = new UILabel();
            _timerLabel.Font = Helpers.Constants.Semibold14;
            _timerLabel.TextColor = UIColor.White;
            _timerLabel.UserInteractionEnabled = false;
            _timerLabel.Hidden = false;
            AddSubview(_timerLabel);

            _videoLoader = new UIActivityIndicatorView();
            _videoLoader.Color = UIColor.DarkGray;
            _videoLoader.HidesWhenStopped = true;
            AddSubview(_videoLoader);
            _videoLoader.StartAnimating();

            _timerLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            _timerLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 20);
            _videoLoader.AutoCenterInSuperview();

            var interval = new CMTime(1, 1);
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
                item?.RemoveObserver(this, (NSString)ObserveBufferEmptyKey);
                item?.RemoveObserver(this, (NSString)ObserveLikelyToKeepUpKey);
                item?.RemoveObserver(this, (NSString)ObserveBufferFullKey);
                _isRegistered = false;
            }

            if (url != null)
            {
                item = new AVPlayerItem(url);
                item.AddObserver(this, (NSString)ObserveKey, NSKeyValueObservingOptions.OldNew, Handle);
                item.AddObserver(this, (NSString)ObserveBufferEmptyKey, NSKeyValueObservingOptions.OldNew, Handle);
                item.AddObserver(this, (NSString)ObserveLikelyToKeepUpKey, NSKeyValueObservingOptions.OldNew, Handle);
                item.AddObserver(this, (NSString)ObserveBufferFullKey, NSKeyValueObservingOptions.OldNew, Handle);
                _isRegistered = true;
                Player.ReplaceCurrentItemWithPlayerItem(item);
            }
            else
                Player.ReplaceCurrentItemWithPlayerItem(null);
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            switch (keyPath)
            {
                case ObserveBufferEmptyKey:
                    _videoLoader?.StartAnimating();
                    break;
                case ObserveLikelyToKeepUpKey:
                case ObserveBufferFullKey:
                    _videoLoader?.StopAnimating();
                    if (_shouldPlay)
                        Player.Play();
                    break;
                default:
                    if (_isRegistered)
                    {
                        item?.RemoveObserver(this, (NSString)ObserveKey);
                        _isRegistered = false;
                    }

                    if (_shouldPlay)
                    {
                        _videoLoader?.StartAnimating();
                        Player.Play();
                    }
                    break;
            }
        }

        public void Play()
        {
            _shouldPlay = true;
            if (Player.CurrentItem?.Status == AVPlayerItemStatus.ReadyToPlay &&
                Player.Status == AVPlayerStatus.ReadyToPlay &&
                PlayerLayer.ReadyForDisplay)
            {
                _videoLoader?.StopAnimating();
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
