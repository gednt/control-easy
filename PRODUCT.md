# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

ControlEasy Reborn serves condominium administrators, gatehouse attendants (portaria staff), and residents. Administrators manage condominium operations and records; attendants run day-to-day access operations; residents participate in the access and resident-management workflows relevant to them.

## Product Purpose

ControlEasy Reborn is a multi-tenant platform for complete condominium management. It brings access-control and operational management workflows into one system for condominium teams and residents.

## Positioning

Complete condominium management: ControlEasy combines gatehouse access operations with the wider set of condominium-management records and workflows, rather than treating visitor entry as an isolated process.

## Operating Context

The product is used in condominium administration and gatehouse environments. Core workflows include managing residents, apartments, visitors, vehicles, service providers, photographs and consent policies, gatehouse operations, security users and roles, tenant administration, and operational reports.

## Capabilities and Constraints

- Multi-tenant condominium platform with strict tenant isolation.
- Role- and permission-based access for platform administrators, tenant administrators, gatehouse attendants, and residents.
- Web application built as an Angular SPA backed by an ASP.NET Core minimal API and MySQL.
- Docker Compose is the supported development and deployment workflow.

## Evidence on Hand

- Product and operational documentation: `README.md` and `docs/`.
- Architecture documentation: `docs/ARCHITECTURE.md`.
- A demo Compose overlay and pre-populated sample scenarios: `docker/docker-compose.demo.yml` and `docs/demo-mode.md`.
- No external brand, legal, accessibility, testimonial, customer, benchmark, pricing, or press assets were confirmed for future work.

## Product Principles

1. Support the full condominium-management lifecycle, not isolated access-control moments.
2. Make role-specific operations clear for administrators, gatehouse attendants, and residents.
3. Preserve tenant isolation and permission boundaries in every workflow.
4. Keep operational records dependable, traceable, and practical in gatehouse and administrative settings.
