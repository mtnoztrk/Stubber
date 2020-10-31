using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StubberProject.Helpers;
using StubberProject.Models;

namespace StubberProject.Extensions
{
    public static class StubberExtensions
    {
        public static void UseStubber(this IServiceCollection services, IConfiguration configuration)
        {
            //services.Configure<StubberOption>(x => configuration.GetSection("StubberConfig").Bind(x));
            services.AddSingleton<IProcessor, DefaultProcessor>();
            services.AddSingleton<IOutputter, DefaultOutputter>();
            services.AddSingleton<IStubberManager, StubberManager>();
        }

        public static void UseStubber(this IApplicationBuilder app)
        {
            var services = app.ApplicationServices;

            //var timezoneSetting = services.GetService<IOptions<StubberOption>>();
            //ServiceLocator.RegisterService(timezoneSetting);

            ServiceLocator.RegisterService(services.GetService<IProcessor>());
            ServiceLocator.RegisterService(services.GetService<IOutputter>());
            ServiceLocator.RegisterService(services.GetService<IStubberManager>());
        }
    }
}
