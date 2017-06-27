using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using System;
using Android.Content;
using System.Linq;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{
	[Activity(Label = "TagsActivity",ScreenOrientation =Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode=Android.Views.SoftInput.AdjustNothing)]
	public class TagsActivity : BaseActivity, TagsView
	{
		TagsPresenter presenter;
#pragma warning disable 0649, 4014
		[InjectView(Resource.Id.ic_close)] ImageButton Close;
		[InjectView(Resource.Id.search_box)] EditText SearchBox;
		[InjectView(Resource.Id.tags_list)] RecyclerView TagsList;
		[InjectView(Resource.Id.tag_container)] TagLayout tagLayout;
		[InjectView(Resource.Id.scroll)] ScrollView Scroll;
#pragma warning restore 0649

        [InjectOnClick(Resource.Id.btn_post)]
        public void PostTags(object sender, EventArgs e)
        {
            if (SelectedCategories.Count > 0)
            {
                Intent returnIntent = new Intent();
                Bundle b = new Bundle();
                b.PutStringArray("TAGS", SelectedCategories.Select(o => o.Name).ToArray<string>());
                returnIntent.PutExtra("TAGS", b);
                SetResult(Android.App.Result.Ok,returnIntent);
                Finish();
            }
        }

		TagsAdapter Adapter;

		List<SearchResult> SelectedCategories = new List<SearchResult>();

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.lyt_tags);

			Cheeseknife.Inject(this);

			TagsList.SetLayoutManager(new LinearLayoutManager(this));
			Adapter = new TagsAdapter(this);
			TagsList.SetAdapter(Adapter);

			Close.Click +=(sender,e)=> OnBackPressed();

			SearchBox.TextChanged += (sender, e) =>
			{
				if (SearchBox.Text.Length > 1)
				{
					presenter.SearchTags(SearchBox.Text).ContinueWith((arg) => {
							Adapter.Reset(arg.Result.Result.Results);
							RunOnUiThread(() => Adapter.NotifyDataSetChanged());
					});
				}
			};

			Adapter.Click += Adapter_Click;

			presenter.GetTopTags().ContinueWith((arg) => {
                Adapter.Reset(arg.Result.Result.Results);
                RunOnUiThread(() => Adapter.NotifyDataSetChanged());
            });
        }

		void Adapter_Click(int obj)
		{
			if (SelectedCategories.Count < 4 && SelectedCategories.Find((finded) => finded.Name.Equals(Adapter.GetItem(obj).Name)) == null)
			{
				SelectedCategories.Add(Adapter.GetItem(obj));
				AddTag(Adapter.GetItem(obj).Name);
			}
		}

		public void AddTag(string s)
		{
			FrameLayout _add = (FrameLayout)LayoutInflater.Inflate(Resource.Layout.lyt_tag, null, false);
			var text = _add.FindViewById<TextView>(Resource.Id.text);
			text.Text = string.Format("#{0}", s);
			text.Click += (sender, e) => DeleteTag(text);
			tagLayout.AddView(_add);
			Scroll.RequestLayout();

		}

		void DeleteTag(TextView t)
		{ 
			SelectedCategories.RemoveAt(SelectedCategories.FindIndex((obj) => obj.Name.Equals(t.Text.Remove(0,1))));
			tagLayout.RemoveView((FrameLayout)t.Parent);
		}

		protected override void CreatePresenter()
		{
			presenter = new TagsPresenter(this);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Cheeseknife.Reset(this);
		}
	}
}
