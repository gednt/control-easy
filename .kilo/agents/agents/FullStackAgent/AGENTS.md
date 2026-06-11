# Project Overview:

This project intends to

# Technical details

## Project Discovery

Before starting any work, you MUST first search through the repo or folder to understand the project and its patterns. Do not assume you know the project — discover it:

- **Search the codebase** to understand what the project is about, its purpose, and its structure.
- **Identify patterns** for key subjects such as:
    - Frontend framework and component library (React, Vue, Svelte, Angular)
    - Backend framework and runtime (Node.js, Python, Go, Rust, Java, C#)
    - Architectural pattern (Monolith, Microservices, Serverless, Jamstack)
    - Database operations (ORM/ODM, query patterns, migrations, models)
    - API style (REST, GraphQL, gRPC, WebSocket) and versioning strategy
    - Authentication and authorization (JWT, OAuth2, session-based, RBAC)
    - State management (Redux, Zustand, Pinia, Context API, server state)
    - Middleware and interceptor patterns
    - Validation and error handling conventions
    - Testing patterns (unit, integration, E2E, contract testing)
    - CI/CD pipeline and deployment strategy
    - Caching strategy (Redis, Memcached, CDN)
    - Naming conventions (files, variables, classes, functions, exports)
    - Folder structure and module organization
    - Environment and configuration management
    - Internationalization and localization
    - Styling approach (Tailwind, CSS Modules, Styled Components, SCSS)
- **Use glob and grep tools** to quickly scan for these patterns before writing any code.
- **Read key files** like package.json, pyproject.toml, go.mod, README, config files, and existing source to understand the tech stack and conventions.
- **Write your findings concisely** into the **Project Overview** and **Technical details** sections above, so they persist for future reference.

### If the project is new (empty or no existing patterns found)

Survey the user with the following questions and document their answers into the **Project Overview** and **Technical details** sections above. For each question, provide suggestions based on what you find in the codebase OR based on your knowledge of common patterns and best practices — the user can accept a suggestion, modify it, or provide their own answer. If the project is new and empty, help the user brainstorm by suggesting well-known options and trade-offs for each question:

1. What is this project from the full-stack perspective? What user flows span both frontend and backend?
2. What frontend framework and component library will be used?
3. What backend framework, language, and runtime will be used?
4. What database and ORM/ODM will be used? How are models, tables, and fields named?
5. What API style will be used? (REST, GraphQL, gRPC) Any auth or middleware patterns?
6. How will state be managed across the stack? What is the server state vs client state boundary?
7. What testing framework and strategy will be used? Where do tests live?
8. What are the shared types/interfaces between frontend and backend?
9. What styling approach will be used on the frontend?
10. Are there any build, CI/CD, or deployment instructions to document?

### After discovery is complete

Once all findings have been documented into the **Project Overview** and **Technical details** sections — or if the user opts not to answer the survey questions — remove this entire **Project Discovery** section from AGENTS.md. Its purpose is one-time bootstrapping only.

# Strategy

For each implementation, generate a folder with the implementation name in the format `.specs/function-or-bug-fix`, each folder will have the name of the function or bug fix it intends to achieve.
- Inside it, you will generate three documents:
    - **Requirements:** What needs to happen, in the format of user story:
        - **UC[Number]:** As a user, I want/need to [what needs to happen or needs to be fixed]
    - **Conditional**:
        - If a feature:
            - **design.md:** How it will be done.
                - **Overview**: The context of the task being achieved. A concise three paragraphs introduction to the task.
                - **Glossary**: The naming conventions encountered or useful for the task execution. All the terms the agent will need to keep in the context while executing the plan.
                - **Architecture:**
                    - A Brief paragraph describing the architecture of the design of the feature.
                    - Titles with mermaid diagrams, flow, success criteria, components, files to be created, code snippets, testing strategy, verification approach, etc... Everything the design needs to be achieved... More items can be added as needed.
                        - Each item will be a title or subtitle.
                - **Full-Stack Integration Map:**
                    - Data flow from user action → API call → server handler → database → response → UI update
                    - Shared types/interfaces between frontend and backend
                    - Server state vs client state boundary
                    - Authentication and authorization flow across the stack
                - **API Contract Specification:**
                    - Endpoint definitions (path, method, request/response schemas)
                    - Authentication and authorization requirements
                    - Error response schemas and status codes
                - **UI/UX Specification:**
                    - Component hierarchy and composition
                    - Loading, error, empty, and success states
                    - Accessibility requirements
        - If a bug:
            - **bugfix.md**:
                 - **Overview**: The context of the task being achieved. A concise three paragraphs introduction to the task.
                - **Glossary**: The naming conventions encountered or useful for the task execution. All the terms the agent will need to keep in the context while executing the plan.
                - **Bug details**: What is the anomaly?
                - **Bug condition**: This bug manifests when... [details]
                - **Examples:** Examples of the bug occurrence.
                - **Stack traces and logs**: Relevant error output
                - **Reproduction steps**: Step-by-step instructions covering both frontend and backend
                - **Unchanged behaviors**
                - **Fix Implementation**
                    - Frontend fix description
                    - Backend fix description
                    - Data migration steps if schema changes are required
        - **tasks.md**: Step by step implementation with verification gates of the previous files. The tasks itself.
            - [ ] 1. Task
            - [ ] 2. Task
            - [ ] 3. Task
            - [ ] etc...
            - Each task can have as many subtasks as needed.
            - ## Task Dependency Graph
            ```json
            {
              "waves": [
                { "wave": 1, "tasks": ["<task_id>", "..."] },
                { "wave": 2, "tasks": ["<task_id>", "..."] }
              ]
            }
            ```
            Task IDs reference the numbered tasks above. Tasks in the same wave have no dependencies on each other and can run in parallel. A wave only starts after all tasks in the previous wave are complete. Add as many waves as the task complexity demands.
        - After the conclusion of any task is completed, mark it as done.
        - Update AGENTS.md regularly to match the project architecture, code standards, build instructions and things as such.

## Full-Stack-Specific Rules

- **End-to-end thinking**: Every feature must be designed across the full stack before implementation. No backend-only or frontend-only shortcuts.
- **Type safety across the boundary**: Shared types/interfaces between frontend and backend must be kept in sync. Prefer a shared types package or code generation over duplicate definitions.
- **State ownership clarity**: Document which layer owns each piece of state. Server state belongs to the server; derive client state from server responses whenever possible.
- **Error propagation**: Backend errors must be transformed into user-facing frontend errors at the API boundary. Never leak internal error details to the client.
- **Migration safety**: Database migrations must be backward-compatible with the currently deployed frontend. Deploy backend changes before frontend changes when schemas evolve.

## Browser Testing & Verification

You MUST use Chrome (via the Playwright browser tools) to verify work that can be observed in a browser — this applies to all cases, not just UX/UI changes:

- **Present the current state** to the user so they can see how things look before proceeding. Take screenshots or snapshots after each meaningful change and share them with the user for feedback.
- **Test full-stack flows** end-to-end: form submission → API call → database write → confirmation in UI.
- **Test browser-dependent functionality** (e.g., WebSocket real-time feeds, file uploads, SSR rendering, hydration) directly in Chrome rather than relying solely on code review.
- **Iterate** by making changes, reloading in Chrome, and presenting the result to the user for approval before moving on.

Do not skip this step — the user should always be able to visually verify work in a real browser before it is considered done.

You are allowed to open a Chrome browser (via the Playwright browser tools) whenever needed — no additional permission is required.

## User Confirmation Before Proceeding

Before starting the implementation of a spec, you MUST present the plan to the user and wait for explicit confirmation. This is not optional — no spec implementation should be started without user approval.

- **Always wait for user confirmation** before beginning implementation of a spec (i.e., after requirements, design, and tasks documents are created). No exceptions.
- Present a clear summary of what will be done, which files will be affected across the stack, any database changes, and potential risks.
- Only proceed after the user explicitly confirms (e.g., "yes", "go ahead", "proceed").
- If the user rejects or requests changes, update the plan accordingly and ask for confirmation again.