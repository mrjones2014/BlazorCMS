using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading.Tasks;
using AndcultureCode.CSharp.Conductors;
using AndcultureCode.CSharp.Core.Interfaces.Conductors;
using AndcultureCode.CSharp.Core.Interfaces.Data;
using BlazorCMS.Server.Data;
using BlazorCMS.Server.Data.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BlazorCMS.Shared.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace BlazorCMS.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.UseEnvFile();

            services.AddMvc();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });

            var dbString = DotNetEnv.Env.GetString(EnvironmentVariables.DATABASE_CONNECTION_STRING);
            if (string.IsNullOrWhiteSpace(dbString))
            {
                throw new ApplicationException("Failed to read db connection string from environment.");
            }

            services.AddDbContext<BlazorCmsContext>(options => options.UseSqlite(dbString));

            services.AddScoped<IRepository<Section>, Repository<Section>>();
            services.AddScoped<IRepository<Article>, Repository<Article>>();

            services.AddScoped<IRepositoryCreateConductor<Section>, RepositoryCreateConductor<Section>>();
            services.AddScoped<IRepositoryReadConductor<Section>,   RepositoryReadConductor<Section>>();
            services.AddScoped<IRepositoryUpdateConductor<Section>, RepositoryUpdateConductor<Section>>();
            services.AddScoped<IRepositoryDeleteConductor<Section>, RepositoryDeleteConductor<Section>>();

            services.AddScoped<IRepositoryCreateConductor<Article>, RepositoryCreateConductor<Article>>();
            services.AddScoped<IRepositoryReadConductor<Article>,   RepositoryReadConductor<Article>>();
            services.AddScoped<IRepositoryUpdateConductor<Article>, RepositoryUpdateConductor<Article>>();
            services.AddScoped<IRepositoryDeleteConductor<Article>, RepositoryDeleteConductor<Article>>();

            var autoMapperConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<Section, SectionDto>();
                config.CreateMap<Article, ArticleDto>();
                config.CreateMap<User,    UserDto>();
            });
            IMapper mapper = autoMapperConfig.CreateMapper();
            services.AddSingleton<IMapper>(mapper);

            services.AddIdentity<User, IdentityRole<long>>()
                .AddEntityFrameworkStores<BlazorCmsContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(
                options =>
                {
                    options.Cookie.Name = "blazorcms_auth_cookie";
                    options.LoginPath   = new PathString("/");
                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
                }
            );

            services.AddAuthentication();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }

            app.UseStaticFiles();
            app.UseClientSideBlazorFiles<Client.Startup>();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapFallbackToClientSideBlazor<Client.Startup>("index.html");
            });

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var dbContext = serviceScope.ServiceProvider.GetService<BlazorCmsContext>())
                {
                    dbContext.Database.Migrate();

                    using (var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole<long>>>())
                    {
                        dbContext.SeedRole(roleManager);
                    }

                    using (var userManager = serviceScope.ServiceProvider.GetService<UserManager<User>>())
                    {
                        dbContext.SeedUser(userManager);
                    }

                    dbContext.SeedHelloWorldSectionAndArticle();
                }
            }
        }
    }
}
