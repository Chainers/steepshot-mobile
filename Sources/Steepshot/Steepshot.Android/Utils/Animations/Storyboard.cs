using Steepshot.Utils.Animations.Base;

namespace Steepshot.Utils.Animations
{
    public class Storyboard : BaseStoryboard
    {
        public Storyboard() : base(new AnimationTimer()) { }
        public Storyboard(uint startAt) : base(startAt, new AnimationTimer()) { }
    }
}