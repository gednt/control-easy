# Test runner image for Phase 13 (and beyond).
# Installs Node + Chrome so Karma can launch ChromeHeadless from inside
# the test container. Used by `make test` and the per-task verification
# loop. Not part of the runtime stack.
FROM mcr.microsoft.com/playwright:v1.49.0-jammy

ARG NODE_VERSION=20
ENV NODE_VERSION=${NODE_VERSION}

# Install Node 20 (NodeSource) for Angular's ng build + karma.
RUN apt-get update && apt-get install -y curl gnupg ca-certificates \
    && curl -fsSL https://deb.nodesource.com/setup_${NODE_VERSION}.x | bash - \
    && apt-get install -y nodejs \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# The host project must bind-mount /app and provide /app/node_modules.
# Example: docker run --rm -v $(pwd)/src/Web/ControlEasyReborn.Web:/app \
#                          -v $(pwd)/src/Web/ControlEasyReborn.Web/node_modules:/app/node_modules \
#                          ce-feat-planning-reconcile-v2-web-test:latest \
#                          sh -c "npx ng test --no-watch --browsers=ChromeHeadlessNoSandbox"