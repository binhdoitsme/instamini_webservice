using InstaminiWebService.ModelWrappers.Base;

namespace InstaminiWebService.ModelWrappers.Factory
{
    public interface IModelWrapperFactory
    {
        public IModelWrapper<T> Create<T>(T input) where T : class;
    }
}
