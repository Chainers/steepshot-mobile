using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Photos;
using PureLayout.Net;
using Steepshot.Core.Models.Enums;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class FolderTableViewSource : UITableViewSource
    {
        private readonly List<Tuple<string, PHFetchResult>> _albums;
        public event Action<ActionType, Tuple<string, PHFetchResult>> CellAction;
        private readonly List<AlbumTableViewCell> _cellsList = new List<AlbumTableViewCell>();

        public FolderTableViewSource(List<Tuple<string, PHFetchResult>> albums)
        {
            _albums = albums;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (AlbumTableViewCell)tableView.DequeueReusableCell(nameof(AlbumTableViewCell), indexPath);
            if (!cell.IsCellActionSet)
                cell.CellAction += CellAction;
            cell.UpdateCell(_albums[indexPath.Row]);
            if (!_cellsList.Any(c => c.Handle == cell.Handle))
                _cellsList.Add(cell);
            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _albums.Count;
        }

        public void FreeAllCells()
        {
            foreach (var item in _cellsList)
            {
                item.CellAction = null;
            }
        }
    }

    public class AlbumTableViewCell : UITableViewCell
    {
        private UILabel _name;
        private UILabel _count;
        private UIImageView _lastImage;
        private Tuple<string, PHFetchResult> _currentAlbum;
        public bool IsCellActionSet => CellAction != null;
        public Action<ActionType, Tuple<string, PHFetchResult>> CellAction;

        protected AlbumTableViewCell(IntPtr handle) : base(handle)
        {
            _lastImage = new UIImageView();
            _lastImage.ContentMode = UIViewContentMode.ScaleAspectFill;
            _lastImage.ClipsToBounds = true;
            ContentView.AddSubview(_lastImage);
            _lastImage.AutoSetDimensionsToSize(new CGSize(80, 80));
            _lastImage.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 10);
            _lastImage.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            var container = new UIStackView();
            container.Axis = UILayoutConstraintAxis.Vertical;
            ContentView.AddSubview(container);
            container.AutoAlignAxis(ALAxis.Horizontal, _lastImage);
            container.AutoPinEdge(ALEdge.Left, ALEdge.Right, _lastImage, 10);
            container.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            _name = new UILabel();
            _name.Font = Constants.Regular16;
            container.AddArrangedSubview(_name);

            _count = new UILabel();
            _count.Font = Constants.Regular14;
            container.AddArrangedSubview(_count);

            var tapGesture = new UITapGestureRecognizer(() => 
            {
                CellAction?.Invoke(ActionType.Tap, _currentAlbum);
            });
            ContentView.AddGestureRecognizer(tapGesture);
            ContentView.UserInteractionEnabled = true;
        }

        public void UpdateCell(Tuple<string, PHFetchResult> album)
        {
            _currentAlbum = album;
            _name.Text = _currentAlbum.Item1;
            _count.Text = _currentAlbum.Item2.Count.ToString();
            var PHImageManager = new PHImageManager();
            PHImageManager.RequestImageForAsset((PHAsset)_currentAlbum.Item2.LastObject, new CGSize(300, 300),
                                                PHImageContentMode.AspectFill, new PHImageRequestOptions()
                                                {
                                                    DeliveryMode = PHImageRequestOptionsDeliveryMode.Opportunistic,
                                                    ResizeMode = PHImageRequestOptionsResizeMode.Exact
                                                }, (img, info) =>
                                                {
                                                    _lastImage.Image = img;
                                                });
        }
    }
}
