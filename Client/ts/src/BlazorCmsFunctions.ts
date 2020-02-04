import Prism from 'prismjs';

interface IBlazorCmsFunctions {
    focus: (selector: string) => void;
    hasClass: (selector: string, className: string) => void;
    applySyntaxHighlighting: (selector: string) => void;
}

const functions: IBlazorCmsFunctions = {
    focus: function (selector) {
        const el: HTMLElement | null = document.querySelector(selector);
        if (el != null && el.focus != null) {
            el.focus();
        }
    },
    hasClass: function (selector, className) {
        const el = document.querySelector(selector);
        if (el == null || el.classList == null) {
            return false;
        }

        return el.classList.contains(className);
    },
    applySyntaxHighlighting: function (selector) {
        if (selector == null) {
            Prism.highlightAll();
            return;
        }

        const el = document.querySelector(selector);
        if (el == null) {
            return;
        }

        Prism.highlightAllUnder(el);
    }
};

export const BlazorCmsFunctions = {
    init: () => (<any>window).BlazorCmsFunctions = functions
};
