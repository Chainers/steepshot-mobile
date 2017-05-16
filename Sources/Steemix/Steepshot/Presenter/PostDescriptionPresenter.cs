using System;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Responses;
using System.Threading.Tasks;

namespace Steepshot
{
	public class PostDescriptionPresenter : BasePresenter
	{
		public PostDescriptionPresenter(PostDescriptionView view):base(view)
		{
		}

		public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request)
		{
			return await Api.Upload(request);
		}
	}
}
