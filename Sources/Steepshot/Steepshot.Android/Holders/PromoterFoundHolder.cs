using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Steepshot.Core.Extensions;
using System.Threading;
using Java.Lang;
using Android.OS;

namespace Steepshot.Holders
{
    public class PromoterFoundHolder : RecyclerView.ViewHolder
    {
        private readonly Context context;

        private Handler _handler;
        private ImageView _promoterAvatar;
        private TextView _promoterLogin;
        private TextView _expectedUpvoteTimeLabel;

        public PromoterFoundHolder(View itemView, Context context) : base(itemView)
        {
            this.context = context;
            InitializeView();
        }

        private void InitializeView()
        {
            _handler = new Handler(context.MainLooper);

            _promoterAvatar = ItemView.FindViewById<ImageView>(Resource.Id.promoter_avatar);

            _promoterLogin = ItemView.FindViewById<TextView>(Resource.Id.promoter_name);
            _promoterLogin.Typeface = Style.Semibold;

            var expectedTime = ItemView.FindViewById<TextView>(Resource.Id.expected_time);
            expectedTime.Typeface = Style.Regular;
            expectedTime.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ExpectedVoteTime);

            _expectedUpvoteTimeLabel = ItemView.FindViewById<TextView>(Resource.Id.expected_counter);
            _expectedUpvoteTimeLabel.Typeface = Style.Light;
        }

        public void UpdatePromoterInfo(PromoteResponse promoteInfo)
        {
            _promoterLogin.Text = $"@{promoteInfo.Bot.Author}";

            if (!string.IsNullOrEmpty(promoteInfo.Bot.Avatar))
                Picasso.With(context).Load(promoteInfo.Bot.Avatar.GetImageProxy(_promoterAvatar.LayoutParameters.Width, _promoterAvatar.LayoutParameters.Height))
                       .Placeholder(Resource.Drawable.ic_holder)
                       .NoFade()
                       .Priority(Picasso.Priority.Normal)
                       .Into(_promoterAvatar, null, null);
            else
                Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(_promoterAvatar);

            var expectedUpvoteTime = promoteInfo.ExpectedUpvoteTime;
            var timer = new Timer((obj) =>
            {
                expectedUpvoteTime = expectedUpvoteTime.Subtract(TimeSpan.FromSeconds(1));

                new Handler(Looper.MainLooper).Post(new Runnable(() => 
                {
                    if (expectedUpvoteTime.ToString().Length > 8)
                        _expectedUpvoteTimeLabel.Text = expectedUpvoteTime.ToString().Remove(8);
                    else
                        _expectedUpvoteTimeLabel.Text = expectedUpvoteTime.ToString();
                }));
            }, null, DateTime.Now.Add(expectedUpvoteTime).Millisecond, (int)TimeSpan.FromSeconds(1).TotalMilliseconds);
        }
    }
}
