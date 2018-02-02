using Steepshot.Utils.Animations.Base;

namespace Steepshot.Utils.Animations
{
    public class Storyboard : BaseStoryboard
    {
        public Storyboard() : base(new AnimationTimer(), new UIInvoker()) { }
        public Storyboard(uint startAt) : base(startAt, new AnimationTimer(), new UIInvoker()) { }
    }
}