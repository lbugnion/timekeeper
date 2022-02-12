window.onunload = () => {

    if (noSleep) {
        noSleep.disable();
    }
}

window.branding = {

    setTitle: (title) => { document.title = title; },
}

window.host = {

    focusAndSelect: (elementId) => {
        const element = document.getElementById(elementId);
        host.focusAndSelectElement(element);
    },

    focusAndSelectElement: (element) => {
        if (element != null) {
            element.focus();

            if (element.type == "textarea") {
                element.select();
            }
        }
    },

    observeFocusAndSelect: (elementId) => {
        var observer = new MutationObserver(function (mutations) {
            var element = document.getElementById(elementId);

            if (element) {
                host.focusAndSelectElement(element);
                observer.disconnect();
            }
        });

        observer.observe(document, { attributes: false, childList: true, characterData: false, subtree: true });
    }
}

var noSleep;

window.nosleep = {

    isMobile: () => {
        if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
            // true for mobile device
            noSleep = new NoSleep();
            return true;
        } else {
            // false for not mobile device
            noSleep = null;
            return false;
        }
    },

    enableDisableNoSleep: (enable) => {

        if (enable) {
            noSleep = new NoSleep();
            noSleep.enable();
        }
        else {
            noSleep.disable();
            noSleep = null;
        }
    }
}
