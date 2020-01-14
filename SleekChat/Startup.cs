using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SleekChat.Data.Contracts;
using SleekChat.Data.InMemoryDataService;

namespace SleekChat
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddControllers();
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    //options.SuppressConsumesConstraintForFormFileParameters = true;
                    //options.SuppressInferBindingSourcesForParameters = true;
                    options.SuppressModelStateInvalidFilter = true;
                    //options.SuppressMapClientErrors = true;
                    //options.ClientErrorMapping[404].Link =
                    //    "https://httpstatuses.com/404";
                });

            services.AddSingleton<IUserData, UserData>();
            services.AddSingleton<IGroupData, GroupData>();
            services.AddSingleton<IMembershipData, MembershipData>();
            services.AddSingleton<IMessageData, MessageData>();
            services.AddSingleton<INotificationData, NotificationData>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
