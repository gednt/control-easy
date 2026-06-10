# Project Overview:

This project intends to

# Technical details

## Project Discovery

Before starting any work, you MUST first search through the repo or folder to understand the project and its patterns. Do not assume you know the project — discover it:

- **Search the codebase** to understand what the project is about, its purpose, and its structure.
- **Identify patterns** for key subjects such as:
    - UI framework and component library (React, Vue, Svelte, Angular, Web Components)
    - Styling approach (CSS Modules, Tailwind, Styled Components, SCSS, CSS-in-JS)
    - State management (Redux, Zustand, Pinia, MobX, Context API, signals)
    - Routing and navigation patterns
    - Form handling and validation libraries
    - Accessibility (ARIA patterns, screen reader support, keyboard navigation)
    - Responsive design and breakpoint conventions
    - Component composition patterns (compound components, render props, hooks)
    - Animation and transition libraries
    - Testing patterns (Jest, Vitest, Cypress, Playwright, Testing Library)
    - Build tooling and bundler (Vite, Webpack, Turbopack, esbuild)
    - Naming conventions (files, variables, classes, functions, exports)
    - Folder structure and module organization
    - Error boundary and error handling conventions
    - Internationalization and localization
- **Use glob and grep tools** to quickly scan for these patterns before writing any code.
- **Read key files** like package.json, README, config files, and existing source to understand the tech stack and conventions.
- **Write your findings concisely** into the **Project Overview** and **Technical details** sections above, so they persist for future reference.

### If the project is new (empty or no existing patterns found)

Survey the user with the following questions and document their answers into the **Project Overview** and **Technical details** sections above. For each question, provide suggestions based on what you find in the codebase OR based on your knowledge of common patterns and best practices — the user can accept a suggestion, modify it, or provide their own answer. If the project is new and empty, help the user brainstorm by suggesting well-known options and trade-offs for each question:

1. What is this project from the UI perspective? What screens and flows does the user interact with?
2. What UI framework and component library will be used?
3. What styling approach will be used? (Tailwind, CSS Modules, Styled Components, SCSS, etc.)
4. How will state be managed? (Redux, Zustand, Pinia, Context API, etc.)
5. What routing solution will be used?
6. What accessibility standards must be met? (WCAG level, ARIA patterns)
7. What responsive design breakpoints are used?
8. What testing framework and strategy will be used? Where do tests live?
9. What build tool and bundler will be used?
10. Is internationalization/localization required?

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
                - **UI/UX Specification**:
                    - Component hierarchy and composition
                    - Layout diagrams and responsive behavior
                    - Interaction patterns (hover, focus, loading, error, empty states)
                    - Accessibility requirements (ARIA roles, keyboard navigation, screen reader announcements)
                    - Animation and transition specifications
        - If a bug:
            - **bugfix.md**:
                 - **Overview**: The context of the task being achieved. A concise three paragraphs introduction to the task.
                - **Glossary**: The naming conventions encountered or useful for the task execution. All the terms the agent will need to keep in the context while executing the plan.
                - **Bug details**: What is the anomaly?
                - **Bug condition**: This bug manifests when... [details]
                - **Examples:** Examples of the bug occurrence.
                - **Reproduction steps**: Step-by-step instructions to reproduce in the browser
                - **Unchanged behaviors**
                - **Fix Implementation**
                    - Assuming our root analysis is correct:
                        - Implementation description
        - **tasks.md**: Step by step implementation with verification gates of the previous files. The tasks itself.
            - [ ] 1. Task
            - [ ] 2. Task
            - [ ] 3. Task
            - [ ] etc...
            - Each task can have as many subtasks as needed.
        - After the conclusion of any task is completed, mark it as done.
        - Update AGENTS.md regularly to match the project architecture, code standards, build instructions and things as such.

## Frontend-Specific Rules

- **Component-first design**: Every piece of UI must be decomposed into reusable, composable components before implementation begins.
- **Accessibility audit**: All implemented UI must meet WCAG 2.1 AA at minimum. Include ARIA attributes, keyboard navigation, and focus management in every component spec.
- **Responsive by default**: All layouts must be designed mobile-first. Breakpoints and adaptive behavior must be documented in design.md.
- **State locality principle**: Prefer local component state. Only elevate state to global stores when data is shared across distant components or needs to persist across routes.
- **Visual regression verification**: After implementing any UI change, capture a screenshot or snapshot and compare against the expected output.

## Browser Testing & Verification

You MUST use Chrome (via the Playwright browser tools) to verify work that can be observed in a browser — this applies to all cases, not just UX/UI changes:

- **Present the current state** to the user so they can see how things look before proceeding. Take screenshots or snapshots after each meaningful change and share them with the user for feedback.
- **Test browser-dependent functionality** (e.g., component rendering, form interactions, responsive layouts, animations, accessibility tree, keyboard navigation) directly in Chrome rather than relying solely on code review.
- **Verify accessibility** by inspecting the accessibility tree and testing keyboard navigation in the browser.
- **Iterate** by making changes, reloading in Chrome, and presenting the result to the user for approval before moving on.

Do not skip this step — the user should always be able to visually verify work in a real browser before it is considered done.

You are allowed to open a Chrome browser (via the Playwright browser tools) whenever needed — no additional permission is required.

## User Confirmation Before Proceeding

Before starting the implementation of a spec, you MUST present the plan to the user and wait for explicit confirmation. This is not optional — no spec implementation should be started without user approval.

- **Always wait for user confirmation** before beginning implementation of a spec (i.e., after requirements, design, and tasks documents are created). No exceptions.
- Present a clear summary of what will be done, which components and files will be affected, and any potential risks.
- Only proceed after the user explicitly confirms (e.g., "yes", "go ahead", "proceed").
- If the user rejects or requests changes, update the plan accordingly and ask for confirmation again.