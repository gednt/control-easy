# Project Overview:

This project intends to

# Technical details

## Project Discovery

Before starting any work, you MUST first search through the repo or folder to understand the project and its patterns. Do not assume you know the project — discover it:

- **Search the codebase** to understand what the project is about, its purpose, and its structure.
- **Identify patterns** for key subjects such as:
    - Data platform and tooling (dbt, Airflow, Spark, Flink, Prefect, Dagster)
    - Data warehouse or lakehouse (Snowflake, BigQuery, Redshift, Databricks, DuckDB)
    - Data lake storage (S3, GCS, ADLS) and file formats (Parquet, Delta, Iceberg)
    - Orchestration patterns (DAG structure, scheduling, dependencies, backfills)
    - Transformation frameworks (dbt models, Spark jobs, SQL scripts)
    - Data ingestion patterns (CDC, bulk, incremental, streaming)
    - Data modeling approach (star schema, Data Vault, One Big Table, wide tables)
    - Data quality frameworks (Great Expectations, Soda, dbt tests, custom)
    - Partitioning and clustering strategies
    - Naming conventions (tables, columns, schemas, pipelines, models)
    - Folder structure and pipeline module organization
    - Environment management (dev, staging, production data)
    - CI/CD for data pipelines (testing, linting, deployment)
    - Schema registry and evolution patterns
    - Cost management and resource optimization
    - Monitoring and alerting for pipelines
- **Use glob and grep tools** to quickly scan for these patterns before writing any code.
- **Read key files** like dbt_project.yml, airflow dags, pyproject.toml, README, and existing pipeline source to understand the data stack and conventions.
- **Write your findings concisely** into the **Project Overview** and **Technical details** sections above, so they persist for future reference.

### If the project is new (empty or no existing patterns found)

Survey the user with the following questions and document their answers into the **Project Overview** and **Technical details** sections above. For each question, provide suggestions based on what you find in the codebase OR based on your knowledge of common patterns and best practices — the user can accept a suggestion, modify it, or provide their own answer. If the project is new and empty, help the user brainstorm by suggesting well-known options and trade-offs for each question:

1. What is this project from the data engineering perspective? What data flows and pipelines does it manage?
2. What data platform and tooling will be used? (dbt, Airflow, Spark, Flink, etc.)
3. What data warehouse or lakehouse will be used? (Snowflake, BigQuery, Redshift, Databricks)
4. What data ingestion patterns will be used? (CDC, bulk, incremental, streaming)
5. What data modeling approach will be followed? (star schema, Data Vault, One Big Table)
6. What data quality framework will be used? (Great Expectations, Soda, dbt tests)
7. What orchestration patterns will be followed? (DAG structure, scheduling, backfills)
8. What partitioning and clustering strategies are required?
9. What CI/CD approach will be used for data pipelines?
10. What monitoring and alerting is required for pipeline health?

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
                - **Data Pipeline Specification:**
                    - Source systems and extraction method (CDC, bulk, incremental)
                    - Transformation logic and dependencies (dbt models, Spark jobs)
                    - Target schema and data model
                    - Partitioning and clustering strategy
                    - Data quality checks and validation rules
                    - Freshness SLAs and alerting thresholds
                    - Backfill strategy
        - If a bug:
            - **bugfix.md**:
                 - **Overview**: The context of the task being achieved. A concise three paragraphs introduction to the task.
                - **Glossary**: The naming conventions encountered or useful for the task execution. All the terms the agent will need to keep in the context while executing the plan.
                - **Bug details**: What is the anomaly?
                - **Bug condition**: This bug manifests when... [details]
                - **Examples:** Examples of the bug occurrence (data samples, schema mismatches, pipeline failures).
                - **Root cause analysis**: Upstream source change, schema drift, data quality violation, logic error, etc.
                - **Impact assessment**: Downstream consumers affected, data freshness impact, historical data affected
                - **Unchanged behaviors**
                - **Fix Implementation**
                    - Assuming our root analysis is correct:
                        - Implementation description
                        - Backfill procedure for corrected historical data
        - **tasks.md**: Step by step implementation with verification gates of the previous files. The tasks itself.
            - [ ] 1. Task
            - [ ] 2. Task
            - [ ] 3. Task
            - [ ] etc...
            - Each task can have as many subtasks as needed.
        - After the conclusion of any task is completed, mark it as done.
        - Update AGENTS.md regularly to match the project architecture, code standards, build instructions and things as such.

## Data Engineering-Specific Rules

- **Idempotent pipelines**: Every pipeline must be safe to re-run without duplicating data or causing side effects. Use upserts, merge statements, or idempotency keys.
- **Schema evolution safety**: Changes to schemas must be backward-compatible. Additive changes (new nullable columns) are preferred. Destructive changes require a migration plan.
- **Data quality gates**: No data lands in production tables without passing validation. Quality checks are non-negotiable checkpoints, not optional steps.
- **Incremental by default**: Prefer incremental processing over full refreshes. Document the incremental key and watermark strategy for every pipeline.
- **Lineage traceability**: Every dataset must have a traceable origin. Document source systems, transformation logic, and downstream consumers in the spec.
- **Cost-aware processing**: Estimate compute and storage costs for new pipelines. Prefer columnar formats, partition pruning, and right-sized compute.
- **Failure isolation**: A failure in one pipeline must not cascade to unrelated pipelines. Design independent failure handling and alerting per pipeline.

## Verification & Validation

After implementing pipeline changes, you MUST verify:

- **Dry run**: Execute pipelines in a dev/staging environment with sample data before promoting.
- **Data quality checks**: Run validation rules against output data (row counts, null rates, schema conformance, referential integrity).
- **Freshness verification**: Confirm data reaches the target within the defined SLA.
- **Downstream impact**: Verify that dependent pipelines and consumers are not affected by the change.
- **Backfill test**: If applicable, verify the backfill procedure works on a historical data window.

## User Confirmation Before Proceeding

Before starting the implementation of a spec, you MUST present the plan to the user and wait for explicit confirmation. This is not optional — no spec implementation should be started without user approval.

- **Always wait for user confirmation** before beginning implementation of a spec (i.e., after requirements, design, and tasks documents are created). No exceptions.
- Present a clear summary of what will be done, which tables/pipelines will be affected, data quality implications, and potential risks to downstream consumers.
- Only proceed after the user explicitly confirms (e.g., "yes", "go ahead", "proceed").
- If the user rejects or requests changes, update the plan accordingly and ask for confirmation again.