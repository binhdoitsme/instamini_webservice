using InstaminiWebService.Models;
using InstaminiWebService.ResponseModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstaminiWebService.ResponseModels.Factory
{
    public class ResponseModelFactory : IResponseModelFactory
    {
        private static readonly Dictionary<Type, Type> TypeMap = new Dictionary<Type, Type>
        {
            { typeof(Comment), typeof(CommentResponse) },
            { typeof(Photo), typeof(PhotoResponse) },
            { typeof(Post), typeof(PostResponse) },
            { typeof(User), typeof(UserResponse) }
        };

        public IResponseModel<T> Create<T>(T input) where T : class
        {
            Type typeOfT = typeof(T);
            Type typeOfTWrapper = TypeMap[typeOfT];
            var constructorDelegate = typeOfTWrapper.GetConstructor(new Type[] { typeOfT });
            return (IResponseModel<T>) constructorDelegate.Invoke(new object[] { input });
        }
    }
}
