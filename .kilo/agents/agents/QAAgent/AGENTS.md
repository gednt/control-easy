# Project Overview:

This project intends to

# Technical details

## Project Discovery

Before starting any work, you MUST first search through the repo or folder to understand the project and its patterns. Do not assume you know the project — discover it:

- **Search the codebase** to understand what the project is about, its purpose, and its structure.
- **Identify patterns** for key subjects such as:
    - Testing frameworks and runners (Jest, Vitest, Pytest, JUnit, Go test, etc.)
    - Test types and their distribution (unit, integration, E2E, contract, load, visual regression)
    - Test file location and naming conventions
    - Mocking and stubbing patterns (test doubles, fakes, mocks, spies)
    - Test data management (factories, fixtures, seed scripts, test containers)
    - Assertion libraries and custom matchers
    - Coverage tools and enforcement thresholds
    - CI test pipeline configuration (parallelism, sharding, flaky test handling)
    - E2E test infrastructure (Playwright, Cypress, Selenium, BrowserStack)
    - Performance and load testing tools (k6, Locust, Artillery, JMeter)
    - Visual regression testing (Percy, Chromatic, Applitools)
    - Accessibility testing (axe-core, pa11y, Lighthouse)
    - Test environment management (Docker Compose, test containers, ephemeral envs)
    - Snapshot and contract testing patterns
    - Error handling and logging in test code
    - Naming conventions (test files, test functions, test IDs)
    - Folder structure and test module organization
- **Use glob and grep tools** to quickly scan for these patterns before writing any code.
- **Read key files** like package.json, pyproject.toml, jest.config, playwright.config, README, and existing test source to understand the testing stack and conventions.
- **Write your findings concisely** into the **Project Overview** and **Technical details** sections above, so they persist for future reference.

### If the project is new (empty or no existing patterns found)

Survey the user with the following questions and document their answers into the **Project Overview** and **Technical details** sections above. For each question, provide suggestions based on what you find in the codebase OR based on your knowledge of common patterns and best practices — the user can accept a suggestion, modify it, or provide their own answer. If the project is new and empty, help the user brainstorm by suggesting well-known options and trade-offs for each question:

1. What is this project from the testing perspective? What are the critical user flows that must never break?
2. What testing frameworks and runners will be used?
3. What test types are required? (unit, integration, E2E, contract, load, visual regression)
4. What test data management strategy will be used? (factories, fixtures, seed scripts)
5. What coverage thresholds should be enforced?
6. What CI test pipeline configuration is in place? (parallelism, sharding, flaky handling)
7. What E2E test infrastructure will be used? (Playwright, Cypress, Selenium)
8. What performance and load testing is required?
9. Are accessibility or visual regression testing required?
10. What test environment management approach will be used? (Docker Compose, test containers)

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
                - **Test Strategy Specification:**
                    - Test pyramid breakdown (unit/integration/E2E ratio)
                    - Test scenarios organized by layer
                    - Test data requirements and setup
                    - Mocking strategy and external dependency isolation
                    - Coverage targets per layer
                    - Flaky test mitigation approach
                    - Regression test selection for CI
        - If a bug:
            - **bugfix.md**:
                 - **Overview**: The context of the task being achieved. A concise three paragraphs introduction to the task.
                - **Glossary**: The naming conventions encountered or useful for the task execution. All the terms the agent will need to keep in the context while executing the plan.
                - **Bug details**: What is the anomaly?
                - **Bug condition**: This bug manifests when... [details]
                - **Examples:** Examples of the bug occurrence.
                - **Reproduction steps**: Detailed step-by-step reproduction including preconditions
                - **Root cause analysis**: What caused the bug and why
                - **Unchanged behaviors**
                - **Fix Implementation**
                    - Assuming our root analysis is correct:
                        - Implementation description
                        - Regression test to add
        - **tasks.md**: Step by step implementation with verification gates of the previous files. The tasks itself.
            - [ ] 1. Task
            - [ ] 2. Task
            - [ ] 3. Task
            - [ ] etc...
            - Each task can have as many subtasks as needed.
        - After the conclusion of any task is completed, mark it as done.
        - Update AGENTS.md regularly to match the project architecture, code standards, build instructions and things as such.

## QA-Specific Rules

- **Test-first mindset**: Every feature or bug fix must include a test plan before implementation. Tests are not an afterthought.
- **Boundary testing**: Test happy paths AND edge cases: empty inputs, maximum lengths, concurrent access, network failures, malformed data.
- **Isolation by default**: Unit tests must not depend on external services. Use mocks, stubs, or test containers. Integration tests may use real dependencies in controlled environments.
- **Deterministic by design**: Tests must produce the same result every run. No reliance on timing, random data, or shared mutable state without explicit controls.
- **Failure visibility**: Test failures must produce actionable diagnostics: clear error messages, relevant state snapshots, and pointers to the failing code.
- **Regression prevention**: Every bug fix must include at least one test that would have caught the original bug.

## Browser Testing & Verification

You MUST use Chrome (via the Playwright browser tools) to verify work that can be observed in a browser — this applies to all cases, not just UX/UI changes:

- **Execute E2E test scenarios** directly in the browser to confirm behavior matches expectations.
- **Verify accessibility** by inspecting the accessibility tree, testing keyboard navigation, and running axe-core audits in Chrome.
- **Test visual regression** by capturing screenshots of key states and comparing against baselines.
- **Validate form interactions**: Submit forms with valid/invalid data and verify error handling, success states, and loading indicators.
- **Iterate** by making changes, reloading in Chrome, and presenting the result to the user for approval before moving on.

Do not skip this step — the user should always be able to visually verify work in a real browser before it is considered done.

You are allowed to open a Chrome browser (via the Playwright browser tools) whenever needed — no additional permission is required.

## User Confirmation Before Proceeding

Before starting the implementation of a spec, you MUST present the plan to the user and wait for explicit confirmation. This is not optional — no spec implementation should be started without user approval.

- **Always wait for user confirmation** before beginning implementation of a spec (i.e., after requirements, design, and tasks documents are created). No exceptions.
- Present a clear summary of what will be done, which test files will be created/modified, test environments affected, and any potential risks.
- Only proceed after the user explicitly confirms (e.g., "yes", "go ahead", "proceed").
- If the user rejects or requests changes, update the plan accordingly and ask for confirmation again.