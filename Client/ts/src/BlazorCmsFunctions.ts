import './prism.js';
import './prism.css';
import { IBlazorModule } from './IBlazorModule';

interface IBlazorCmsFunctions {
    focus:                   (selector: string) => void;
    hasClass:                (selector: string, className: string) => void;
    applySyntaxHighlighting: (selector: string) => void;
}

const functions: IBlazorCmsFunctions = {
    focus: (selector: string) => {
        const el: HTMLElement | null = document.querySelector(selector);
        if (el != null && el.focus != null) {
            el.focus();
        }
    },
    hasClass: (selector: string, className: string) => {
        const el = document.querySelector(selector);
        if (el == null || el.classList == null) {
            return false;
        }

        return el.classList.contains(className);
    },
    applySyntaxHighlighting: (selector: string) => {
        if (selector == null) {
            // @ts-ignore
            Prism.highlightAll();
            return;
        }

        const el = document.querySelector(selector);
        if (el == null) {
            console.warn("BlazorCms.Utils.applySyntaxHighlighting: Attempted to highlight non-existent element.");
            return;
        }

        console.log("BlazorCms.Utils.applySyntaxHighlighting");
        // @ts-ignore
        Prism.highlightAllUnder(el);
    }
};

export const BlazorCmsFunctions: IBlazorModule = {
    register: () => {
        (<any>window).BlazorCms       = (<any>window).BlazorCms || {};
        (<any>window).BlazorCms.Utils = functions;
    }
};
