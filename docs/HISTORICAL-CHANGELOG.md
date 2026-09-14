# ControlEasy Classic — Historical Release Chronicle (2014)

> **Historical Document:** This chronicle preserves the authentic release notes from the original desktop incarnation of **ControlEasy**, developed by **Felipe Coelho** during his technical degree studies at **ETEC** and maintained across its initial years of evolution.

---

## Quick Navigation

- [Phase 0.x — Prototyping & First Steps](#phase-0x--prototyping--first-steps)
- [Phase 1.x — Beta & First Stable Releases](#phase-1x--beta--first-stable-releases)
- [Phase 2.x — Maturation, Vehicles & Database Transition](#phase-2x--maturation-vehicles--database-transition)
- [Phase Z 2.9.x — The "Three Powers", Security & Expansion](#phase-z-29x--the-three-powers-security--expansion)
- [Phase Z 3.x — The Desktop Zenith: Full SQL, Webcam & QR Codes](#phase-z-3x--the-desktop-zenith-full-sql-webcam--qr-codes)

---

## Phase 0.x — Prototyping & First Steps

### Version 0.1
- Added basic Resident Registration functionality.
- Added basic Resident Lookup functionality (Search by National ID / RG not yet implemented).
- Added experimental photo viewing for condominium residents.
- Floor-by-floor lookup functionality is not yet fully functional.

### Version 0.2
- Floor lookup routine completed up to the 5th floor (3rd floor missing).
- Search by RG (National ID) implemented.

### Version 0.3
- Floor lookup routine completed up to the 30th floor.
- Search by RG removed (search button non-functional).

### Version 0.4
- Resident lookup routine by RG successfully implemented.

### Version 0.5
- Visitor entry partially implemented.

### Version 0.6
- Basic resident entry routine implemented (entry only, exit pending).

### Version 0.7
- Basic resident exit routine implemented.
- Resident entry routine successfully implemented.
- Visitor entry and exit routine successfully implemented.

### Version 0.8
- Resident exit routine removed due to failures.
- Visitor exit routine removed due to failures.

### Version 0.9
- Implemented basic entry and exit routine for service providers.
- Implemented final entry and exit algorithm for residents.

---

## Phase 1.x — Beta & First Stable Releases

### Version Beta 1.0 — Control Easy
- Application title added.
- Several improvements to data entry workflows.
- Version deemed stable by the developer and delivered to the client.

### Version 1.1 Release Candidate
- Battery of tests performed.
- UI bug fixes and polish.
- Fixes in data addition.
- Bug fixes.

### Version 1.2 Release Candidate
- Bug fixes.

### Version 1.3 Release Candidate
- Bug fixes.

### Version 1.4 Stable Version
- Partial definitive graphical user interface deployed.
- Bug fixes and security patches.

### Version 1.5 Stable Version
- Bug fixes.

### Version 1.6 Stable Version
- Resident registration screen updated to its definitive version.
- General usability improvements.

### Version 1.7.9 Stable Version
- Added ability to customize the application (required upon every application launch).
- Added vehicle registration support (still experimental).
- Final adjustments for the second stable release.

---

## Phase 2.x — Maturation, Vehicles & Database Transition

### Version 2.0
- Application customization finalized (with minor pending bugs).
- Added credentialed user requirement for registering residents.
- Added user account and password management.
- Added definitive vehicle entry and exit routines.
- Numerous bug fixes.
- Updated images across several application screens.
- Graphical interface nearly complete.

### Version 2.0.1
- Bug fixes.

### Version 2.0.2
- Bug fixes.

### Version 2.0.3
- Fixed vehicle exit bug.

### Version 2.5
- Added webcam photo capture functionality for residents.
- Bug fixes.

### Version 2.5.1
- Improved resident entry workflow (automatic autofill of selected fields).

### Version 2.5.2
- Improved service provider entry and bug fixes.

### Version 2.8.9 Stable Version
- Migration to 64-bit architecture.
- Migration of key system areas to MySQL (early foundation of DBTools).
- Experimental redesign of resident registration and lookup.
- Experimental redesign of vehicle registration and lookup.
- Numerous bug fixes.

### Version 2.9.0 (64-bit)
- **Resident Entry Enhancement:** When searching by RG, the resident's photo is displayed first. Clicking the release button confirms that the resident is authorized to enter.
- **Resident Exit Enhancement:** Performance optimization by reusing the lookup interface routines.
- *Historical note:* Initial handling of screen flickering during fast queries and manual closing of error dialogs.

---

## Phase Z 2.9.x — The "Three Powers", Security & Expansion

> **Historical Note:** The "Z" lineage represented a major architectural re-engineering of gatehouse access control and memory management.

### Version 2.9.1 — *FAILED VERSION (Inoperable)*
> **Status:** `UNSTABLE & INOPERANT`  
> Attempted improvements to resident entry and added a login window for system access. However, a structural fault led to this version being abandoned and marked for the first time as unstable and inoperable — serving as the foundation for the next release.

### Version Z 2.9.2
- Successfully implemented login screen for credentialed users. Test cases passed.
- Hardened security across multiple aspects of the program.
- Reduced memory footprint through code cleanup.
- If the client does not specify a custom condominium image, a default image is automatically selected to prevent personalization errors (can be changed later).

### Version Z 2.9.3
- **Gatehouse Audit Logging:** When authorizing resident entry or exit, the logged-in gatehouse attendant's name is recorded in the ledger, ensuring full accountability for who authorized access.
- Reduced memory usage through continued codebase cleanup.
- Duplicate entry warning: if a resident is already registered as inside the premises, the system alerts the operator and asks whether to open a new entry log.

### Version Z 2.9.4
- Added operational reporting for resident entry and exit records.
- *Note:* Remember to restore `TelaInicial` (Main Screen) as the application startup form.

### Version Z 2.9.5
- Visitor entry and exit remodeled under the new automated release standard with operator audit logging.
- Added gatehouse shift handover support (switch accounts without restarting the application).

### Version Z 2.9.6
- Added dedicated login and administrative module for administrators (building managers / síndicos).
- Added creation, deletion, and editing of administrator credentials.

### Version Z 2.9.7
- **Completed "The Three Powers" Authentication:** Full separation of roles and permissions:
  1. *Credentialed User*
  2. *Gatehouse Attendant (Porteiro)*
  3. *Administrator (Síndico / Building Manager)*
- Administrative users can access management sections freely without re-entering passwords on every action.
- Visual overhaul of ControlEasy's main screen.
- *Author's note:* One of the most satisfying releases in the author's opinion; selected for the final course presentation (PTCC).

### Version Z 2.9.8
- Completed the MySQL migration for service provider entry and exit routines.

### Version Z 2.9.9
- Added vehicle entry and exit routines (testing phase).
- Added gatehouse attendant management by administrators (testing phase).
- Fixed login bug that allowed arbitrary usernames or passwords for attendants.
- General graphical redesign and bug fixes.
- Final adjustments for the third stable release.

---

## Phase Z 3.x — The Desktop Zenith: Full SQL, Webcam & QR Codes

### Version Z 3.0.0
- Beta implementation of the user privileges and role-based access system.
- Comprehensive graphical overhaul across the entire application.
- Administrative notification indicating whether the current operator is an attendant or administrator.
- Single sign-on within administrative mode (enter password once for the entire session).
- Major code cleanup to reduce memory consumption and maximize performance.
- Integrated reporting for all entry and exit operations with printing capabilities.

### Version Z 3.0.1
- Webcam interface enhancements; partially resolved accidental automatic webcam activation. Ready for debugging.

### Version Z 3.0.2
- **Complete Abandonment of Microsoft Access:** Full migration of system data and operations to SQL / MySQL.
- Quick navigation shortcut codes to jump directly between system functions.
- Added the "About" screen with system information.
- Code cleanup and bug fixes.

### Version Z 3.0.3
- Fixed application personalization bug where selected condominium images were not saving.

### Version Z 3.0.4
- Added developer quick-access shortcut for in-field debugging (enter password directly).

### Version Z 3.1.5
- Improved core application routines, particularly vehicle registration (automatically checks resident registry for existing RG).
- Fixed save button bug in vehicle registration.
- **Screen Flickering Fix:** External query dialogs now execute minimized in the background and close automatically upon completion, eliminating screen flickering.
- Code cleanup.

### Version Z 3.2.5
- Enhanced resident entry: duplicate entries prevented (if resident is already inside, duplicate entry is blocked).
- Enhanced resident exit: photo lookup minimized during query execution.
- Fixed vehicle image preview bug.
- Eliminated screen flicker when querying resident records across all system forms.
- Required visitor name before granting building access.
- Minor visual repositioning of main screen icons.
- Webcam preview rendering improvements.

### Version Z 3.2.5.1
- Enhanced all registration routines by automatically converting input fields to uppercase for search uniformity.

### Version Z 3.3
- Expanded search capabilities across all entry and exit release screens.
- Security hardening.

### Version Z 3.3.1
- Integrated launchers for prerequisite software installers (MySQL and dependencies) directly from the user interface.
- Data import functionality (*Advanced Technique*).
- Login screen correction: credentialed user acknowledged as an administrator.

### Version Z 3.4.1
- Prototyped resident identification via QR Code Card (scheduled for full release in next version).

### Version Z 3.5.1
- Full implementation and successful testing of resident QR Code Card identification.
- Instant QR code generator built into the system.
- Included QR code test suite library.
- Note: Next version will support printing QR cards with resident and condominium info.

### Version Z 3.5.2
- Improved QR card entry: continuous camera focus on the same QR card only triggers a single entry event.

### Version Z 3.5.2.1
- Fixed webcam form lifecycle: camera automatically disconnects and releases device handles when forms close.

### Version Z 3.5.2.2
- Improved QR code capture: data validation happens directly inside the QR capture form.

### Version Z 3.5.2.3
- Improved gatehouse entry controls.
- Fixed resident entry data insertion bugs.
- Added physical printing of customized QR Code cards featuring resident names and condominium branding.

### Version Z 3.5.2.4
- Bug fixes, resident registration improvements, and resident exit adjustments.

### Version Z 3.6.0.0
- Added system tray notification icon to control background functions (toggling cameras on/off).
- Added user option to run with or without webcam support.
- Redesigned resident entry and exit algorithms.
- Created the **Interact V3.0.3 Algorithm**.

### Version Z 3.6.0.1
- Added system tray quick-action menu: switch operator, entry/exit operational reports, manual QR decoder/generator.

### Version Z 3.6.0.2
- Redesigned system tray control panel with intuitive iconography.

### Version Z 3.6.0.3
- Main screen wallpaper rendering improvements and null image fallback fix.
- Partially resolved accidental webcam activation when opening the tray control panel.
- Added system clock display to the tray control panel interface.

### Version Z 3.6.0.4 Silver Version
- Definitive fix for accidental webcam activation from the tray control panel.
- Safe camera handling: if the user starts without a webcam, camera controls are hidden and disabled to prevent hardware exceptions.

### Version Z 3.6.0.5
- Flexible exit workflow: resident exit no longer strictly requires a prior recorded entry (handles scenarios where a resident departs by car but returns on foot, or vice-versa).

### Version Z 3.6.0.6
- Fixed cold-start QR card image saving bug where an error dialog appeared without persisting files.
- Final recorded desktop release of the classic era before the conceptual leap to modern web architecture and multi-tenancy in **ControlEasy Reborn**.

---

## Evolution: ControlEasy Reborn

Following Version Z 3.6.0.6, the desktop era concluded. The platform was completely rebuilt from the ground up as **ControlEasy Reborn** — an enterprise-grade, multi-tenant web modular monolith on ASP.NET Core 8 minimal APIs, Angular 18, and DBTools.

For all release notes of the modern platform (v1.0.0, v1.1.0, v2.0.0, and beyond), see the primary [CHANGELOG.md](../CHANGELOG.md).

