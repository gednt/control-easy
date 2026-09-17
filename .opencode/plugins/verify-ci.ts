import type { Plugin } from "@opencode-ai/plugin";

/**
 * OpenCode plugin that enforces the mandatory local CI verification gate.
 * Listens on session.idle to verify that any code changes pass all local
 * CI jobs before the agent becomes idle.
 *
 * Shell selection: per AGENTS.md Principle VIII ("Host-OS / Shell-Aware
 * Command Execution"), POSIX-only snippets (bash, sh) MUST NOT be invoked
 * from a Windows host. The canonical fix is the PowerShell wrapper under
 * scripts/hooks/. We detect the host platform and dispatch accordingly.
 */
export const VerifyCiPlugin: Plugin = async ({ $ }) => {
  return {
    event: async ({ event }) => {
      if (event.type === "session.idle") {
        const isWindows = process.platform === "win32";
        const hookScript = isWindows
          ? "scripts/hooks/verify-ci-agent-stop.ps1"
          : "scripts/hooks/verify-ci-agent-stop.sh";
        try {
          if (isWindows) {
            await $`powershell -NoProfile -ExecutionPolicy Bypass -File ${hookScript}`;
          } else {
            await $`bash ${hookScript}`;
          }
        } catch (err: unknown) {
          console.error("Local CI verification gate failed on session.idle:", err);
        }
      }
    },
  };
};

export default VerifyCiPlugin;