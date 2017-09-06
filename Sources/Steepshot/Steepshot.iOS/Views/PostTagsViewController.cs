using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class PostTagsViewController : BaseViewControllerWithPresenter<TagsPresenter>
    {
        protected override void CreatePresenter()
        {
            _presenter = new TagsPresenter();
        }
        private TagsCollectionViewSource _collectionviewSource;
        private CancellationTokenSource _cts;
        private PostTagsTableViewSource _tagsSource = new PostTagsTableViewSource();
        private Timer _timer;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _timer = new Timer(OnTimer);
            //Table initialization
            tagsTable.Source = _tagsSource;
            tagsTable.RegisterClassForCellReuse(typeof(UITableViewCell), "PostTagsCell");
            GetTags(null);
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
        }

        private void TableTagSelected(int row)
        {
            AddTag(_tagsSource.Tags[row].Name);
        }

        private void CollectionTagSelected(int row)
        {
            _collectionviewSource.TagsCollection.RemoveAt(row);
            tagsCollectionView.ReloadData();
        }

        private void OnTimer(object state)
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
                _cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {

            }

            try
            {
                using (_cts = new CancellationTokenSource())
                {
                    OperationResult<SearchResponse<SearchResult>> response;
                    if (string.IsNullOrEmpty(query))
                    {
                        response = await _presenter.GetTopTags();
                    }
                    else
                    {
                        response = await _presenter.SearchTags(query);
                    }
                    if (response.Success)
                    {
                        _tagsSource.Tags.Clear();
                        _tagsSource.Tags = response.Result?.Results;
                        tagsTable.ReloadData();
                    }
                    else
                        Reporter.SendCrash(Localization.Errors.PostTagsError + response.Errors[0], BasePresenter.User.Login, AppVersion);
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, BasePresenter.User.Login, AppVersion);
            }
        }

        private void AddTags(object sender, EventArgs e)
        {
            TagsList.AddRange(_collectionviewSource.TagsCollection.Except(TagsList));
            TagsList = TagsList.Take(4).ToList();
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
