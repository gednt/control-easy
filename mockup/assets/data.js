/* Mock data + pure helper for the design-system mockup.
   Powers: search debounce, tab status filter, block filter, sort, pagination, recent activity.

   Exposes window.MockData with:
     - residents: array of 30 records
     - statusLabel(status): "Active" / "Pending" / "Overdue" / "Inactive"
     - statusTone(status): tone-* class for ce-badge
     - filterSortPaginate(residents, opts) -> { rows, total, page, pageSize, pageCount }
     - renderResidentRow(r): HTML string for a <tr>
     - escapeHtml(s): tiny HTML escape
     - recentActivity: array of 3 events for the demo feed
*/

(function () {
  "use strict";

  function escapeHtml(s) {
    return String(s == null ? "" : s)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function statusLabel(s) {
    return { active: "Active", pending: "Pending", overdue: "Overdue", inactive: "Inactive" }[s] || s;
  }
  function statusTone(s) {
    return { active: "success", pending: "warning", overdue: "danger", inactive: "neutral" }[s] || "neutral";
  }

  // 30 residents — varied blocks, statuses, visit times.
  const residents = [
    { id: 1,  name: "Maria Silva",      cpf: "123.456.789-00", block: "A", apartment: "102", phone: "(11) 98765-4321", status: "active",   lastVisitAt: "2026-06-12T20:00:00Z" },
    { id: 2,  name: "João Pereira",     cpf: "234.567.890-11", block: "B", apartment: "205", phone: "(11) 99876-5432", status: "active",   lastVisitAt: "2026-06-11T18:30:00Z" },
    { id: 3,  name: "Ana Costa",        cpf: "345.678.901-22", block: "C", apartment: "312", phone: "(11) 97654-3210", status: "pending",  lastVisitAt: "2026-06-09T10:15:00Z" },
    { id: 4,  name: "Carlos Mendes",    cpf: "456.789.012-33", block: "D", apartment: "408", phone: "(11) 96543-2109", status: "overdue",  lastVisitAt: "2026-05-15T09:00:00Z" },
    { id: 5,  name: "Beatriz Lima",     cpf: "567.890.123-44", block: "A", apartment: "501", phone: "(11) 95432-1098", status: "inactive", lastVisitAt: "2026-04-20T14:00:00Z" },
    { id: 6,  name: "Ricardo Alves",    cpf: "678.901.234-55", block: "B", apartment: "602", phone: "(11) 94321-0987", status: "active",   lastVisitAt: "2026-06-12T08:14:00Z" },
    { id: 7,  name: "Patrícia Souza",   cpf: "789.012.345-66", block: "C", apartment: "704", phone: "(11) 93210-9876", status: "active",   lastVisitAt: "2026-06-11T18:42:00Z" },
    { id: 8,  name: "Felipe Rocha",     cpf: "890.123.456-77", block: "A", apartment: "803", phone: "(11) 92109-8765", status: "active",   lastVisitAt: "2026-06-10T11:20:00Z" },
    { id: 9,  name: "Camila Ribeiro",   cpf: "901.234.567-88", block: "B", apartment: "901", phone: "(11) 91098-7654", status: "pending",  lastVisitAt: "2026-06-08T16:00:00Z" },
    { id: 10, name: "Bruno Carvalho",   cpf: "012.345.678-99", block: "C", apartment: "110", phone: "(11) 90987-6543", status: "active",   lastVisitAt: "2026-06-12T07:45:00Z" },
    { id: 11, name: "Larissa Martins",  cpf: "111.222.333-44", block: "D", apartment: "212", phone: "(11) 90876-5432", status: "overdue",  lastVisitAt: "2026-05-22T13:30:00Z" },
    { id: 12, name: "Gabriel Santos",   cpf: "222.333.444-55", block: "A", apartment: "315", phone: "(11) 90765-4321", status: "active",   lastVisitAt: "2026-06-11T20:10:00Z" },
    { id: 13, name: "Fernanda Costa",   cpf: "333.444.555-66", block: "B", apartment: "418", phone: "(11) 90654-3210", status: "active",   lastVisitAt: "2026-06-10T09:50:00Z" },
    { id: 14, name: "Rodrigo Lima",     cpf: "444.555.666-77", block: "C", apartment: "521", phone: "(11) 90543-2109", status: "inactive", lastVisitAt: "2026-03-30T17:20:00Z" },
    { id: 15, name: "Juliana Ferreira", cpf: "555.666.777-88", block: "D", apartment: "624", phone: "(11) 90432-1098", status: "active",   lastVisitAt: "2026-06-12T11:00:00Z" },
    { id: 16, name: "Marcos Oliveira",  cpf: "666.777.888-99", block: "A", apartment: "727", phone: "(11) 90321-0987", status: "pending",  lastVisitAt: "2026-06-07T15:45:00Z" },
    { id: 17, name: "Vanessa Almeida",  cpf: "777.888.999-00", block: "B", apartment: "830", phone: "(11) 90210-9876", status: "active",   lastVisitAt: "2026-06-11T10:25:00Z" },
    { id: 18, name: "Eduardo Pereira",  cpf: "888.999.000-11", block: "C", apartment: "933", phone: "(11) 90109-8765", status: "active",   lastVisitAt: "2026-06-09T19:00:00Z" },
    { id: 19, name: "Aline Rodrigues",  cpf: "999.000.111-22", block: "D", apartment: "140", phone: "(11) 90098-7654", status: "overdue",  lastVisitAt: "2026-05-18T12:00:00Z" },
    { id: 20, name: "Tiago Nascimento", cpf: "000.111.222-33", block: "A", apartment: "243", phone: "(11) 89987-6543", status: "active",   lastVisitAt: "2026-06-12T13:30:00Z" },
    { id: 21, name: "Renata Ferreira",  cpf: "111.222.333-00", block: "B", apartment: "346", phone: "(11) 88876-5432", status: "active",   lastVisitAt: "2026-06-11T22:00:00Z" },
    { id: 22, name: "Pedro Almeida",    cpf: "222.333.444-00", block: "C", apartment: "449", phone: "(11) 88765-4321", status: "pending",  lastVisitAt: "2026-06-06T08:30:00Z" },
    { id: 23, name: "Sofia Castro",     cpf: "333.444.555-00", block: "D", apartment: "552", phone: "(11) 88654-3210", status: "active",   lastVisitAt: "2026-06-10T14:10:00Z" },
    { id: 24, name: "Lucas Barbosa",    cpf: "444.555.666-00", block: "A", apartment: "655", phone: "(11) 88543-2109", status: "inactive", lastVisitAt: "2026-04-05T11:00:00Z" },
    { id: 25, name: "Helena Gomes",     cpf: "555.666.777-00", block: "B", apartment: "758", phone: "(11) 88432-1098", status: "active",   lastVisitAt: "2026-06-12T06:00:00Z" },
    { id: 26, name: "André Lopes",      cpf: "666.777.888-00", block: "C", apartment: "861", phone: "(11) 88321-0987", status: "active",   lastVisitAt: "2026-06-11T07:20:00Z" },
    { id: 27, name: "Bianca Dias",      cpf: "777.888.999-11", block: "D", apartment: "964", phone: "(11) 88210-9876", status: "overdue",  lastVisitAt: "2026-05-28T16:40:00Z" },
    { id: 28, name: "Rafael Teixeira",  cpf: "888.999.000-22", block: "A", apartment: "171", phone: "(11) 88109-8765", status: "active",   lastVisitAt: "2026-06-12T12:15:00Z" },
    { id: 29, name: "Carla Moreira",    cpf: "999.000.111-33", block: "B", apartment: "274", phone: "(11) 88098-7654", status: "pending",  lastVisitAt: "2026-06-05T13:55:00Z" },
    { id: 30, name: "Diego Cardoso",    cpf: "000.111.222-44", block: "C", apartment: "377", phone: "(11) 87987-6543", status: "active",   lastVisitAt: "2026-06-12T10:00:00Z" }
  ];

  // 3 recent activity events (static)
  const recentActivity = [
    { id: "a1", actor: "Maria Silva",  action: "pre-registered a visit for <strong>João Silva</strong> (her father) on Sunday at 14:00.", when: "2 hours ago", badge: { tone: "info", text: "Pre-register" }, block: "A", apt: "102" },
    { id: "a2", actor: "DHL Express",  action: "delivered a package to <strong>Ana Costa</strong> at Block C, apt 312-C.",                  when: "3 hours ago", badge: { tone: "success", text: "Check-out"   }, block: "C", apt: "312" },
    { id: "a3", actor: "João Pereira", action: "was added as a new resident of <strong>Apartment 205-B</strong>.",                            when: "Yesterday",   badge: { tone: "primary", text: "New"         }, block: "B", apt: "205" }
  ];

  // --- Helpers --------------------------------------------------------------

  function relativeTime(iso, nowMs) {
    nowMs = nowMs || Date.now();
    const t = new Date(iso).getTime();
    const diffSec = Math.round((nowMs - t) / 1000);
    if (diffSec < 60) return "Just now";
    const m = Math.round(diffSec / 60);
    if (m < 60) return m + " minute" + (m === 1 ? "" : "s") + " ago";
    const h = Math.round(m / 60);
    if (h < 24) return h + " hour" + (h === 1 ? "" : "s") + " ago";
    const d = Math.round(h / 24);
    if (d === 1) return "Yesterday";
    if (d < 30) return d + " days ago";
    const mo = Math.round(d / 30);
    if (mo === 1) return "1 month ago";
    if (mo < 12) return mo + " months ago";
    const y = Math.round(mo / 12);
    return y + " year" + (y === 1 ? "" : "s") + " ago";
  }

  function toTime(iso) {
    return new Date(iso).getTime();
  }

  function filterSortPaginate(list, opts) {
    opts = opts || {};
    const search = (opts.search || "").trim().toLowerCase();
    const block = opts.block || "all";
    const status = opts.status || "all";
    const sortKey = opts.sortKey || "nameAsc";
    const page = Math.max(1, opts.page || 1);
    const pageSize = Math.max(1, opts.pageSize || 7);

    let out = list.slice();

    if (block !== "all") out = out.filter((r) => r.block === block);
    if (status !== "all") out = out.filter((r) => r.status === status);
    if (search) {
      out = out.filter((r) => {
        return (
          r.name.toLowerCase().indexOf(search) !== -1 ||
          r.cpf.toLowerCase().indexOf(search) !== -1 ||
          r.apartment.toLowerCase().indexOf(search) !== -1 ||
          r.phone.toLowerCase().indexOf(search) !== -1 ||
          r.block.toLowerCase().indexOf(search) !== -1
        );
      });
    }

    if (sortKey === "nameAsc") out.sort((a, b) => a.name.localeCompare(b.name));
    else if (sortKey === "nameDesc") out.sort((a, b) => b.name.localeCompare(a.name));
    else if (sortKey === "apartment") out.sort((a, b) => (a.block + a.apartment).localeCompare(b.block + b.apartment));
    else if (sortKey === "lastVisitDesc") out.sort((a, b) => toTime(b.lastVisitAt) - toTime(a.lastVisitAt));

    const total = out.length;
    const pageCount = Math.max(1, Math.ceil(total / pageSize));
    const safePage = Math.min(page, pageCount);
    const start = (safePage - 1) * pageSize;
    const rows = out.slice(start, start + pageSize);

    return { rows, total, page: safePage, pageSize, pageCount };
  }

  function statusCounts(list) {
    const counts = { all: list.length, active: 0, pending: 0, overdue: 0, inactive: 0 };
    list.forEach((r) => { counts[r.status] = (counts[r.status] || 0) + 1; });
    return counts;
  }

  function renderResidentRow(r) {
    const initials = r.name.split(" ").filter(Boolean).slice(0, 2).map((s) => s[0]).join("");
    const tone = statusTone(r.status);
    const label = statusLabel(r.status);
    const when = relativeTime(r.lastVisitAt);
    return (
      '<tr data-resident-id="' + r.id + '">' +
        '<td>' +
          '<div class="row row-3">' +
            '<div class="ce-avatar size-sm" data-avatar-name="' + escapeHtml(r.name) + '" style="background: linear-gradient(135deg, #4f46e5, #818cf8);"><span data-avatar-initials>' + escapeHtml(initials) + '</span></div>' +
            '<div>' +
              '<div class="font-semibold">' + escapeHtml(r.name) + '</div>' +
              '<div class="text-xs text-secondary">' + escapeHtml(r.cpf) + '</div>' +
            '</div>' +
          '</div>' +
        '</td>' +
        '<td>' + escapeHtml(r.apartment + '-' + r.block) + '</td>' +
        '<td>Block ' + escapeHtml(r.block) + '</td>' +
        '<td>' + escapeHtml(r.phone) + '</td>' +
        '<td><span class="ce-badge tone-' + tone + ' size-sm">' + label + '</span></td>' +
        '<td>' + escapeHtml(when) + '</td>' +
        '<td class="resident-row-actions">' +
          '<div class="ce-dropdown">' +
            '<button class="icon-btn" data-dropdown-trigger="row-' + r.id + '" aria-label="Actions">⋯</button>' +
            '<div class="ce-dropdown-menu" id="row-' + r.id + '" role="menu">' +
              '<button class="ce-dropdown-item" role="menuitem" data-row-action="view" data-resident-id="' + r.id + '">View</button>' +
              '<button class="ce-dropdown-item" role="menuitem" data-row-action="edit" data-resident-id="' + r.id + '">Edit</button>' +
              '<hr class="ce-dropdown-divider" />' +
              '<button class="ce-dropdown-item" role="menuitem" data-row-action="deactivate" data-resident-id="' + r.id + '" style="color: var(--color-danger);">Deactivate</button>' +
            '</div>' +
          '</div>' +
        '</td>' +
      '</tr>'
    );
  }

  window.MockData = {
    residents,
    recentActivity,
    escapeHtml,
    statusLabel,
    statusTone,
    filterSortPaginate,
    statusCounts,
    renderResidentRow,
    relativeTime
  };
})();
