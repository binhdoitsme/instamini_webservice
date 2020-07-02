using InstaminiWebService.Database;
using InstaminiWebService.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace InstaminiWebService.Repositories
{
    public class RepositoryFactory
    {
        private static readonly IDictionary<Type, Type> REPOSITORY_TYPES = new Dictionary<Type, Type>()
        {
            { typeof(User), typeof(UserRepository) },
            { typeof(Post), typeof(PostRepository) },
            { typeof(Comment), typeof(CommentRepository) }
        };

        private readonly InstaminiContext DbContext;
        private readonly IConfiguration Configuration;

        public RepositoryFactory(InstaminiContext dbContext, IConfiguration configuration)
        {
            DbContext = dbContext;
            Configuration = configuration;
        }

        public Repository<T> GetRepository<T>() where T : class
        {
            var targetType = typeof(T);
            if (!REPOSITORY_TYPES.ContainsKey(targetType))
            {
                throw new InvalidOperationException("Repository is not supported for type " + targetType.Name);
            }
            var repositoryType = REPOSITORY_TYPES[targetType];
            var constructorDelegate = repositoryType.GetConstructor(new Type[] { typeof(InstaminiContext), typeof(IConfiguration) });
            var instance = constructorDelegate.Invoke(new object[] { DbContext, Configuration });
            return (Repository<T>)instance;
        }
    }
}
