using Marshmallow.HotChocolate.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Marshmallow.HotChocolate
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQueryProjection(this IServiceCollection services)
        {
            services.AddTransient<IQueryProjection, QueryProjection>();
            return services;
        }
    }
}
