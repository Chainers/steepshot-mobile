using System;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
	public abstract class BaseTableSource<T> : UITableViewSource
	{
		public delegate void ScrolledToBottomHandler();
		public event ScrolledToBottomHandler ScrolledToBottom;
		public List<T> TableItems = new List<T>();

		public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
		{
			if (indexPath.Row == TableItems.Count - 1 && ScrolledToBottom != null)
			{
				ScrolledToBottom();
			}
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return TableItems.Count;
		}
	}
}
