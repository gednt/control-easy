# Editorial Review — Prose

Verdict: **Minor revisions recommended.** The PRD is clear, well-structured, and appropriate for an internal/dev-facing audience. A handful of small ambiguities and inconsistencies should be fixed before finalization.

| Original Text | Revised Text | Changes |
|---------------|--------------|---------|
| FR-2.2: "tears down the worktree's Compose project (containers, networks, and volume), removes the hostname entry" | "tears down the worktree's Compose project (containers, networks, and volumes), removes the hostname entry" | Changed "volume" to "volumes" for consistency with the plural "containers, networks". |
| FR-4.4: "After `worktree-down.sh`, the hostname query returns \"not found\" or NXDOMAIN." | "After `worktree-down.sh`, the hostname query returns NXDOMAIN or \"not found\"." | Lead with the precise DNS term (NXDOMAIN) and keep the plain-language fallback; avoids implying two distinct outcomes. |
| FR-6.2: "On a clean devcontainer with cached base images" | "In a clean devcontainer with cached base images" | Use the standard preposition "in" for an environment/container. |
| FR-7.1: "retired modes A/B/C are clearly marked as not for new work" | "retired modes A/B/C are clearly marked as not for new work" or "retired modes A/B/C are clearly marked retired" | The phrasing is grammatically acceptable but slightly awkward; consider "clearly marked as retired" if a smoother reading is preferred. |
| Phase-level verification gate: "`git worktree list` shows the main checkout plus at least one worktree stack on its own hostname and port" | "`git worktree list` shows the main checkout plus at least one worktree on its own hostname and port" | Removed "stack" to avoid the ambiguous compound "worktree stack"; the worktree runs the stack. |
| Phase-level verification gate: "`curl -k https://ce-<id>.localhost:18080+10N/health` returning 200" | "`curl -k https://ce-<id>.localhost:<port>/health` returning 200, where `<port>` follows the worktree port allocation convention" | The "`18080+10N`" notation is undefined in this document and may confuse readers; reference the convention or define N here. |
| Phase-level verification gate: "`docker compose -p ce-<id> down -v` removes only that worktree's containers, networks, and volume." | "`docker compose -p ce-<id> down -v` removes only that worktree's containers, networks, and volumes." | Changed "volume" to "volumes" to match the plural list. |
| Open Questions, [NOTE FOR PM]: "Although you stated no corporate restrictions, documenting the fallback costs little." | (no change recommended) | Tone is appropriately informal for an internal note to a PM; preserve author voice. |
