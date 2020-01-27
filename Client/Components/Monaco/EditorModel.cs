using System;
using BlazorCMS.Client.Components.Monaco.Options;

namespace BlazorCMS.Client.Components.Monaco
{
    public class EditorModel
    {
        public EditorModel()
        {
        }

        public EditorModel(EditorOptions options)
        {
            Options = options;
        }

        public string        Id      { get; set; } = $"Monaco_{new Random().Next(0, 1000000).ToString()}";
        public EditorOptions Options { get; set; } = new EditorOptions();
    }
}
