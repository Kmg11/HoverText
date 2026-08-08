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

    const OFFSET_X = 24;
    const OFFSET_Y = 18;

    function updatePopup() {
        const show = ctrlDown && pointerInScreen && popupTextContent;
        if (!popup) return;
        popup.classList.toggle("visible", !!show);
        if (show && popupText) popupText.textContent = popupTextContent;
    }

    function positionPopup() {
        if (!popup || !screen) return;
        const rect = screen.getBoundingClientRect();
        let x = pointerX - rect.left + OFFSET_X;
        let y = pointerY - rect.top + OFFSET_Y;
        const w = popup.offsetWidth;
        const h = popup.offsetHeight;
        if (x + w > rect.width - 10) x = pointerX - rect.left - w - OFFSET_X;
        if (x < 10) x = 10;
        if (y + h > rect.height - 10) y = pointerY - rect.top - h - OFFSET_Y;
        if (y < 10) y = 10;
        popup.style.left = x + "px";
        popup.style.top = y + "px";
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
        popupTextContent = text;
        updatePopup();
        if (popup) {
            if (text) positionPopup();
            else popup.style.left = popup.style.top = "";
        }
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
