using InstaminiWebService.Models;
using InstaminiWebService.ResponseModels.Base;

namespace InstaminiWebService.ResponseModels
{
    public class PhotoResponse : IResponseModel<Photo>
    {
        public string Link { get; private set; }

        public PhotoResponse(Photo photo)
        {
            Link = $"/photos/{photo.FileName}";
        }
    }
}
