using System;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class PostTagsViewController : BaseViewControllerWithPresenter<TagsPresenter>
    {
        private TagsCollectionViewSource _collectionviewSource;
        private PostTagsTableViewSource _tagsSource;
        private Timer _timer;


        protected override void CreatePresenter()
        {
            _presenter = new TagsPresenter();
        }


        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();
            _timer = new Timer(OnTimer);
            //Table initialization
            _tagsSource = new PostTagsTableViewSource(_presenter);
            tagsTable.Source = _tagsSource;
            tagsTable.RegisterClassForCellReuse(typeof(UITableViewCell), "PostTagsCell");
            _tagsSource.RowSelectedEvent += TableTagSelected;

            //Collection view initialization
            tagsCollectionView.RegisterClassForCell(typeof(TagCollectionViewCell), nameof(TagCollectionViewCell));
            tagsCollectionView.RegisterNibForCell(UINib.FromName(nameof(TagCollectionViewCell), NSBundle.MainBundle), nameof(TagCollectionViewCell));
            // research flow layout
            //tagsCollectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
            //{
            //EstimatedItemSize = new CGSize(100, 50),

            //}, false);
            _collectionviewSource = new TagsCollectionViewSource();
            _collectionviewSource.RowSelectedEvent += CollectionTagSelected;
            tagsCollectionView.Source = _collectionviewSource;

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

            Activeview = searchText;

            var error = await _presenter.TryGetTopTags();

            if (error == null)
                tagsTable.ReloadData();

            ShowAlert(error);
        }

        private void TableTagSelected(int row)
        {
            var tag = _presenter[row];
            if (tag == null)
                return;
            AddTag(tag.Name);
        }

        private void CollectionTagSelected(int row)
        {
            _collectionviewSource.TagsCollection.RemoveAt(row);
            tagsCollectionView.ReloadData();
        }

        private async void OnTimer(object state)
        {
            await GetTags(searchText.Text);
        }

        private async Task GetTags(string query)
        {
            if (query != null && query.Length == 1)
                return;

            _presenter.Clear();
            Exception error;
            if (string.IsNullOrEmpty(query))
            {
                error = await _presenter.TryGetTopTags();
            }
            else
            {
                error = await _presenter.TryLoadNext(query);
            }

            if (error == null)
                tagsTable.ReloadData();
            ShowAlert(error);
        }

        private void AddTags(object sender, EventArgs e)
        {
            //TagsList.AddRange(_collectionviewSource.TagsCollection.Except(TagsList));
            //TagsList = TagsList.Take(4).ToList();
            NavigationController.PopViewController(true);
        }

        private void AddTagButtonClick(object sender, EventArgs e)
        {
            AddTag(searchText.Text);
            searchText.Text = "";
            searchText.ResignFirstResponder();
        }

        private void AddTag(string tag)
        {
            if (!_collectionviewSource.TagsCollection.Contains(tag) && !string.IsNullOrEmpty(tag))
            {
                _collectionviewSource.TagsCollection.Add(tag);
                tagsCollectionView.ReloadData();
            }
        }
    }
}
