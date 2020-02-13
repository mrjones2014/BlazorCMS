import CodeMirror        from 'codemirror';
import { IBlazorModule } from "./IBlazorModule";

interface BlazorComponentInstance {
    invokeMethodAsync: (methodName: string, ...args: any[]) => void;
}

interface EditorTuple {
    id:     string;
    editor: CodeMirror.Editor;
}

interface CodeMirrorInterop {
    editors:          Array<EditorTuple>;
    initializeEditor: (id: string, value: string) => void;
    getEditor:        (id: string) => CodeMirror.Editor;
    getValue:         (id: string) => string;
    setValue:         (id: string, value: string) => void;
    onChange:         (id: string, component: BlazorComponentInstance, methodName: string) => void;
}

const codeMirrorFunctions: CodeMirrorInterop = {
    editors: [],
    initializeEditor: (id: string, value: string) => {
        const editor = CodeMirror((host: HTMLElement) => {
            const root: HTMLElement | null = document.getElementById(id);
            if (root == null) {
                throw "Root element is null!";
            }
            if (root.parentNode == null) {
                throw "Cannot initialize editor on <html> element!";
            }
            root.parentNode.replaceChild(host, root);
        }, {
            value: value,
            mode: 'markdown',
            theme: 'material-darker',
            autofocus: true
        });
        codeMirrorFunctions.editors.push({id, editor});
    },
    getEditor: (id: string) => {
        const editor = codeMirrorFunctions.editors.find((t: EditorTuple) => t.id == id);
        if (editor == null) {
            throw `Could not find editor with ID ${id}`;
        }

        return editor.editor;
    },
    getValue: (id: string) => {
        return codeMirrorFunctions.getEditor(id).getValue();
    },
    setValue: (id: string, value: string) => {
        codeMirrorFunctions.getEditor(id).setValue(value);
    },
    onChange: (id: string, component: BlazorComponentInstance, methodName: string) => {
        const editor = codeMirrorFunctions.getEditor(id);
        const handler = (instance: CodeMirror.Editor) => {
            if (component != null) {
                component.invokeMethodAsync(methodName, instance.getValue());
                return;
            }
            console.warn("BlazorCMS:: JS callback executed after Blazor component unmounted.");
        };
        editor.on("change", handler);
        editor.on("blur", handler);
    }
};

export const CodeMirrorInterop: IBlazorModule = {
    register: () => {
        (<any>window).BlazorCms = (<any>window).BlazorCms || {};
        (<any>window).BlazorCms.CodeMirrorInterop = codeMirrorFunctions;
    }
};
