(() => {
    "use strict";

    const yearEl = document.getElementById("year");
    if (yearEl) yearEl.textContent = String(new Date().getFullYear());

    const githubBtn = document.getElementById("download-github");
    const noteEl = document.getElementById("download-note");

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
