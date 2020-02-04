import * as monaco from 'monaco-editor';
import { IBlazorModule }  from './IBlazorModule';

interface BlazorComponentInstance {
    invokeMethodAsync: (methodName: string) => void;
}

interface EditorTuple {
    id: string;
    editor: monaco.editor.IStandaloneCodeEditor;
}

interface IMonacoInterop {
    Editors: Array<EditorTuple>;
    InitializeEditor: (model: any) => boolean;
    GetValue: (id: string) => string;
    SetValue: (id: string, value: string) => void;
    OnContentChange: (componentInstance: BlazorComponentInstance, id: string, methodName: string) => void;
}

const functions: IMonacoInterop = {
    Editors: [],
    InitializeEditor: (model: any) => {
        const el: HTMLElement | null = document.getElementById(model.id);
        if (el == null) {
            return false;
        }
        const thisEditor: monaco.editor.IStandaloneCodeEditor = monaco.editor.create(el, model.options);
        if (functions.Editors.find((e: EditorTuple) => e.id === model.id)) {
            return false;
        } else {
            functions.Editors.push({id: model.id.toString(), editor: thisEditor});
        }
        return true;
    },
    GetValue: (id: string) => {
        const myEditor = functions.Editors.find((e: EditorTuple) => e.id === id);
        if (!myEditor) {
            throw "Could not find an editor with id";
        }
        return myEditor.editor.getValue();
    },
    SetValue: (id: string, value: string) => {
        const myEditor = functions.Editors.find((e: EditorTuple) => e.id === id);
        if (!myEditor) {
            throw "Could not find an editor with id";
        }
        myEditor.editor.setValue(value);
        return true;
    },
    OnContentChange: (componentInstance: BlazorComponentInstance, id: string, methodName: string) => {
        const myEditor = functions.Editors.find((e: EditorTuple) => e.id === id);
        if (myEditor == null || myEditor.editor == null) {
            throw "Could not find an editor with id";
        }
        myEditor.editor.getModel()!.onDidChangeContent(() => componentInstance.invokeMethodAsync(methodName));
        myEditor.editor.onDidBlurEditorWidget(() => componentInstance.invokeMethodAsync(methodName));
    }
};

export const MonacoInterop: IBlazorModule = {
    register: () => {
        (<any>window).BlazorCms        = (<any>window).BlazorCms || {};
        (<any>window).BlazorCms.Monaco = (<any>window).BlazorCms.Monaco = functions;
    }
};
