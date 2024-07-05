using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;

namespace GrpcDotNetNamedPipes.Extensions.Service
{
    public static class GrpcDotNetNamedPipesClientExtensions
    {
        public static void AddNamedPipeChannelOptions(this IServiceCollection serviceCollection, NamedPipeChannelOptions namedPipeChannelOptions)
        {
            serviceCollection.AddSingleton(namedPipeChannelOptions);
        }
        public static void AddNamedPipeChannelOptions(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<NamedPipeChannelOptions>();
        }
        public static void AddNamedPipeChannel(this IServiceCollection serviceCollection, string serverName, string pipeName)
        {
            serviceCollection.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<NamedPipeChannelOptions>();
                if (options != null)
                {
                    return new NamedPipeChannel(serverName, pipeName, options);
                }
                else
                {

                    return new NamedPipeChannel(serverName, pipeName);
                }
            });
        }
        public static void AddGrpcNamedPipesClient<TClient>(this IServiceCollection serviceCollection) where TClient : ClientBase<TClient>
        {
            serviceCollection.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<NamedPipeChannel>();
                if (options == null)
                {
                    throw new TypeLoadException($"{typeof(TClient)} requires param:{typeof(NamedPipeChannel)},Please ensure it has been registered!");
                }
                var clientInstance = ActivatorUtilities.CreateInstance<TClient>(sp);
                return clientInstance;
            });
        }
    }
}
