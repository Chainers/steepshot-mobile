using System.Threading.Tasks;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.View;

namespace Steepshot.Presenter
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
