# Quickstart: Validate CI API Readiness

1. Confirm both jobs use a 180-second deadline and accept 200/503.
2. Confirm readiness exports `API_BASE_URL` and `E2E_BASE_URL`.
3. Syntax-check the extracted Bash blocks and assert expected workflow structure.
4. In the devcontainer, rebuild API/web and run Unit, Integration, and Architecture tests.
5. Confirm cleanup remains guarded by `if: always()`.

Expected: startup after 60 seconds passes, consumers receive numeric dynamic-port URLs, and a real timeout prints logs.

