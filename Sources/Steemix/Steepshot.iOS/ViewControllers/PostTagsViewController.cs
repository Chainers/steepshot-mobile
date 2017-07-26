/*
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
    public partial class PostTagsViewController : BaseViewController
    {
        protected PostTagsViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        private TagsCollectionViewSource collectionviewSource;
		private CancellationTokenSource cts;
        private PostTagsTableViewSource tagsSource = new PostTagsTableViewSource();
		private Timer _timer;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
			_timer = new Timer(onTimer);
            //Table initialization
            tagsTable.Source = tagsSource;
            tagsTable.RegisterClassForCellReuse(typeof(UITableViewCell), "PostTagsCell");
            GetTags(null);
            tagsSource.RowSelectedEvent += TableTagSelected;

            //Collection view initialization
            tagsCollectionView.RegisterClassForCell(typeof(TagCollectionViewCell), nameof(TagCollectionViewCell));
            tagsCollectionView.RegisterNibForCell(UINib.FromName(nameof(TagCollectionViewCell), NSBundle.MainBundle), nameof(TagCollectionViewCell));
            // research flow layout
            //tagsCollectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
            //{
                //EstimatedItemSize = new CGSize(100, 50),
                
            //}, false);
            collectionviewSource = new TagsCollectionViewSource();
            collectionviewSource.RowSelectedEvent += CollectionTagSelected;
            tagsCollectionView.Source = collectionviewSource;

            addTagButton.TouchDown += AddTagButtonClick;
            addTagsButton.TouchDown += AddTags;

            searchText.ShouldReturn += (textField) =>
            {
                searchText.ResignFirstResponder();
                return true;
            };

            searchText.EditingChanged += (sender, e) =>
            {
                _timer.Change(1500, Timeout.Infinite);
            };

			activeview = searchText;
        }

        private void TableTagSelected(int row)
        {
            AddTag(tagsSource.Tags[row].Name);
        }

        private void CollectionTagSelected(int row)
        {
            collectionviewSource.tagsCollection.RemoveAt(row);
            tagsCollectionView.ReloadData();
        }

		private void onTimer(object state)
		{
			InvokeOnMainThread(() =>
		   {
			   GetTags(searchText.Text);
		   });
		}

        private async Task GetTags(string query)
        {
			if (query != null && query.Length == 1)
				return;

			try
			{
				cts?.Cancel();
			}
			catch (ObjectDisposedException)
			{

			}

			try
			{
				using (cts = new CancellationTokenSource())
				{
					OperationResult<SearchResponse<SearchResult>> response;
					if (string.IsNullOrEmpty(query))
					{
						var request = new SearchRequest() { };
						response = await Api.GetCategories(request, cts);
					}
					else
					{
						var request = new SearchWithQueryRequest(query) { SessionId = UserContext.Instanse.Token };
						response = await Api.SearchCategories(request, cts);
					}
					if (response.Success)
					{
						tagsSource.Tags.Clear();
						tagsSource.Tags = response.Result?.Results;
						tagsTable.ReloadData();
					}
					else
						Reporter.SendCrash("Post tags page get items error: " + response.Errors[0]);
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
        }

        private void AddTags(object sender, EventArgs e)
        {
            UserContext.Instanse.TagsList.AddRange(collectionviewSource.tagsCollection.Except(UserContext.Instanse.TagsList));
            UserContext.Instanse.TagsList = UserContext.Instanse.TagsList.Take(4).ToList();
            this.NavigationController.PopViewController(true);
        }

        private void AddTagButtonClick(object sender, EventArgs e)
        {
            AddTag(searchText.Text);
            searchText.Text = "";
            searchText.ResignFirstResponder();
        }

        private void AddTag(string tag)
        {
            if (!collectionviewSource.tagsCollection.Contains(tag) &&!string.IsNullOrEmpty(tag))
            {
                collectionviewSource.tagsCollection.Add(tag);
                tagsCollectionView.ReloadData();
            }
        }
    }
}
*/
