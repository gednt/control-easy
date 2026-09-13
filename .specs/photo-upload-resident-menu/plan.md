# Implementation Plan: Photo Upload Reliability and Resident Actions

1. Add tenant-scoped photo entity metadata and list support to the Photos module, preserving the existing authenticated content endpoint.
2. Send entity metadata in multipart uploads, render protected previews through authenticated blob retrieval, and replace generic upload failure handling with ProblemDetails-aware messages.
3. Harden the shared dropdown's viewport placement, stacking, keyboard behavior, and mobile sizing; apply the resident-specific trigger treatment.
4. Cover request metadata, upload error display, binding reload, capture conversion, and resident menu placement with unit and browser tests.
