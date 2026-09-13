---
name: ControlEasy Reborn
description: A dependable operational record for condominium life.
colors:
  ledger-paper: "#e8e3d7"
  record-surface: "#fffdf7"
  charcoal-rail: "#182a33"
  brick-stamp: "#a84d3d"
  brass-priority: "#7e5c22"
  record-line: "#c9c1b3"
  ink: "#182831"
  slate-note: "#52646b"
typography:
  display:
    fontFamily: "Fraunces, Georgia, serif"
    fontSize: "clamp(2.05rem, 3.4vw, 3.35rem)"
    fontWeight: 500
    lineHeight: 1.1
    letterSpacing: "-0.04em"
  body:
    fontFamily: "Manrope, sans-serif"
    fontSize: "1rem"
    fontWeight: 400
    lineHeight: 1.5
  label:
    fontFamily: "DM Mono, monospace"
    fontSize: "0.75rem"
    fontWeight: 700
    lineHeight: 1
    letterSpacing: "0.08em"
rounded:
  sm: "2px"
  md: "4px"
  lg: "8px"
spacing:
  compact: "8px"
  standard: "16px"
  section: "24px"
  spacious: "32px"
components:
  button-primary:
    backgroundColor: "{colors.brick-stamp}"
    textColor: "{colors.record-surface}"
    typography: "{typography.label}"
    rounded: "{rounded.sm}"
    padding: "0 16px"
    height: "44px"
  record-surface:
    backgroundColor: "{colors.record-surface}"
    rounded: "{rounded.sm}"
    padding: "24px"
---

# Design System: ControlEasy Reborn

## Overview

**Creative North Star: "The Shift Ledger"**

ControlEasy is designed as the trustworthy working record passed between a gatehouse attendant, an administrator, and a resident—not as a generic analytics dashboard. Warm paper surfaces, ruled rows, stamped exceptions, and a dark operational rail make work legible at a glance while retaining a calm, human tone.

The system earns its character through operational evidence: records align to lines, hierarchy has the restraint of a ledger, and the brick-red accent appears when a decision, exception, or new entry needs attention. It rejects glassy dashboards, inflated rounded cards, and decorative gradients.

**Key Characteristics:**

- Charcoal rail, warm paper workspace, and precise brick-red actions.
- Editorial display type paired with compact monospaced operational labels.
- Structural rules, tab dividers, and stamped states instead of floating decorative cards.

## Colors

The palette is an operations desk: paper and ink carry most information; brick and brass are reserved for consequential actions and exceptions.

### Primary

- **Brick Stamp:** Used for primary actions, active navigation, state stamps, and handover emphasis.
- **Brass Priority:** Used for held or priority conditions and the directory’s record edge.

### Neutral

- **Ledger Paper:** The broad working ground that prevents the app from feeling like a blank dashboard.
- **Record Surface:** The clean paper layer for lists, forms, and handover panels.
- **Charcoal Rail:** The persistent operational frame and the only substantial dark field.
- **Ink and Slate Note:** High-contrast record text and secondary annotation respectively.

**The Rare Stamp Rule.** Brick is never a decorative wash. Use it to identify the next consequential action, a selected location, or a recorded exception.

## Typography

**Display Font:** Fraunces, with Georgia as fallback.

**Body Font:** Manrope, sans-serif.

**Label/Mono Font:** DM Mono, monospace.

**Character:** Fraunces gives record titles a considered editorial gravity; Manrope keeps dense operational text calm and readable; DM Mono makes timestamps, column labels, and state marks scan as metadata.

### Hierarchy

- **Display:** Fraunces 500 at the display scale. Use for page and record titles only.
- **Headline:** Fraunces 500 at 1.5rem. Use for titled ledger sections and handover notes.
- **Body:** Manrope 400 at 1rem. Use for explanatory and transactional content.
- **Label:** DM Mono 700 at 0.75rem, uppercase with tracked letters. Use for navigation groups, columns, timestamps, and record classifications.

**The Two-Speed Rule.** Human-readable titles are editorial; operational metadata is monospaced and compact. Never make every label loud.

## Layout

The authenticated shell is a slim charcoal rail plus a restrained top utility bar. The dashboard’s wide view has a central working ledger and a right-side directory/handover stack; at 960px the stack moves beneath the ledger, and at 680px ledger data collapses into two-column rows without losing status or time. The spacing rhythm is 8px, 16px, 24px, then 32px.

## Elevation & Depth

Depth is structural, not glossy: paper records get a soft ambient lift, while primary actions receive a small hard offset that reads like a physical stamp. `--shadow-card` is the default record lift; `--shadow-primary-glow` is reserved for emphasis and interaction.

**The Paper Stack Rule.** Lift only a record that needs separation from the desk. Use borders, rules, and tonal paper before adding shadow.

## Shapes

The form language is mostly square and precisely edged: 2px for stamps and controls, 4px for fields, and 8px only where a larger container needs a softened edge. Rows and panels use straight borders, top rules, and occasional left record bars. Pills belong only to compact status signals.

## Components

### Buttons

- **Primary:** Brick Stamp background, paper text, DM Mono label treatment, a 2px outline, and a subtle physical offset.
- **Secondary:** Paper or transparent background with a visible record line; never a filled neutral capsule.
- **Hover / Focus:** Primary actions shift by a pixel like a stamp; keyboard focus remains a high-contrast outline.

### Cards / Containers

- **Character:** Paper records, not floating widgets.
- **Shape:** Square or 2px corners with a charcoal top rule or brick left rule.
- **Background:** Record Surface, with ledger lines where a live work list benefits from them.

### Inputs / Fields

- **Style:** Paper field, visible record line, 0–4px corners, and clear ink text.
- **Focus:** A visible primary outline that preserves the field’s structural edge.

### Navigation

- **Style:** A charcoal operational rail with grouped mono labels and a brick active state.
- **Mobile:** The rail yields to a compact utility bar; essential context and actions remain in the first viewport.

### Shift Ledger

- **Character:** A live, ruled queue that joins arrivals, destinations, record states, and time in one scan path.
- **State:** A newly logged entry briefly moves into the handover state, reinforcing that actions become part of the shift record.

## Do's and Don'ts

### Do:

- **Do** make the next operational action obvious with one brick stamp.
- **Do** use ruled rows, tab dividers, and monospaced labels to make dense records fast to scan.
- **Do** preserve the line from live work to a chronological handover on gatehouse surfaces.

### Don't:

- **Don't** replace records with a field of isolated rounded cards.
- **Don't** use gradients, glass effects, or generic blue dashboard accents.
- **Don't** use system UI fonts for display hierarchy or operational labels.
