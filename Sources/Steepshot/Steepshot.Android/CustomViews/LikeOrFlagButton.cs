using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Steepshot.Core.Models.Common;

namespace Steepshot.CustomViews
{
    public class LikeOrFlagButton : ImageButton
    {
        private CancellationSignal _isAnimationRuning;

        #region constructors

        protected LikeOrFlagButton(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public LikeOrFlagButton(Context context) : base(context)
        {
        }

        public LikeOrFlagButton(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public LikeOrFlagButton(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public LikeOrFlagButton(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }       

        #endregion
       
        public async Task UpdateLikeAsync(Post post)
        {
            if (post.VoteChanging || post.FlagChanging)
            {
                await LikeSetAsync(post.FlagChanging)
                    .ConfigureAwait(true);
                return;
            }

            if (_isAnimationRuning != null && !_isAnimationRuning.IsCanceled)
            {
                _isAnimationRuning.Cancel();
                _isAnimationRuning = null;
                ScaleX = 1f;
                ScaleY = 1f;
            }

            if (post.Vote)
                SetImageResource(post.IsEnableVote ? Resource.Drawable.ic_new_like_filled : Resource.Drawable.ic_new_like_disabled);
            else if (post.Flag)
                SetImageResource(post.IsEnableVote ? Resource.Drawable.ic_flag_active : Resource.Drawable.ic_flag);
            else
                SetImageResource(post.IsEnableVote ? Resource.Drawable.ic_new_like_selected : Resource.Drawable.ic_new_like);
        }

        private async Task LikeSetAsync(bool isFlag)
        {
            if (_isAnimationRuning != null && !_isAnimationRuning.IsCanceled)
                return;

            _isAnimationRuning = new CancellationSignal();

            try
            {
                ScaleX = 0.7f;
                ScaleY = 0.7f;

                SetImageResource(isFlag ? Resource.Drawable.ic_flag_active : Resource.Drawable.ic_new_like_filled);

                var tick = 0;
                do
                {
                    if (_isAnimationRuning.IsCanceled)
                        return;

                    tick++;

                    var mod = tick % 6;
                    if (mod != 5)
                    {
                        ScaleX += 0.05f;
                        ScaleY += 0.05f;
                    }
                    else
                    {
                        ScaleX = 0.7f;
                        ScaleY = 0.7f;
                    }

                    await Task.Delay(100)
                        .ConfigureAwait(true);

                } while (true);
            }
            catch
            {
                //todo nothing
            }
        }
    }
}