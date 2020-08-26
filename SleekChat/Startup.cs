using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;
//using SleekChat.Data.InMemoryDataService;
using SleekChat.Data.SqlServerDataService;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SleekChat
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        //public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHttpContextAccessor();

            services
                .AddDbContextPool<SleekChatContext>(options => options
                .UseSqlServer(Configuration.GetConnectionString("SleekChatConnStr")));

            IConfigurationSection appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            AppSettings appSettings = appSettingsSection.Get<AppSettings>();

            byte[] keyInBytes = Encoding.UTF8.GetBytes(appSettings.SecretKey);

            services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                //x.RequireHttpsMetadata = env.IsProduction(); // True for production, otherwise False
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyInBytes),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddScoped<IUserData, SqlUserData>();
            services.AddScoped<IGroupData, SqlGroupData>();
            services.AddScoped<IMembershipData, SqlMembershipData>();
            services.AddScoped<IMessageData, SqlMessageData>();
            services.AddScoped<INotificationData, SqlNotificationData>();
            services.AddScoped<ICurrentUser, CurrentUser>();

            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc("SleekChatOpenAPISpecification",
                    new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = "SleekChat API",
                        Version = "1"
                    });

            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseRouting();

            app.UseSwagger();

            app.UseSwaggerUI(setupAction => 
            {
                setupAction.SwaggerEndpoint(
                    "/swagger/SleekChatOpenAPISpecification/swagger.json",
                    "SleekChat API"
                    );
                setupAction.RoutePrefix = "";
            });

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
