---
description: Run the mandatory local CI verification gate (all 6 stages: format, build, unit + arch tests, integration tests, web build, OpenAPI drift check).
---

Run the local CI verification script to ensure all jobs pass before marking any work done:

```bash
scripts/verify-ci-local.sh
```
