
- source_spec: `_bmad-output/implementation-artifacts/spec-fix-bootstrap-healthcheck-race.md`
  summary: DemoSeederService ignores InsertAsync/UpdateAsync bool results throughout; partial demo seed failure is silent (less severe due to seed-version self-heal on next boot).
  evidence: DemoSeederService.cs SeedUsersAsync/InsertUserAsync/WriteSeedVersionAsync all discard bool returns; audit-only per spec.
- source_spec: `_bmad-output/implementation-artifacts/spec-fix-bootstrap-healthcheck-race.md`
  summary: API container has no restart policy; a DB outage longer than the ~46s retry window kills the stack until manual intervention.
  evidence: docker/docker-compose.yml api service lacks restart policy; bootstrap throws after exhaustion (fail-fast is intentional).
- source_spec: `_bmad-output/implementation-artifacts/spec-fix-bootstrap-healthcheck-race.md`
  summary: Seeded plaintext admin password logged at WRN to Console JSON + Seq on every first boot (pre-existing); GenerateRandomString has modulo bias (pre-existing).
  evidence: PlatformAdminBootstrapService.cs LogWarning(Password) unchanged; chars.Length=70 not power-of-2 vs RandomNumberGenerator.GetBytes.
- source_spec: `_bmad-output/implementation-artifacts/spec-fix-system-audit-records.md`
  summary: Add an executable, cross-tenant AuditLog integration test that proves the tenant interceptor excludes foreign rows.
  evidence: The repository test asserts generated tenant predicates, but its fake client does not execute SQL; the Dockerized Testcontainers integration suite cannot start in the current Windows Docker-socket environment.
- source_spec: `_bmad-output/implementation-artifacts/spec-fix-system-audit-records.md`
  summary: Move AuditLog sorting and pagination back to database-side execution once tenant-filter-safe DBTools ordering is available.
  evidence: The current repair must sort and page mapped rows in memory because the interceptor appends predicates after DBTools `whereClause` text; a large audit ledger can therefore load more rows than the requested page size.
