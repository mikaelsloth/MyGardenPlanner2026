// Delt, stack-sikkert fokus-trap (NavDrawer, dialoger).
// activate(container): husker elementet med fokus, fokuserer [data-autofocus] eller det første
//   fokuserbare element og fanger Tab i containeren. Returnerer et id (> 0).
// deactivate(id): fjerner trap'et. Fokus returneres til det element, der havde fokus før aktivering,
//   men kun hvis trap'et var det øverste (en trap længere nede i stakken flytter ikke fokus).
// Kun det øverste trap i stakken fanger Tab, så en dialog kan åbnes oven på en anden.

const traps = [];
let nextId = 1;

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

function onKeydown(event) {
    if (event.key !== 'Tab') {
        return;
    }

    // Sikkerhedsnet: et trap, hvis container er fjernet fra DOM uden deactivate, må aldrig låse Tab.
    while (traps.length > 0 && !traps[traps.length - 1].element.isConnected) {
        traps.pop();
    }

    if (traps.length === 0) {
        document.removeEventListener('keydown', onKeydown, true);
        return;
    }

    const container = traps[traps.length - 1].element;
    const focusableElements = getFocusableElements(container);
    if (focusableElements.length === 0) {
        event.preventDefault();
        return;
    }

    const first = focusableElements[0];
    const last = focusableElements[focusableElements.length - 1];
    const active = document.activeElement;

    if (event.shiftKey) {
        if (active === first || !container.contains(active)) {
            event.preventDefault();
            last.focus();
        }
    } else if (active === last || !container.contains(active)) {
        event.preventDefault();
        first.focus();
    }
}

export function activate(container) {
    if (!container) {
        return 0;
    }

    const trap = {
        id: nextId++,
        element: container,
        previouslyFocused: document.activeElement,
    };

    traps.push(trap);

    // Capturing phase, så Tab opsnappes før Blazors egne lyttere.
    // Escape røres ikke og bobler videre til Blazors @onkeydown.
    if (traps.length === 1) {
        document.addEventListener('keydown', onKeydown, true);
    }

    const target = container.querySelector('[data-autofocus]') ?? getFocusableElements(container)[0];
    if (target) {
        target.focus();
    } else {
        container.focus();
    }

    return trap.id;
}

export function deactivate(id) {
    const index = traps.findIndex(trap => trap.id === id);
    if (index === -1) {
        return;
    }

    const wasTop = index === traps.length - 1;
    const [trap] = traps.splice(index, 1);

    if (traps.length === 0) {
        document.removeEventListener('keydown', onKeydown, true);
    }

    if (wasTop && trap.previouslyFocused && typeof trap.previouslyFocused.focus === 'function') {
        trap.previouslyFocused.focus();
    }
}