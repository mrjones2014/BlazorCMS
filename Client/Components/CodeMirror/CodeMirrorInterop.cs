using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorCMS.Client.Components.CodeMirror
{
    public class CodeMirrorInterop
    {
        [Inject]
        private IJSRuntime JsRuntime { get; set; }


    }
}
