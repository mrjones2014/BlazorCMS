using Microsoft.JSInterop;

namespace BlazorCMS.Client.Components.CodeMirror
{
    public interface ICodeMirrorInterop
    {
        void InitializeEditor(string id, string value, bool autofocus);
        void OnChange<T>(string id, DotNetObjectReference<T> componentRef, string methodName) where T : class;
    }

    public class CodeMirrorInterop : ICodeMirrorInterop
    {
        private class Methods
        {
            public static string InitializeEditor = methodName("initializeEditor");
            public static string GetEditor        = methodName("getEditor");
            public static string GetValue         = methodName("getValue");
            public static string SetValue         = methodName("setValue");
            public static string OnChange         = methodName("onChange");

            private static string methodName(string methodName) => $"{Namespace}.{methodName}";
        }

        private const string Namespace = "BlazorCms.CodeMirrorInterop";
        private IJSRuntime JsRuntime { get; set; }

        public CodeMirrorInterop(IJSRuntime runtime)
        {
            JsRuntime = runtime;
        }

        public void InitializeEditor(string id, string value, bool autofocus) =>
            JsRuntime.InvokeVoidAsync(Methods.InitializeEditor, id, value, autofocus);

        public void OnChange<T>(string id, DotNetObjectReference<T> componentRef, string methodName) where T : class =>
            JsRuntime.InvokeVoidAsync(
                Methods.OnChange,
                id,
                componentRef,
                methodName
            );
    }
}
