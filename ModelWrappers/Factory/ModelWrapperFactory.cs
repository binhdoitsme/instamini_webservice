using InstaminiWebService.Models;
using InstaminiWebService.ModelWrappers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstaminiWebService.ModelWrappers.Factory
{
    public class ModelWrapperFactory : IModelWrapperFactory
    {
        private static readonly Dictionary<Type, Type> TypeMap = new Dictionary<Type, Type>
        {
            { typeof(Comment), typeof(CommentWrapper) },
            { typeof(Photo), typeof(PhotoWrapper) },
            { typeof(Post), typeof(PostWrapper) }
        };

        public IModelWrapper<T> Create<T>(T input) where T : class
        {
            Type typeOfT = typeof(T);
            Type typeOfTWrapper = TypeMap[typeOfT];
            var constructorDelegate = typeOfTWrapper.GetConstructor(new Type[] { typeOfT });
            return (IModelWrapper<T>) constructorDelegate.Invoke(new object[] { input });
        }
    }
}
