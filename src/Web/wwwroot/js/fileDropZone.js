export function initializeFileDropZone(dropZoneElement, inputFile) {

    function onPaste(e) {
        inputFile.files = e.clipboardData.files;
        const event = new Event('change', { bubbles: true });
        inputFile.dispatchEvent(event);
    }

    function onDrop(e) {
        inputFile.files = e.dataTransfer.files;
        const event = new Event('change', { bubbles: true });
        inputFile.dispatchEvent(event);
    }

    dropZoneElement.addEventListener('paste', onPaste);
    dropZoneElement.addEventListener('drop', onDrop);

    return {
        dispose: () => {
            dropZoneElement.removeEventListener('paste', onPaste);
            dropZoneElement.removeEventListener('drop', onDrop);
        }
    }
}