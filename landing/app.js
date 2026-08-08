(() => {
    "use strict";

    const yearEl = document.getElementById("year");
    if (yearEl) yearEl.textContent = String(new Date().getFullYear());

    const githubBtn = document.getElementById("download-github");
    const noteEl = document.getElementById("download-note");

    const popup = document.querySelector(".hover-popup");
    const popupText = popup ? popup.querySelector(".hover-popup-text") : null;
    const screen = document.querySelector(".screen");
    let ctrlDown = false;
    let popupTextContent = "";
    let pointerX = 0;
    let pointerY = 0;
    let pointerInScreen = false;

    const GAP_Y = 24;
    const GAP_ABOVE_Y = 24;

    function updatePopup() {
        if (!popup) return;
        const show = ctrlDown && pointerInScreen && popupTextContent;
        popup.classList.toggle("visible", !!show);
    }

    function setPopupText(text) {
        if (popupText) popupText.textContent = text;
    }

    // Mirrors OverlayWindow.xaml.cs: the overlay centers horizontally under
    // the cursor, sits GAP_Y below it, flips above with GAP_ABOVE_Y when it
    // would run off the bottom, and is clamped to the screen edges.
    function positionPopup() {
        if (!popup || !screen) return;
        const rect = screen.getBoundingClientRect();
        const w = popup.offsetWidth;
        const h = popup.offsetHeight;
        const cx = pointerX - rect.left;
        const cy = pointerY - rect.top;

        let left = cx - w / 2;
        if (left < 0) left = 0;
        else if (left + w > rect.width) left = rect.width - w;

        let top = cy + GAP_Y;
        if (top + h > rect.height) top = cy - GAP_ABOVE_Y - h;
        if (top < 0) top = 0;

        popup.style.left = left + "px";
        popup.style.top = top + "px";
    }

    function updateFromPointer() {
        if (!screen) return;
        const rect = screen.getBoundingClientRect();
        pointerInScreen =
            pointerX >= rect.left && pointerX <= rect.right &&
            pointerY >= rect.top && pointerY <= rect.bottom;

        let text = "";
        if (pointerInScreen) {
            const el = document.elementFromPoint(pointerX, pointerY);
            const hit = el && el.closest("[data-hover-text]");
            text = hit ? hit.getAttribute("data-hover-text") : "";
        }

        const textChanged = text !== popupTextContent;
        popupTextContent = text;

        // The real overlay anchors in place and only re-positions when the
        // text under the cursor actually changes (OverlayWindow.xaml.cs).
        if (textChanged) {
            if (text) {
                setPopupText(text);
                positionPopup();
            } else {
                popup.style.left = popup.style.top = "";
            }
        }

        updatePopup();
    }

    window.addEventListener("mousemove", (e) => {
        pointerX = e.clientX;
        pointerY = e.clientY;
        updateFromPointer();
    });

    window.addEventListener("keydown", (e) => {
        if (e.key === "Control" && !ctrlDown) {
            ctrlDown = true;
            updatePopup();
        }
    });

    window.addEventListener("keyup", (e) => {
        if (e.key === "Control" && ctrlDown) {
            ctrlDown = false;
            updatePopup();
        }
    });

    window.addEventListener("blur", () => {
        if (ctrlDown) {
            ctrlDown = false;
            updatePopup();
        }
    });

    const RELEASE_PAGE = "https://github.com/Kmg11/HoverText/releases/latest";
    const RELEASES_API = "https://api.github.com/repos/Kmg11/HoverText/releases/latest";

    function setNote(text) {
        if (noteEl) noteEl.textContent = text;
    }

    function pickInstaller(assets) {
        return assets
            .filter((a) => /\.exe$/i.test(a.name) && /setup/i.test(a.name))
            .sort((a, b) => a.name.localeCompare(b.name))[0];
    }

    // Resolve the latest release's installer URL via the GitHub API so the
    // button always points at the current version. Falls back to the release
    // page if the fetch fails (offline, rate-limited, or non-API environments).
    fetch(RELEASES_API)
        .then((res) => {
            if (!res.ok) throw new Error("bad status " + res.status);
            return res.json();
        })
        .then((release) => {
            const asset = pickInstaller(release.assets || []);
            if (!asset) throw new Error("no installer asset");

            githubBtn.href = asset.browser_download_url;
            setNote("Latest: " + release.tag_name + " · " + asset.name);
        })
        .catch(() => {
            githubBtn.href = RELEASE_PAGE;
            setNote("Open the releases page to download the latest version.");
        });
})();
