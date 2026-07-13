# Quickstart: Validate CI API Readiness

1. Confirm both jobs use a 180-second deadline and accept 200/503.
2. Confirm readiness exports `API_BASE_URL` and `E2E_BASE_URL`.
3. Simulate a containerized runner, attach it to the Compose network, and verify it resolves `api:8080`.
4. Confirm host-runner fallback still uses the dynamically published port.
5. Syntax-check the extracted Bash blocks and assert expected workflow structure.
6. In the devcontainer, rebuild API/web and run Unit, Integration, and Architecture tests.
7. Confirm cleanup disconnects the runner and remains guarded by `if: always()`.

Expected: containerized runners use Compose DNS, host runners use dynamic published ports, and a real timeout prints logs.
