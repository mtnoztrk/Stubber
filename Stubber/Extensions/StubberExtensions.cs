using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StubberProject.Helpers;
using StubberProject.Models;

namespace StubberProject.Extensions
{
    public static class StubberExtensions
    {
        public static void UseStubber(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<StubberOption>(configuration.GetSection("Stubber"));
            services.AddMemoryCache();
            services.AddSingleton<IProcessor, DefaultProcessor>();
            services.AddSingleton<IOutputter, DefaultOutputter>();
            services.AddSingleton<IStubberManager, StubberManager>();
        }

        public static void UseStubber(this IApplicationBuilder app)
        {
            var services = app.ApplicationServices;
            ServiceLocator.RegisterService(services.GetService<IStubberManager>());
        }
    }
}
