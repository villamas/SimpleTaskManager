# Codex Instructions — Simple Task Manager

## Source of truth

Read and follow `docs/requirements.md` and `docs/technical-design.md` before changing the application. They are the approved baseline. Do not redesign the application, repeat requirements analysis, or introduce requirements or technical decisions.

## Development principles

- Keep implementation proportional to a basic POC.
- Follow KISS and YAGNI.
- Use only the approved technology stack and architecture.
- Keep the application and database entirely local.
- Do not add features, dependencies, abstractions, or architectural complexity.
- Use English for code, documentation, comments, and UI text.

## Repository boundaries

- Preserve the existing `feature/createSimpleTask` branch.
- Keep the Assessment folder excluded from Git.
- Preserve unrelated existing changes.
- Do not automatically commit, push, merge, or switch branches.

## Testing and validation

- Limit automated testing to the eight approved integration scenarios IT-001 through IT-008.
- Do not create an extensive unit-test suite or impose a mandatory coverage target.
- Build and validate the implementation before reporting completion.
- Use Playwright MCP only during browser validation, if available.
- If MCP is unavailable, validate manually and describe that accurately.
- Use synthetic data only.

## Change control and reporting

- Request human approval before changing approved scope or technical design.
- Report actual results, issues, limitations, and deviations; do not infer success from intended steps.
- Report only genuine blockers or decisions requiring human attention.
