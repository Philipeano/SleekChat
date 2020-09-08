using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
using SleekChat.Data.SqlServerDataService;


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
                // x.RequireHttpsMetadata = env.IsProduction(); // True for production, otherwise False
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
                // Create and configure OpenAPI specification document with basic information 
                setupAction.SwaggerDoc("SleekChatOpenAPISpecification",
                    new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = "SleekChat API",
                        Version = "1",
                        Description = "This API allows friends and colleagues create groups for messaging and collaboration.",
                        Contact = new Microsoft.OpenApi.Models.OpenApiContact() 
                        {
                            Email = "philipeano@gmail.com",
                            Name = "Philip Newman",
                            Url = new Uri("https://www.twitter.com/philipeano")
                        },
                        License = new Microsoft.OpenApi.Models.OpenApiLicense()
                        {
                            Name = "MIT License",
                            Url = new Uri("https://opensource.org/licenses/MIT")
                        }
                    });

                //Fetch all XML output documents, and include their content in the OpenAPI specification
                var currentAssembly = Assembly.GetExecutingAssembly();
                var linkedAssemblies = currentAssembly.GetReferencedAssemblies();
                var fullAssemblyList = linkedAssemblies.Union(new AssemblyName[] { currentAssembly.GetName() });
                var xmlCommentFiles = fullAssemblyList
                    .Select(a => Path.Combine(AppContext.BaseDirectory, $"{a.Name}.xml"))
                    .Where(f => File.Exists(f))
                    .ToArray();

                foreach (string xmlFile in xmlCommentFiles)
                {
                    setupAction.IncludeXmlComments(xmlFile);
                }
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else 
            {
                app.UseHsts();
            }

            app.UseStatusCodePages(async context => { 
                context.HttpContext.Response.ContentType = "application/json";
                if (context.HttpContext.Response.StatusCode == 401)
                    await context.HttpContext.Response.WriteAsync(new FormatHelper().Render("Unauthorised! You are not signed in."));
            });

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
