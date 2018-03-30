namespace Steepshot.CustomViews
{
    public interface ICoordinatorListener
    {

        bool IsBeingDragged { get; }
        bool OnCoordinateScroll(int x, int y, int deltaX, int deltaY, bool isScrollToTop);
        void OnSwitch();
    }
}