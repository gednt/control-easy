# Agents

A multi-agent spec-driven development framework with an orchestrator and domain-specialized agents for structured, traceable implementation.

## Overview

This repository provides an orchestrated agent system where a central **Orchestrator Agent** routes tasks to the right domain-specialized agent. Each agent follows a spec-driven methodology: breaking down features and bug fixes into well-defined specifications before writing code. Work is documented with requirements, design (or bug analysis), and a task checklist — ensuring clarity and traceability from idea to implementation.

## Agent Architecture

```text
AGENTS.md                          # Orchestrator Agent (routing, delegation, coordination)
agents/
├── SpecDrivenDevelopment/         # General-purpose spec-driven implementation
├── FrontendAgent/                  # UI, styling, accessibility, client-side state
├── BackendAgent/                   # APIs, databases, auth, server logic
├── FullStackAgent/                 # End-to-end features (frontend + backend)
├── QAAgent/                        # Testing strategy, coverage, regression
├── CodeReviewAgent/                # Branch diff review & general code analysis
├── DocumentationAgent/             # Technical docs, API docs, guides, changelogs
├── DevOpsAgent/                    # Infrastructure, CI/CD, deployment, monitoring
└── DataEngineerAgent/              # Pipelines, ETL/ELT, data quality, modeling
```

Each agent has its own `AGENTS.md` with domain-specific discovery, strategy, rules, and verification steps.

### Agent Discovery Types

| Type | Agents | Mechanism |
|---|---|---|
| **Project Discovery** | SpecDrivenDevelopment, FrontendAgent, BackendAgent, FullStackAgent, QAAgent, DevOpsAgent, DataEngineerAgent | Scans codebase + surveys user with brainstorming suggestions |
| **Context Discovery** | CodeReviewAgent, DocumentationAgent | Reads config/tooling files only — no user survey needed |

## Global Rules

- **No emojis** — across all agents, specs, code, commits, documentation, and communication.
- **Documentation Agent language** — asks the user what language to write docs in before starting any task.
- **Code Review Agent language** — writes reviews in the same language used in the user's prompt.

## Project Context Flow

1. **Orchestrator routes first** — classifies the request and delegates to the correct agent.
2. **Destination agent fills its own overview** — upon first interaction, the agent runs its domain-specific discovery and populates its `Project Overview` and `Technical details`.
3. **No pre-filling by orchestrator** — each agent owns its own discovery process.

## Spec Structure

```text
.specs/
└── <feature-or-bug-fix>/
    ├── requirements.md   # User stories (UC[n]: As a user, I want/need to...)
    ├── design.md         # Feature design (overview, glossary, architecture, diagrams)
    ├── bugfix.md         # Bug analysis (overview, condition, examples, fix plan)
    ├── review.md         # Branch review findings (Code Review Agent)
    ├── analysis.md       # General code analysis (Code Review Agent)
    ├── docplan.md       # New documentation structure plan (Documentation Agent)
    ├── docchange.md     # Existing documentation update plan (Documentation Agent)
    ├── orchestration.md  # Multi-agent coordination log (Orchestrator)
    └── tasks.md          # Step-by-step implementation checklist
```

- Use **design.md** for features
- Use **bugfix.md** for bug fixes
- Use **docplan.md** for new documentation sets
- Use **docchange.md** for updating existing docs
- Use **review.md** for branch diff reviews
- Use **analysis.md** for general code analysis
- **tasks.md** is always required and lists implementation steps with verification gates. Includes a **Task Dependency Graph** section defining parallel execution waves:
    ```json
    {
      "waves": [
        { "wave": 1, "tasks": ["<task_id>", "..."] },
        { "wave": 2, "tasks": ["<task_id>", "..."] }
      ]
    }
    ```
    Task IDs reference the numbered tasks. Tasks in the same wave have no dependencies on each other and can run in parallel. A wave only starts after all tasks in the previous wave are complete.

## How It Works

1. **User submits a task** — the Orchestrator classifies it (feature, bug fix, refactoring, testing, infrastructure, data, code review, documentation).
2. **Orchestrator routes** — delegates to the appropriate specialized agent(s), presenting the plan for user confirmation.
3. **Agent discovers** — the destination agent runs its domain-specific discovery (project discovery with user survey, or context discovery from config files).
4. **Agent specs** — requirements, design or bug analysis, and task checklist are created.
5. **Agent implements** — tasks are executed with verification gates, marking each done.
6. **Orchestrator verifies** — confirms completion, cross-agent consistency, and alerts to any remaining issues.

## Routing Rules

| Request Type | Routed To |
|---|---|
| Frontend-only | FrontendAgent |
| Backend-only | BackendAgent |
| Frontend + Backend | FullStackAgent |
| Testing / QA | QAAgent |
| Code review / analysis | CodeReviewAgent |
| Technical docs / API docs / changelogs | DocumentationAgent |
| Infrastructure / CI/CD | DevOpsAgent |
| Data pipelines / modeling | DataEngineerAgent |
| Unclear / general | SpecDrivenDevelopment |

## Agent Config Files

Each AI coding agent reads instructions from a specific config file. This project provides all of them at the root — each one simply says "Follow AGENTS.md", pointing to the shared Orchestrator instructions.

| Agent | Config File |
|---|---|
| Claude | `CLAUDE.md`, `.claude/rules/spec-driven-development.md` |
| Gemini | `GEMINI.md` |
| GitHub Copilot | `.github/copilot-instructions.md` |
| Cursor | `.cursor/rules/spec-driven-development.mdc` |
| Windsurf (Devin) | `.devin/rules/spec-driven-development.md` |
| Amazon Q | `.amazonq/rules/spec-driven-development.md` |
| Cline | `.clinerules/spec-driven-development.md` |
| Aider | `CONVENTIONS.md` |
| Kiro | `.kiro/steering/spec-driven-development.md` |

No build or transform step is needed — just use the repo with your preferred AI coding agent and it will follow the root `AGENTS.md`.

## Contributing

1. Create a new spec folder under `.specs/` following the naming convention `.specs/<feature-or-bug-fix>`.
2. Fill in the required documents (requirements, design/bugfix, tasks).
3. Implement tasks incrementally, marking them done as you go.
4. Keep `AGENTS.md` updated with project architecture, code standards, and build instructions.

## License

This project is provided as-is. See individual files for any additional terms.