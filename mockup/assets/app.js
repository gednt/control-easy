/* App behaviors (ControlEasy Reborn mockup).
   - Theme toggle
   - Mobile menu drawer
   - Dropdown menus (open/close, outside-click, Escape, viewport clamping)
   - Toast service
   - Modal open/close + focus management
   - Tabs
   - Password show/hide
   - Avatar initials
   - Login form (test@test.com / test123, fail@x.com, ratelimit@x.com, multi → tenant picker)
   - Resident table: search debounce, tab status, block filter, sort, pagination
   - Recent activity rendering on app.html
*/

(function () {
  "use strict";

  /* ---------------- Theme toggle ---------------- */
  document.addEventListener("click", (e) => {
    const t = e.target.closest("[data-theme-toggle]");
    if (t) {
      e.preventDefault();
      const next = window.ThemeService.toggle();
      const label = t.querySelector("[data-theme-label]");
      if (label) label.textContent = next.charAt(0).toUpperCase() + next.slice(1);
    }
  });

  /* ---------------- Mobile menu drawer ---------------- */
  document.addEventListener("click", (e) => {
    const t = e.target.closest("[data-mobile-menu]");
    if (t) {
      document.body.classList.toggle("drawer-open");
      return;
    }
    if (document.body.classList.contains("drawer-open")) {
      const drawer = document.querySelector(".sidebar.is-drawer");
      if (drawer && !drawer.contains(e.target) && !e.target.closest("[data-mobile-menu]")) {
        document.body.classList.remove("drawer-open");
      }
    }
  });

  /* ---------------- Dropdown menus ---------------- */
  function closeAllDropdowns(except) {
    document.querySelectorAll(".ce-dropdown-menu.is-open").forEach((m) => {
      if (m !== except) m.classList.remove("is-open");
    });
  }
  document.addEventListener("click", (e) => {
    const trigger = e.target.closest("[data-dropdown-trigger]");
    if (trigger) {
      e.preventDefault();
      e.stopPropagation();
      const id = trigger.getAttribute("data-dropdown-trigger");
      const menu = document.getElementById(id);
      if (menu) {
        const isOpen = menu.classList.contains("is-open");
        closeAllDropdowns(menu);
        if (!isOpen) {
          menu.classList.add("is-open");
          setTimeout(() => {
            const r = menu.getBoundingClientRect();
            if (r.right > window.innerWidth - 8) {
              menu.style.right = "0";
              menu.style.left = "auto";
            }
            if (r.bottom > window.innerHeight - 8) {
              menu.style.top = "auto";
              menu.style.bottom = "calc(100% + 4px)";
            }
          }, 0);
        } else {
          menu.classList.remove("is-open");
        }
      }
      return;
    }
    // Filter dropdowns: clicking an item rewrites the trigger label
    const filterItem = e.target.closest("[data-filter-menu] [data-filter-value]");
    if (filterItem) {
      applyFilterValue(filterItem);
      const menu = filterItem.closest(".ce-dropdown-menu");
      if (menu) menu.classList.remove("is-open");
      e.stopPropagation();
      return;
    }
    const sortItem = e.target.closest("[data-filter-menu='sort'] [data-sort-value]");
    if (sortItem) {
      applySortValue(sortItem);
      const menu = sortItem.closest(".ce-dropdown-menu");
      if (menu) menu.classList.remove("is-open");
      e.stopPropagation();
      return;
    }
    const item = e.target.closest(".ce-dropdown-item");
    if (item) {
      const menu = item.closest(".ce-dropdown-menu");
      if (menu) menu.classList.remove("is-open");
    }
    if (!e.target.closest(".ce-dropdown")) closeAllDropdowns();
  });
  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape") closeAllDropdowns();
  });

  /* ---------------- Toast service ---------------- */
  const Toast = (function () {
    let region = null;
    function ensure() {
      if (region) return region;
      region = document.createElement("div");
      region.className = "ce-toast-region";
      region.setAttribute("role", "region");
      region.setAttribute("aria-live", "polite");
      region.setAttribute("aria-label", "Notifications");
      document.body.appendChild(region);
      return region;
    }
    function show(tone, title, message, opts) {
      opts = opts || {};
      const el = document.createElement("div");
      el.className = `ce-toast tone-${tone}`;
      el.setAttribute("role", tone === "danger" ? "alert" : "status");
      el.innerHTML = `
        <div class="ce-toast-body">
          ${title ? `<div class="ce-toast-title">${title}</div>` : ""}
          <div class="ce-toast-message">${message}</div>
        </div>
        <button class="ce-toast-close" aria-label="Dismiss">${window.icon("x", 16)}</button>
      `;
      ensure().appendChild(el);
      const dismiss = () => {
        el.style.animation = "toast-out 200ms ease-out forwards";
        setTimeout(() => el.remove(), 200);
      };
      el.querySelector(".ce-toast-close").addEventListener("click", dismiss);
      if (opts.duration !== 0) setTimeout(dismiss, opts.duration || 5000);
      return { dismiss };
    }
    return {
      success: (m, o) => show("success", null, m, o),
      info: (m, o) => show("info", null, m, o),
      warning: (m, o) => show("warning", null, m, o),
      danger: (m, o) => show("danger", null, m, o),
      _show: show
    };
  })();
  window.Toast = Toast;

  /* Demo: toast buttons + anchors */
  document.addEventListener("click", (e) => {
    const b = e.target.closest("[data-toast]");
    if (b) {
      // For anchor tags, prevent the empty-hash navigation
      if (b.tagName === "A") e.preventDefault();
      const tone = b.getAttribute("data-toast");
      const msg = b.getAttribute("data-toast-msg") || "Notification";
      Toast[tone](msg);
    }
  });

  /* ---------------- Modal open/close + backdrop click + focus trap ---------------- */
  let lastFocusedBeforeModal = null;
  document.addEventListener("click", (e) => {
    const opener = e.target.closest("[data-modal-open]");
    if (opener) {
      e.preventDefault();
      const id = opener.getAttribute("data-modal-open");
      const modal = document.getElementById(id);
      if (modal) {
        lastFocusedBeforeModal = document.activeElement;
        modal.classList.add("is-open");
        modal.removeAttribute("hidden");
        setTimeout(() => {
          const f = modal.querySelector("button, [href], input, select, textarea, [tabindex]:not([tabindex='-1'])");
          if (f) f.focus();
        }, 30);
      }
      return;
    }
    const closer = e.target.closest("[data-modal-close]");
    if (closer) {
      e.preventDefault();
      const modal = closer.closest(".ce-modal-backdrop");
      if (modal) closeModal(modal);
      return;
    }
    // Click on the backdrop itself (not on .ce-modal) closes
    const backdrop = e.target.closest(".ce-modal-backdrop");
    if (backdrop && e.target === backdrop) {
      closeModal(backdrop);
    }
  });
  function closeModal(modal) {
    modal.classList.remove("is-open");
    modal.setAttribute("hidden", "");
    if (lastFocusedBeforeModal && typeof lastFocusedBeforeModal.focus === "function") {
      lastFocusedBeforeModal.focus();
    }
    lastFocusedBeforeModal = null;
  }
  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape") {
      const open = document.querySelector(".ce-modal-backdrop.is-open");
      if (open) closeModal(open);
    }
  });

  /* ---------------- Tabs (generic) ---------------- */
  document.addEventListener("click", (e) => {
    const tab = e.target.closest("[data-tab-trigger]");
    if (tab) {
      // Handled by the resident table wiring; keep generic click behavior as fallback
      const group = tab.closest(".ce-tabs");
      if (!group) return;
      group.querySelectorAll("[data-tab-trigger]").forEach((t) => {
        t.setAttribute("aria-selected", t === tab ? "true" : "false");
        if (t === tab) t.removeAttribute("tabindex"); else t.setAttribute("tabindex", "-1");
      });
      // Defer to the resident table renderer if applicable
      const status = tab.getAttribute("data-status");
      if (status) {
        if (window.Residents) {
          window.Residents.setStatus(status);
        }
      }
    }
  });

  /* ---------------- Password show/hide ---------------- */
  document.addEventListener("click", (e) => {
    const t = e.target.closest("[data-password-toggle]");
    if (t) {
      const inputId = t.getAttribute("data-password-toggle");
      const input = document.getElementById(inputId);
      if (!input) return;
      const isPassword = input.type === "password";
      input.type = isPassword ? "text" : "password";
      t.setAttribute("aria-label", isPassword ? "Hide password" : "Show password");
      t.innerHTML = window.icon(isPassword ? "eyeOff" : "eye", 16);
    }
  });

  /* ---------------- Remember me ---------------- */
  const REMEMBER_KEY = "ce.email";
  function prefillRemember() {
    const input = document.getElementById("login-email");
    const cb = document.getElementById("login-remember");
    if (!input || !cb) return;
    try {
      const saved = localStorage.getItem(REMEMBER_KEY);
      if (saved) {
        input.value = saved;
        cb.checked = true;
      }
    } catch (e) { /* ignore */ }
  }
  prefillRemember();

  /* ---------------- Login form ---------------- */
  const loginForm = document.getElementById("login-form");
  if (loginForm) {
    // Session-expired detection
    const params = new URLSearchParams(window.location.search);
    if (params.get("reason") === "session-expired") {
      setTimeout(() => Toast.info("Your session has expired. Please sign in again.", { duration: 8000 }), 300);
    }

    let rateLimitTimer = null;

    loginForm.addEventListener("submit", (e) => {
      e.preventDefault();
      const emailEl = document.getElementById("login-email");
      const passwordEl = document.getElementById("login-password");
      const email = emailEl.value.trim();
      const password = passwordEl.value;
      const remember = document.getElementById("login-remember").checked;
      const errorRegion = document.getElementById("login-error");
      const errorText = document.getElementById("login-error-text");
      const submitBtn = document.getElementById("login-submit");
      const submitText = document.getElementById("login-submit-text");
      const submitSpinner = document.getElementById("login-submit-spinner");

      // Reset
      errorRegion.hidden = true;
      passwordEl.setAttribute("aria-invalid", "false");

      // If the button is currently rate-limit disabled, ignore
      if (submitBtn.disabled) return;

      function showError(msg, focusField) {
        errorText.textContent = msg;
        errorRegion.hidden = false;
        if (focusField) focusField.setAttribute("aria-invalid", "true");
      }
      function startLoading() {
        submitBtn.disabled = true;
        submitBtn.setAttribute("aria-busy", "true");
        submitText.textContent = "Signing in…";
        if (submitSpinner) submitSpinner.hidden = false;
      }
      function stopLoading() {
        submitBtn.disabled = false;
        submitBtn.removeAttribute("aria-busy");
        submitText.textContent = "Sign in";
        if (submitSpinner) submitSpinner.hidden = true;
      }

      // Validation
      if (!email || !password) {
        showError("Please enter your email and password.", password);
        emailEl.focus();
        return;
      }
      if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        showError("Please enter a valid email address.", emailEl);
        emailEl.focus();
        return;
      }
      if (password.length < 6) {
        showError("Password must be at least 6 characters.", passwordEl);
        passwordEl.focus();
        return;
      }

      // Persist / clear remembered email
      try {
        if (remember) localStorage.setItem(REMEMBER_KEY, email);
        else localStorage.removeItem(REMEMBER_KEY);
      } catch (err) { /* ignore */ }

      startLoading();

      setTimeout(() => {
        const lower = email.toLowerCase();

        if (lower === "fail@x.com") {
          stopLoading();
          showError("Invalid email or password. Please try again.", passwordEl);
          return;
        }
        if (lower === "ratelimit@x.com") {
          // Disable button for 15 s and keep error visible
          const remaining = 15;
          submitText.textContent = `Try again in ${remaining}s`;
          if (submitSpinner) submitSpinner.hidden = true;
          submitBtn.setAttribute("aria-busy", "false");
          let left = remaining;
          if (rateLimitTimer) clearInterval(rateLimitTimer);
          rateLimitTimer = setInterval(() => {
            left -= 1;
            if (left > 0) {
              submitText.textContent = `Try again in ${left}s`;
            } else {
              clearInterval(rateLimitTimer);
              rateLimitTimer = null;
              stopLoading();
            }
          }, 1000);
          showError("Too many attempts. Try again in 15 seconds.", passwordEl);
          return;
        }
        if (lower === "test@test.com" && password === "test123") {
          window.location.href = "app.html";
          return;
        }
        if (lower.indexOf("multi") !== -1) {
          showTenantPicker([
            { tenantId: "t1", slug: "default", displayName: "Condomínio Vila Madalena", userDisplayName: "Renata Ferreira" },
            { tenantId: "t2", slug: "globex",   displayName: "Edifício Globex Plaza",   userDisplayName: "Renata Ferreira" }
          ]);
          return;
        }
        // Default: navigate (demo)
        window.location.href = "app.html";
      }, 900);
    });
  }

  function showTenantPicker(tenants) {
    const loginCard = document.querySelector(".login-card");
    if (!loginCard) return;
    loginCard.innerHTML = `
      <div class="login-brand">
        <div class="login-brand-mark">CE</div>
        <div class="login-brand-text">ControlEasy</div>
      </div>
      <h1 class="login-title">Choose a tenant</h1>
      <p class="login-subtitle">Select the condominium you want to operate in.</p>
      <div class="tenant-picker-grid">
        ${tenants.map((t) => `
          <button class="tenant-card" data-tenant-id="${t.tenantId}">
            <div class="tenant-card-name">${t.displayName}</div>
            <div class="tenant-card-slug">${t.userDisplayName} · ${t.slug}</div>
          </button>
        `).join("")}
      </div>
      <a href="#" id="tenant-cancel" class="login-forgot">← Use a different account</a>
    `;
    loginCard.querySelectorAll(".tenant-card").forEach((card) => {
      card.addEventListener("click", () => {
        window.location.href = "app.html?tenant=" + card.getAttribute("data-tenant-id");
      });
    });
    document.getElementById("tenant-cancel").addEventListener("click", (e) => {
      e.preventDefault();
      window.location.href = "login.html";
    });
  }

  /* ---------------- Avatar initials ---------------- */
  document.querySelectorAll("[data-avatar-name]").forEach((el) => {
    const name = el.getAttribute("data-avatar-name");
    if (name) {
      const initials = name.split(" ").filter(Boolean).slice(0, 2).map((s) => s[0]).join("");
      const textEl = el.querySelector("[data-avatar-initials]");
      if (textEl) textEl.textContent = initials;
    }
  });

  /* ---------------- Update theme toggle label on load ---------------- */
  function updateThemeLabels() {
    const cur = window.ThemeService.readStored();
    document.querySelectorAll("[data-theme-label]").forEach((l) => {
      l.textContent = cur.charAt(0).toUpperCase() + cur.slice(1);
    });
  }
  updateThemeLabels();
  document.addEventListener("ce:theme-change", updateThemeLabels);

  /* =====================================================================
     Resident table wiring (only active when #resident-tbody is present)
     ===================================================================== */
  // Shared module-scope state used by the resident table + filter/sort handlers
  const residentState = {
    search: "",
    block: "all",
    status: "all",
    sortKey: "nameAsc",
    page: 1,
    pageSize: 7
  };
  function readPageSize() {
    const nav = document.getElementById("resident-pagination");
    if (nav) {
      const v = parseInt(nav.getAttribute("data-size") || "7", 10);
      if (!isNaN(v) && v > 0) return v;
    }
    return 7;
  }
  residentState.pageSize = readPageSize();

  const tbody = document.getElementById("resident-tbody");
  if (tbody && window.MockData) {
    function renderTable() { rerenderResidents(); }

    // Search inputs (topbar + inline)
    let searchDebounce = null;
    document.querySelectorAll("[data-search-target='resident-tbody']").forEach((input) => {
      input.addEventListener("input", (e) => {
        clearTimeout(searchDebounce);
        const value = e.target.value;
        searchDebounce = setTimeout(() => {
          residentState.search = value;
          residentState.page = 1;
          rerenderResidents();
          // Sync other search inputs
          document.querySelectorAll("[data-search-target='resident-tbody']").forEach((other) => {
            if (other !== e.target && other.value !== value) other.value = value;
          });
        }, 120);
      });
    });

    // Pagination click delegation
    const pagination = document.getElementById("resident-pagination");
    if (pagination) {
      pagination.addEventListener("click", (e) => {
        const pageNum = e.target.closest("[data-page]");
        const action = e.target.closest("[data-page-action]");
        if (pageNum) {
          const p = parseInt(pageNum.getAttribute("data-page"), 10);
          if (!isNaN(p) && p !== residentState.page) {
            residentState.page = p;
            rerenderResidents();
          }
        } else if (action) {
          const kind = action.getAttribute("data-page-action");
          const result = window.MockData.filterSortPaginate(window.MockData.residents, residentState);
          const last = result.pageCount;
          if (kind === "first") residentState.page = 1;
          else if (kind === "prev")  residentState.page = Math.max(1, residentState.page - 1);
          else if (kind === "next")  residentState.page = Math.min(last, residentState.page + 1);
          else if (kind === "last")  residentState.page = last;
          rerenderResidents();
        }
      });
    }

    // Clear filters button (inside the empty state)
    const clearBtn = document.getElementById("resident-clear-filters");
    if (clearBtn) {
      clearBtn.addEventListener("click", () => {
        residentState.search = "";
        residentState.block = "all";
        residentState.status = "all";
        residentState.sortKey = "nameAsc";
        residentState.page = 1;
        document.querySelectorAll("[data-search-target='resident-tbody']").forEach((i) => { i.value = ""; });
        const blockLabel = document.getElementById("filter-block-label");
        if (blockLabel) blockLabel.textContent = "Block: All";
        const sortLabel = document.getElementById("filter-sort-label");
        if (sortLabel) sortLabel.textContent = "Sort: Name (A→Z)";
        // Reset tab selection
        document.querySelectorAll("[data-tab-trigger]").forEach((t) => {
          const isAll = t.getAttribute("data-status") === "all";
          t.setAttribute("aria-selected", isAll ? "true" : "false");
          t.setAttribute("tabindex", isAll ? "0" : "-1");
        });
        rerenderResidents();
        Toast.info("Filters cleared.");
      });
    }

    // Public API for the tab handler
    window.Residents = {
      setStatus(s) { residentState.status = s; residentState.page = 1; rerenderResidents(); }
    };

    // Initial render
    rerenderResidents();
  }

  /* =====================================================================
     Filter & sort value handlers (depend on Residents being initialised)
     ===================================================================== */
  function applyFilterValue(item) {
    const value = item.getAttribute("data-filter-value");
    const menu = item.closest("[data-filter-menu]");
    if (!menu || menu.getAttribute("data-filter-menu") !== "block") return;
    residentState.block = value;
    residentState.page = 1;
    const label = document.getElementById("filter-block-label");
    if (label) {
      label.textContent = "Block: " + (value === "all" ? "All" : value);
    }
    rerenderResidents();
  }

  function applySortValue(item) {
    const value = item.getAttribute("data-sort-value");
    residentState.sortKey = value;
    residentState.page = 1;
    const label = document.getElementById("filter-sort-label");
    if (label) {
      const map = {
        nameAsc: "Sort: Name (A→Z)",
        nameDesc: "Sort: Name (Z→A)",
        apartment: "Sort: Apartment",
        lastVisitDesc: "Sort: Last visit (newest)"
      };
      label.textContent = map[value] || label.textContent;
    }
    rerenderResidents();
  }

  // Renders table + pagination + status counts based on the current residentState.
  function rerenderResidents() {
    const tbodyEl = document.getElementById("resident-tbody");
    if (!tbodyEl || !window.MockData) return;
    const state = residentState;
    const result = window.MockData.filterSortPaginate(window.MockData.residents, state);
    const rowsHtml = result.rows.map(window.MockData.renderResidentRow).join("");
    const empty = document.getElementById("resident-empty");
    if (result.total === 0) {
      tbodyEl.innerHTML = "";
      if (empty) { empty.hidden = false; tbodyEl.appendChild(empty); }
    } else {
      tbodyEl.innerHTML = rowsHtml;
      if (empty) empty.hidden = true;
    }
    // Icons + avatar initials on the new rows
    tbodyEl.querySelectorAll("[data-icon]").forEach((el) => {
      el.innerHTML = window.icon(el.getAttribute("data-icon"));
    });
    tbodyEl.querySelectorAll("[data-avatar-name]").forEach((el) => {
      const name = el.getAttribute("data-avatar-name");
      if (!name) return;
      const initials = name.split(" ").filter(Boolean).slice(0, 2).map((s) => s[0]).join("");
      const t = el.querySelector("[data-avatar-initials]");
      if (t) t.textContent = initials;
    });
    // Pagination
    const info = document.getElementById("resident-pagination-info");
    const controls = document.getElementById("resident-pagination-controls");
    if (info) {
      if (result.total === 0) info.textContent = "Showing 0–0 of 0 results";
      else {
        const start = (result.page - 1) * result.pageSize + 1;
        const end = Math.min(result.page * result.pageSize, result.total);
        info.textContent = `Showing ${start}–${end} of ${result.total} results`;
      }
    }
    if (controls) {
      const buttons = [];
      const last = result.pageCount;
      buttons.push({ kind: "first", icon: "chevronsLeft", disabled: result.page <= 1 });
      buttons.push({ kind: "prev",  icon: "chevronLeft",  disabled: result.page <= 1 });
      const pages = [];
      if (last <= 7) { for (let i = 1; i <= last; i++) pages.push(i); }
      else {
        pages.push(1, 2, 3);
        if (result.page > 4) pages.push("…");
        if (result.page > 3 && result.page < last - 2) pages.push(result.page);
        if (result.page < last - 3) pages.push("…");
        pages.push(last);
      }
      pages.forEach((p) => p === "…"
        ? buttons.push({ kind: "ellipsis" })
        : buttons.push({ kind: "page", value: p, active: p === result.page, label: String(p) }));
      buttons.push({ kind: "next",  icon: "chevronRight",  disabled: result.page >= last });
      buttons.push({ kind: "last",  icon: "chevronsRight", disabled: result.page >= last });
      controls.innerHTML = buttons.map((b) => {
        if (b.kind === "ellipsis") return `<span class="ce-pagination-ellipsis" style="padding: 0 var(--space-1); color: var(--color-text-muted);">…</span>`;
        if (b.kind === "page") return `<button class="ce-pagination-btn ce-page-num ${b.active ? "active" : ""}" data-page="${b.value}" aria-current="${b.active ? "page" : "false"}">${b.label}</button>`;
        return `<button class="ce-pagination-btn" data-page-action="${b.kind}" ${b.disabled ? "disabled" : ""} aria-label="${b.kind} page">${b.icon ? `<span data-icon="${b.icon}"></span>` : ""}</button>`;
      }).join("");
      controls.querySelectorAll("[data-icon]").forEach((el) => {
        el.innerHTML = window.icon(el.getAttribute("data-icon"));
      });
    }
    // Status counts (search + block applied, status not)
    const partial = window.MockData.residents.filter((r) => {
      if (state.block !== "all" && r.block !== state.block) return false;
      const s = state.search.trim().toLowerCase();
      if (s) {
        const hay = (r.name + " " + r.cpf + " " + r.apartment + " " + r.phone + " " + r.block).toLowerCase();
        if (hay.indexOf(s) === -1) return false;
      }
      return true;
    });
    const counts = window.MockData.statusCounts(partial);
    document.querySelectorAll("[data-status-count]").forEach((el) => {
      const key = el.getAttribute("data-status-count");
      el.textContent = String(counts[key] || 0);
    });
  }
})();
