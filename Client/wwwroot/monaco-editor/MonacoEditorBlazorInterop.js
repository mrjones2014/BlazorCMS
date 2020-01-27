window.BlazorCms                = window.BlazorCms || {};
window.BlazorCms.Monaco         = window.BlazorCms.Monaco || {};
window.BlazorCms.Monaco.Editors = window.BlazorCms.Monaco.Editors || [];

var initializeEditors = function () {
    if (window.BlazorCms.Monaco.Editors == null) {
        window.BlazorCms.Monaco.Editors = [];
    }
};

window.BlazorCms.Monaco = {
    /**
     * @return {boolean}
     */
    InitializeEditor: function (model) {
        initializeEditors();
        var thisEditor = monaco.editor.create(document.getElementById(model.id), model.options);
        if (window.BlazorCms.Monaco.Editors.find(function (e) { return e.id === model.id; })) {
            return false;
        } else {
            window.BlazorCms.Monaco.Editors.push({id: model.id, editor: thisEditor});
        }
        return true;
    },
    /**
     * @return {string}
     */
    GetValue: function (id) {
        initializeEditors();
        var myEditor = window.BlazorCms.Monaco.Editors.find(function(e) { return e.id === id; });
        if (!myEditor) {
            throw "Could not find an editor with id";
        }
        return myEditor.editor.getValue();
    },
    /**
     * @return {boolean}
     */
    SetValue: function (id, value) {
        initializeEditors();
        var myEditor = window.BlazorCms.Monaco.Editors.find(function(e) { return e.id === id; });
        if (!myEditor) {
            throw "Could not find an editor with id";
        }
        myEditor.editor.setValue(value);
        return true;
    },
    /**
     * @return {boolean}
     */
    SetTheme: function (id, theme) {
        initializeEditors();
        var myEditor = window.BlazorCms.Monaco.Editors.find(function(e) { return e.id === id; });
        if (!myEditor) {
            throw "Could not find an editor with id";
        }
        monaco.editor.setTheme(theme);
        return true;
    },
    OnContentChange: function (componentInstance, id, csharpMethodName) {
        initializeEditors();
        var myEditor = window.BlazorCms.Monaco.Editors.find(function (e) { return e.id === id; });
        if (!myEditor) {
            throw "Could not find an editor with id";
        }
        myEditor.editor.getModel().onDidChangeContent(function () {
            componentInstance.invokeMethodAsync(csharpMethodName);
        });
    }
};
