using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Sweetshot.Library.Models.Responses;

namespace Steemix.Droid.Activities
{
	[Activity(Label = "TagsActivity",WindowSoftInputMode=Android.Views.SoftInput.AdjustNothing)]
	public class TagsActivity : BaseActivity<ViewModels.TagsViewModel>
	{
		[InjectView(Resource.Id.ic_close)]
		ImageButton Close;

		[InjectView(Resource.Id.search_box)]
		EditText SearchBox;

		[InjectView(Resource.Id.tags_list)]
		RecyclerView TagsList;

		[InjectView(Resource.Id.tag_container)]
		TagLayout tagLayout;

		[InjectView(Resource.Id.scroll)]
		ScrollView Scroll;

		Adapter.TagsAdapter Adapter;

		List<Category> SelectedCategories = new List<Category>();

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.lyt_tags);

			Cheeseknife.Inject(this);

			TagsList.SetLayoutManager(new LinearLayoutManager(this));
			Adapter = new Adapter.TagsAdapter(this);
			TagsList.SetAdapter(Adapter);

			Close.Click +=(sender,e)=> OnBackPressed();

			SearchBox.TextChanged += (sender, e) =>
			{
				if (SearchBox.Text.Length > 3)
				{
					ViewModel.SearchTags(SearchBox.Text).ContinueWith((arg) => {
							Adapter.Reset(arg.Result.Result.Results);
							RunOnUiThread(() => Adapter.NotifyDataSetChanged());
						System.Console.WriteLine(arg.Result.Result.Results.Count);
					});
				}
			};

			Adapter.Click += Adapter_Click;
		}

		void Adapter_Click(int obj)
		{
			if (SelectedCategories.Find((finded) => finded.Name.Equals(Adapter.GetItem(obj).Name)) == null)
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
	}
}
