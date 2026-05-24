# Security Policy

We take the security of Postbode seriously. Thank you for helping keep the project and its users safe.

## Reporting a Vulnerability

**Please do not report security vulnerabilities through public GitHub issues, discussions, or pull requests.**

Instead, report them through one of the following private channels:

1. **GitHub Security Advisories** (preferred) — open a private advisory at:
   https://github.com/Droomploeg/Postbode/security/advisories/new
2. **Email** — send details to **mark@droomploeg.nl**

Please include the following information (as much as you can provide):

- Type of issue (e.g. authentication bypass, token leak, injection, etc.)
- Affected component(s) (file paths, project, page)
- Steps to reproduce
- Proof-of-concept or exploit code (if available)
- Impact assessment — what an attacker could achieve

This information helps us triage the report quickly.

## Response Process

- We aim to acknowledge receipt within **3 business days**.
- We will provide an initial assessment and expected timeline within **7 business days**.
- We will keep you informed of progress until the issue is resolved.
- Once a fix is released, we will publicly credit the reporter (unless you prefer to remain anonymous).

## Supported Versions

Postbode is currently in pre-1.0 development. Security fixes are applied to the **`main` branch** and the **latest release tag** only.

| Version | Supported |
|---------|-----------|
| `main` (development) | ✅ |
| Latest release tag | ✅ |
| Older release tags | ❌ |

## Scope

In scope for security reports:

- The Postbode web application (Blazor Server)
- The audit-logging pipeline
- Authentication and authorization flows (Entra ID, On-Behalf-Of, Managed Identity)
- Token handling and storage
- Bicep IaC templates that ship with the repository

Out of scope:

- Vulnerabilities in upstream dependencies (please report those upstream — we will pick up patched versions promptly)
- Misconfiguration of a self-hosted deployment that is not caused by the project's defaults
- Denial-of-service via resource exhaustion against a self-hosted instance

## Disclosure Policy

We follow **coordinated disclosure**. Please give us reasonable time to investigate and patch before any public disclosure.
