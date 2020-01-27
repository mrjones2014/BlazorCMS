using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorCMS.Client.Components.Monaco
{
    public class MonacoJsInterop
    {
        public static async Task<bool> InitializeEditor(IJSRuntime runtime, EditorModel editorModel) =>
            await runtime.InvokeAsync<bool>("BlazorCms.Monaco.InitializeEditor", editorModel);

        public static async Task<string> GetValue(IJSRuntime runtime, string id) =>
            await runtime.InvokeAsync<string>("BlazorCms.Monaco.GetValue", id);

        public static async Task<bool> SetValue(IJSRuntime runtime, string id, string value) =>
            await runtime.InvokeAsync<bool>("BlazorCms.Monaco.SetValue", id, value);

        public static async Task<bool> SetTheme(IJSRuntime runtime, string id, string theme) =>
            await runtime.InvokeAsync<bool>("BlazorCms.Monaco.SetTheme", id, theme);

        public static async Task OnContentChange<T>(IJSRuntime runtime, DotNetObjectReference<T> editorInstance, string id, string methodName) where T : class =>
            await runtime.InvokeVoidAsync("BlazorCms.Monaco.OnContentChange", editorInstance, id, methodName);
    }
}
