using InstaminiWebService.Models;
using InstaminiWebService.ModelWrappers.Base;

namespace InstaminiWebService.ModelWrappers
{
    public class PhotoWrapper : IModelWrapper<Photo>
    {
        public string Link { get; private set; }

        public PhotoWrapper(Photo photo)
        {
            Link = $"/photos/{photo.FileName}";
        }
    }
}
