
- source_spec: `_bmad-output/implementation-artifacts/spec-fix-bootstrap-healthcheck-race.md`
  summary: DemoSeederService ignores InsertAsync/UpdateAsync bool results throughout; partial demo seed failure is silent (less severe due to seed-version self-heal on next boot).
  evidence: DemoSeederService.cs SeedUsersAsync/InsertUserAsync/WriteSeedVersionAsync all discard bool returns; audit-only per spec.
- source_spec: `_bmad-output/implementation-artifacts/spec-fix-bootstrap-healthcheck-race.md`
  summary: API container has no restart policy; a DB outage longer than the ~46s retry window kills the stack until manual intervention.
  evidence: docker/docker-compose.yml api service lacks restart policy; bootstrap throws after exhaustion (fail-fast is intentional).
- source_spec: `_bmad-output/implementation-artifacts/spec-fix-bootstrap-healthcheck-race.md`
  summary: Seeded plaintext admin password logged at WRN to Console JSON + Seq on every first boot (pre-existing); GenerateRandomString has modulo bias (pre-existing).
  evidence: PlatformAdminBootstrapService.cs LogWarning(Password) unchanged; chars.Length=70 not power-of-2 vs RandomNumberGenerator.GetBytes.
