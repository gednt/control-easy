/* ThemeService mockup implementation
   Mirrors .specs/2 - visual-design-system/design.md "Theme service flow"
   - 3 modes: light, dark, system
   - localStorage["ce.theme"]
   - SSR-safe (guards)
   - matchMedia listener for system mode
   - <html data-theme="..."> attribute
   - Inline FOUC-prevention script in each HTML page
*/

(function () {
  "use strict";
  const STORAGE_KEY = "ce.theme";
  const ATTR = "data-theme";

  function isBrowser() { return typeof document !== "undefined"; }

  function getSystemTheme() {
    if (!isBrowser() || !window.matchMedia) return "light";
    return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
  }

  function resolveTheme(theme) {
    if (theme === "system") return getSystemTheme();
    return theme === "dark" ? "dark" : "light";
  }

  function readStored() {
    if (!isBrowser()) return "system";
    try { return localStorage.getItem(STORAGE_KEY) || "system"; }
    catch (e) { return "system"; }
  }

  function applyTheme(resolved) {
    if (!isBrowser()) return;
    document.documentElement.setAttribute(ATTR, resolved);
  }

  function setTheme(theme) {
    if (!isBrowser()) return;
    try { localStorage.setItem(STORAGE_KEY, theme); } catch (e) { /* ignore */ }
    applyTheme(resolveTheme(theme));
    document.dispatchEvent(new CustomEvent("ce:theme-change", { detail: { theme, resolved: resolveTheme(theme) } }));
  }

  function toggle() {
    const current = readStored();
    const order = ["light", "dark", "system"];
    const idx = order.indexOf(current);
    const next = order[(idx + 1) % order.length];
    setTheme(next);
    return next;
  }

  // Listen to system changes
  if (isBrowser() && window.matchMedia) {
    const mq = window.matchMedia("(prefers-color-scheme: dark)");
    const handler = () => {
      if (readStored() === "system") applyTheme(getSystemTheme());
    };
    if (mq.addEventListener) mq.addEventListener("change", handler);
    else if (mq.addListener) mq.addListener(handler);
  }

  // Initial application
  if (isBrowser()) {
    applyTheme(resolveTheme(readStored()));
  }

  // Public API
  window.ThemeService = { setTheme, toggle, readStored, resolveTheme };
})();
