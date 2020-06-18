using InstaminiWebService.ResponseModels.Base;

namespace InstaminiWebService.ResponseModels.Factory
{
    public interface IResponseModelFactory
    {
        public IResponseModel<T> Create<T>(T input) where T : class;
    }
}
