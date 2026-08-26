// JavaScript for NavDrawer component
// Lightweight, dependency-free focus trap for the mobile navigation drawer.
// activate(): remembers the element that had focus before opening (typically the
// hamburger button in PublicHeader), moves focus into the drawer, and cyclically
// traps Tab navigation inside it.
// deactivate(): releases the trap and restores focus to the element that had focus
// before the drawer opened.

let previouslyFocusedElement = null;
let trappedElement = null;
let keydownHandler = null;

const focusableSelector =
    'a[href], button:not([disabled]), textarea:not([disabled]), ' +
    'input:not([disabled]):not([type="hidden"]), select:not([disabled]), ' +
    '[tabindex]:not([tabindex="-1"])';

function getFocusableElements(container) {
    if (!container) {
        return [];
    }

    return Array.from(container.querySelectorAll(focusableSelector))
        .filter(element => element.offsetParent !== null);
}

export function activate(drawerElement) {
    if (!drawerElement) {
        return;
    }

    previouslyFocusedElement = document.activeElement;
    trappedElement = drawerElement;

    const focusable = getFocusableElements(drawerElement);
    if (focusable.length > 0) {
        focusable[0].focus();
    } else {
        drawerElement.focus();
    }

    keydownHandler = event => {
        if (event.key !== 'Tab' || !trappedElement) {
            return;
        }

        const focusableElements = getFocusableElements(trappedElement);
        if (focusableElements.length === 0) {
            event.preventDefault();
            return;
        }

        const first = focusableElements[0];
        const last = focusableElements[focusableElements.length - 1];
        const active = document.activeElement;

        if (event.shiftKey) {
            if (active === first || !trappedElement.contains(active)) {
                event.preventDefault();
                last.focus();
            }
        } else if (active === last || !trappedElement.contains(active)) {
            event.preventDefault();
            first.focus();
        }
    };

    // Capturing phase so Tab is intercepted before Blazor's own listeners.
    // Escape is left untouched and still bubbles to Blazor's @onkeydown handler.
    document.addEventListener('keydown', keydownHandler, true);
}

export function deactivate() {
    if (keydownHandler) {
        document.removeEventListener('keydown', keydownHandler, true);
        keydownHandler = null;
    }

    trappedElement = null;

    if (previouslyFocusedElement && typeof previouslyFocusedElement.focus === 'function') {
        previouslyFocusedElement.focus();
    }

    previouslyFocusedElement = null;
}