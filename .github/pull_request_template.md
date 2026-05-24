<!--
Thanks for contributing to Postbode!
Please fill in the sections below. By submitting this PR you agree to the CLA (CLA.md).
-->

## Summary

<!-- What does this PR do, and why? Focus on the "why". -->

## Type of Change

- [ ] Fix (bug fix)
- [ ] Add (new feature)
- [ ] Refactor (no behavioral change)
- [ ] Update (dependency / version)
- [ ] Change (behavioral change)
- [ ] Remove (removing code/feature)
- [ ] Docs

## Linked Issue

<!-- e.g. Closes #123 -->

## Checklist

### Architecture
- [ ] Layer boundaries respected (no implementations in Application, no business logic in WebApp)
- [ ] New interfaces defined in Application; implementations in Infrastructure
- [ ] No direct Azure Service Bus SDK calls from services (must go through adapters)
- [ ] No infrastructure provisioning logic added (queue/topic creation belongs in Bicep)

### Code Quality
- [ ] Naming conventions followed (see CONTRIBUTING.md)
- [ ] Async methods suffixed with `Async`
- [ ] Private fields use `_camelCase`
- [ ] No unnecessary JavaScript added (Blazor Server preferred)
- [ ] Code style matches `.editorconfig`

### Functionality
- [ ] All message operations are audit-logged via `IAuditLogger`
- [ ] Long-running operations use the worker/dispatcher pattern (not inline)
- [ ] New adapters use `AdapterFactory` with the correct mode (`OnBehalfOf` vs `ServiceAccount`)
- [ ] `ApplicationContext` is properly set (CorrelationId, UserName, CurrentConnection)

### Security
- [ ] No secrets or credentials committed
- [ ] Authentication/authorization properly applied
- [ ] Token flow correct (On-Behalf-Of for user actions, Managed Identity for service actions)

### Testing
- [ ] `dotnet build` passes
- [ ] `dotnet test` passes
- [ ] New functionality has appropriate test coverage (functional tests via Aspire)

### General
- [ ] Commit messages follow project conventions
- [ ] No unrelated changes included
- [ ] I have read and agree to the [CLA](../CLA.md)
