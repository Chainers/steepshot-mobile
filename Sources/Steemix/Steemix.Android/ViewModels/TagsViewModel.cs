using System;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steemix.Droid.ViewModels
{
	public class TagsViewModel :MvvmViewModelBase
	{
		public TagsViewModel()
		{
		}

		public async Task<OperationResult<CategoriesResponse>> SearchTags(string s)
		{ 
			 // Arrange
            var request = new SearchCategoriesRequest(s);

			// Act
			return await ViewModelLocator.Api.SearchCategories(request);
		}
	}
}
