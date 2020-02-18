import CodeMirror        from 'codemirror';
import { IBlazorModule } from "./IBlazorModule";
import "codemirror/lib/codemirror.css";
import "codemirror/theme/darcula.css";
import "codemirror/mode/xml/xml";
import "codemirror/mode/markdown/markdown";
import "codemirror/mode/gfm/gfm";

interface BlazorComponentInstance {
    invokeMethodAsync: (methodName: string, ...args: any[]) => void;
}

interface EditorTuple {
    id:     string;
    editor: CodeMirror.Editor;
}

interface CodeMirrorInterop {
    editors:          Array<EditorTuple>;
    initializeEditor: (id: string, value: string, autofocus: boolean) => void;
    getEditor:        (id: string) => CodeMirror.Editor;
    getValue:         (id: string) => string;
    setValue:         (id: string, value: string) => void;
    onChange:         (id: string, component: BlazorComponentInstance, methodName: string) => void;
}

const codeMirrorFunctions: CodeMirrorInterop = {
    editors: [],
    initializeEditor: (id: string, value: string, autofocus: boolean): void => {
        console.log(`CODEMIRRORINTEROP: initializing editor with ID: ${id}`);
        const root: HTMLElement | null = document.getElementById(id);
        if (root == null) {
            throw "Root element is null!";
        }
        const editor = CodeMirror(root, {
            value:     value,
            mode:      'gfm', // Github-flavored markdown
            theme:     'darcula',
            autofocus: autofocus
        });
        codeMirrorFunctions.editors.push({id, editor});
    },
    getEditor: (id: string): CodeMirror.Editor => {
        const editor = codeMirrorFunctions.editors.find((t: EditorTuple) => t.id == id);
        if (editor == null) {
            throw `Could not find editor with ID ${id}`;
        }

        return editor.editor;
    },
    getValue: (id: string): string => {
        return codeMirrorFunctions.getEditor(id).getValue();
    },
    setValue: (id: string, value: string): void => {
        codeMirrorFunctions.getEditor(id).setValue(value);
    },
    onChange: (id: string, component: BlazorComponentInstance, methodName: string): void => {
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
