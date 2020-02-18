using System.Reflection;
using BlazorCMS.Client.Components.CodeMirror;
using BlazorCMS.Client.State;
using Blazored.Toast;
using BlazorState;
using MediatR;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorCMS.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMediatR(typeof(Startup));
            services.AddBlazorState((aOptions) => aOptions.Assemblies =
                new Assembly[]
                {
                    typeof(Startup).GetTypeInfo().Assembly,
                });
            services.AddTransient<ClientState>();
            services.AddBlazorContextMenu();
            services.AddBlazoredToast();
            services.AddScoped<ICodeMirrorInterop, CodeMirrorInterop>();
            services.AddScoped<IBlazorCmsJsUtils,  BlazorCmsJsUtils>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
