# .devcontainer/Dockerfile
#
# ControlEasy Reborn devcontainer.
#
# The Dockerfile installs everything repo-specific in one place so the build
# is self-contained (no ordering dependency on devcontainer features). The
# devcontainer.json still uses the dotnet and docker-outside-of-docker
# features for those two toolchains (they handle PATH, completions, and
# docker socket wiring reliably across OSes); node is installed here so
# `npm install -g @angular/cli` and `playwright install` work during build.

#hadolint shell=sh
FROM mcr.microsoft.com/devcontainers/base:debian-12

# Pin tool versions for reproducibility
ARG NODE_VERSION=20
ARG SHELLCHECK_VERSION=0.10.0
ARG HADOLINT_VERSION=2.12.0
ARG PLAYWRIGHT_VERSION=1.49

USER root

# ---- Node.js (NodeSource apt feed) -------------------------------------------------
# Installed here (not via the node feature) so `npm install -g @angular/cli`
# and `playwright install` can run in subsequent RUN layers during the build.
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl gnupg \
    && install -d /etc/apt/keyrings \
    && curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key \
        | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg \
    && echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_${NODE_VERSION}.x nodistro main" \
        > /etc/apt/sources.list.d/nodesource.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends nodejs \
    && rm -rf /var/lib/apt/lists/*

# ---- MySQL client + misc tools -----------------------------------------------------
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        default-mysql-client \
        bzip2 \
        xz-utils \
    && rm -rf /var/lib/apt/lists/*

# ---- shellcheck and hadolint from GitHub releases --------------------------------
# Pinned to specific versions for reproducibility.
RUN case "$(dpkg --print-architecture)" in \
        amd64)  SHELLCHECK_ARCH="x86_64";  HADOLINT_ARCH="x86_64"  ;; \
        arm64)  SHELLCHECK_ARCH="aarch64"; HADOLINT_ARCH="arm64"  ;; \
        *) echo "Unsupported architecture: $(dpkg --print-architecture)" >&2; exit 1 ;; \
    esac \
    && curl -fsSL "https://github.com/koalaman/shellcheck/releases/download/v${SHELLCHECK_VERSION}/shellcheck-v${SHELLCHECK_VERSION}.linux.${SHELLCHECK_ARCH}.tar.xz" \
        | tar -xJ --strip-components=1 -C /usr/local/bin "shellcheck-v${SHELLCHECK_VERSION}/shellcheck" \
    && curl -fsSL "https://github.com/hadolint/hadolint/releases/download/v${HADOLINT_VERSION}/hadolint-Linux-${HADOLINT_ARCH}" \
        -o /usr/local/bin/hadolint \
    && chmod +x /usr/local/bin/hadolint

# ---- Angular CLI + Playwright browsers ---------------------------------------------
# Playwright --with-deps installs OS libraries (libnss3, libatk, etc.) that
# chromium needs; that requires apt-get, so this layer runs as root.
RUN npm install -g @angular/cli@18 \
    && npx --yes playwright@${PLAYWRIGHT_VERSION} install --with-deps chromium

# ---- .NET environment variables ----------------------------------------------------
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_GENERATE_ASPNET_CERT=0 \
    DOTNET_USE_POLLING_FILE_WATCHER=1

USER vscode