using System;
using System.Linq;
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

        private PostTagsTableViewSource tagsSource = new PostTagsTableViewSource();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Table initialization
            tagsTable.Source = tagsSource;
            tagsTable.RegisterClassForCellReuse(typeof(UITableViewCell), "PostTagsCell");
            GetTags(null);
            tagsSource.RowSelectedEvent += TableTagSelected;

            //Collection view initialization
            tagsCollectionView.RegisterClassForCell(typeof(TagCollectionViewCell), nameof(TagCollectionViewCell));
            tagsCollectionView.RegisterNibForCell(UINib.FromName(nameof(TagCollectionViewCell), NSBundle.MainBundle), nameof(TagCollectionViewCell));
            // research flow layout
            /*tagsCollectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
            {
                EstimatedItemSize = new CGSize(100, 50),
                
            }, false);*/
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
                GetTags(((UITextField)sender).Text);
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

        private async Task GetTags(string query)
        {
			try
			{
				OperationResult<SearchResponse> response;
				if (string.IsNullOrEmpty(query))
				{
					var request = new SearchRequest() { };
					response = await Api.GetCategories(request);
				}
				else
				{
					var request = new SearchWithQueryRequest(query) { SessionId = UserContext.Instanse.Token };
					response = await Api.SearchCategories(request);
				}
				if (response.Success)
				{
					tagsSource.Tags.Clear();
					tagsSource.Tags = response.Result.Results;
					tagsTable.ReloadData();
				}
			}
			catch (Exception ex)
			{ 
			}
        }

        private void AddTags(object sender, EventArgs e)
        {
            UserContext.Instanse.TagsList.AddRange(collectionviewSource.tagsCollection.Except(UserContext.Instanse.TagsList));
            UserContext.Instanse.TagsList = UserContext.Instanse.TagsList.Take(3).ToList();
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
            if (!tag.StartsWith("#", StringComparison.CurrentCulture))
                tag = tag.Insert(0, "#");

            if (!collectionviewSource.tagsCollection.Contains(tag) &&!string.IsNullOrEmpty(tag))
            {
                collectionviewSource.tagsCollection.Add(tag);
                tagsCollectionView.ReloadData();
            }
        }
    }
}

