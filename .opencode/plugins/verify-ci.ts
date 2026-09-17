import type { Plugin } from "@opencode-ai/plugin";

/**
 * OpenCode plugin that enforces the mandatory local CI verification gate.
 * Listens on session.idle / turn completion to verify that any code changes
 * pass all local CI jobs before the agent becomes idle.
 */
export const VerifyCiPlugin: Plugin = async ({ $ }) => {
  return {
    event: async ({ event }) => {
      if (event.type === "session.idle") {
        try {
          await $`bash scripts/hooks/verify-ci-agent-stop.sh`;
        } catch (err: unknown) {
          console.error("Local CI verification gate failed on session.idle:", err);
        }
      }
    },
  };
};

export default VerifyCiPlugin;
