// JavaScript for InvitationLinkCard component
export function renderQrCode(canvasElement, text) {
    if (window.QRCode && canvasElement) {
        window.QRCode.toCanvas(canvasElement, text, { width: 200, margin: 1 });
    }
}

export function printSection() {
    window.print();
}