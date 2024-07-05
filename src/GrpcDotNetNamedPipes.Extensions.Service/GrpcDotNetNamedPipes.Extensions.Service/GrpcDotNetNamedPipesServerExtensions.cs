using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;
using System.Reflection;

namespace GrpcDotNetNamedPipes.Extensions.Service
{
    public static class GrpcDotNetNamedPipesServerExtensions
    {
        public static void AddGrpcNamedPipesService<TService>(this IServiceCollection serviceCollection) where TService : class
        {
            serviceCollection.AddSingleton(sp =>
            {
                var service = ActivatorUtilities.CreateInstance<TService>(sp)!;
                var server = sp.GetRequiredService<NamedPipeServer>();

                if (server == null)
                {
                    throw new TypeLoadException($"Please ensure the {typeof(NamedPipeServer)} has been registered!");
                }

                var bindServiceMethod = ResolveBindServiceMethod<TService>();
                bindServiceMethod.Invoke(null, [server.ServiceBinder, service]);
                return service;
            });
        }

        public static void AddGrpcNamedPipesServer(this IServiceCollection serviceCollection, string pipeName)
        {
            if (string.IsNullOrWhiteSpace(pipeName))
            {
                throw new ArgumentNullException(nameof(pipeName));
            }

            serviceCollection.AddKeyedSingleton(pipeName, 
                (sp, key) => 
                {
                    if (sp.GetRequiredKeyedService<NamedPipeServer>(key) != null)
                    {
                        throw new InvalidOperationException($"You have registered an instance of {typeof(NamedPipeServer)} with pipename:{key}");
                    }
                    return new NamedPipeServer(key!.ToString()); 
                });
        }
        /// <summary>
        /// Affects AOT compilation
        /// todo...
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        /// <exception cref="TypeLoadException"></exception>

        private static MethodInfo ResolveBindServiceMethod<TService>()
        {
            var serviceType = typeof(TService);
            var baseServiceType = serviceType.BaseType;

            if (baseServiceType == null)
            {
                throw new TypeLoadException($"{serviceType} is not a valid GrpcService type! It must have a base class.");
            }

            if (baseServiceType.IsNested)
            {
                Type declaringType = baseServiceType.DeclaringType!;

                if (declaringType.IsAbstract && declaringType.IsSealed && declaringType.IsClass)
                {
                    Console.WriteLine("Enclosing Static Class: " + declaringType);
                }

                MethodInfo? bindServiceMethod = declaringType.GetMethod("BindService", BindingFlags.Static | BindingFlags.Public);

                if (bindServiceMethod == null)
                {
                    throw new TypeLoadException($"{serviceType} is not a valid GrpcService type! There's no a static method named of 'BindService' found in {declaringType}");
                }

                return bindServiceMethod;
            }
            else
            {
                throw new TypeLoadException($"{serviceType} is not a valid GrpcService type! It's base class must be a nested class.");
            }
        }
    }
}
