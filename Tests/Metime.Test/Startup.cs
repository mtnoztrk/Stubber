using Metime.Helpers;
using Metime.Test.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Metime.Test
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TestDbContext>(options =>
            {
                options.UseInMemoryDatabase("test");
            }, ServiceLifetime.Transient);

            services.AddTransient<EmployeeService>();




            services.AddTransient<ICanGetOffset, FixedGetOffset>();
            var offsetService = services.BuildServiceProvider().GetService<ICanGetOffset>();
            ServiceLocator.RegisterService(offsetService);
        }
    }
}
