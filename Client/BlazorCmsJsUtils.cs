using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorCMS.Client
{
    public interface IBlazorCmsJsUtils
    {
        void Focus(string selector);
        Task<bool> HasClass(string selector, string className);
        void ApplySyntaxHighlighting(string selector);
    }

    public class BlazorCmsJsUtils : IBlazorCmsJsUtils
    {
        private class Methods
        {
            public static string Focus                   = methodName("focus");
            public static string HasClass                = methodName("hasClass");
            public static string ApplySyntaxHighlighting = methodName("applySyntaxHighlighting");

            private static string methodName(string methodName) => $"BlazorCms.Utils.{methodName}";
        }

        private IJSRuntime JsRuntime { get; set; }

        public BlazorCmsJsUtils(IJSRuntime jsRuntime)
        {
            JsRuntime = jsRuntime;
        }

        public void Focus(string selector)
        {
            JsRuntime.InvokeVoidAsync(Methods.Focus, selector);
        }

        public async Task<bool> HasClass(string selector, string className)
        {
            return await JsRuntime.InvokeAsync<bool>(Methods.HasClass, selector, className);
        }

        public void ApplySyntaxHighlighting(string selector)
        {
            JsRuntime.InvokeVoidAsync(Methods.ApplySyntaxHighlighting, selector);
        }
    }
}
