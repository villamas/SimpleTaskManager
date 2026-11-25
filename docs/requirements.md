# Simple Task Manager — Requirements

**Version:** 0.2 — Approved

## Scope

Build a simple localhost web application for one user to manage daily tasks. It covers CRUD, status management, filtering, and local SQLite persistence. Use synthetic data only. No authentication, authorization, cloud infrastructure, external runtime services, or internet access after initial setup. Follow KISS and YAGNI.

## Functional requirements

| ID | Requirement |
|---|---|
| FR-001 | Create a task with a valid title and default Pending status. |
| FR-002 | View all stored tasks in oldest-first creation order. |
| FR-003 | Modify the title of an existing task regardless of status. |
| FR-004 | Permanently delete an existing task after browser confirmation. |
| FR-005 | Change status between Pending and Completed. |
| FR-006 | Filter by All, Pending, or Completed; All is selected on open. |
| FR-007 | Persist tasks and changes in local SQLite across restarts. |

Each task has only a unique automatic ID, a required trimmed title of 1–255 characters, and a Pending or Completed status.

## Non-functional requirements

| ID | Requirement |
|---|---|
| NFR-001 | Operate entirely on localhost. |
| NFR-002 | Work without internet or external runtime services after setup. |
| NFR-003 | Remain basically responsive during normal local use. |
| NFR-004 | Make all task operations available through the web interface. |
| NFR-005 | Preserve saved data after normal restart. |
| NFR-006 | Handle invalid input and nonexistent tasks with understandable errors without crashing. |
| NFR-007 | Follow KISS/YAGNI; avoid unnecessary dependencies and abstractions. |
| NFR-008 | Do not expose services to external network interfaces. |
| NFR-009 | Use synthetic data in development, demonstrations, and testing. |
| NFR-010 | Support Microsoft Edge or Google Chrome; testing either is sufficient. |

## Business rules

| ID | Rule |
|---|---|
| BR-001 | Every task has a unique, automatically assigned identifier. |
| BR-002–BR-003 | Titles are non-empty and 1–255 characters after trimming. |
| BR-004 | Status is exactly Pending or Completed. |
| BR-005–BR-006 | New tasks are Pending; both status transitions are reversible. |
| BR-007 | Duplicate titles are permitted. |
| BR-008 | Native browser confirmation is required; confirmation permanently deletes, cancellation changes nothing. |
| BR-009 | Filtering does not modify stored data. |
| BR-010–BR-011 | Editing changes only the title and is allowed for both statuses. |
| BR-012–BR-013 | Lists are oldest-first and All is the default filter. |
| BR-014 | Nonexistent-task operations do not modify other tasks. |

## Acceptance criteria

**Functional:** AC-001 valid creation saves one unique-ID Pending task; AC-002 rejects empty/whitespace titles; AC-003 displays all tasks; AC-004 displays an empty state; AC-005 valid editing changes title without ID/status changes; AC-006 invalid editing preserves the original; AC-007 confirmed deletion removes the task permanently; AC-008–AC-009 support both status transitions; AC-010–AC-012 correctly filter Pending, Completed, and All; AC-013 preserves all saved changes across restart; AC-014 rejects titles over 255 characters after trimming; AC-023 preserves a task after cancelled deletion; AC-024 allows duplicate titles with different IDs; AC-025 uses oldest-first order; AC-026 defaults to All; AC-027 edits Completed tasks without changing status; AC-028 handles nonexistent IDs without affecting other tasks; AC-029 trims surrounding whitespace; AC-030 accepts exactly 1 and 255 characters; AC-031 preserves data and order when switching filters.

**Non-functional:** AC-015 verifies localhost access and loopback-only exposure; AC-016 verifies local runtime resources and offline operation after setup; AC-017 verifies basic responsiveness; AC-018 verifies all core operations through the UI; AC-019 verifies restart persistence; AC-020 verifies understandable errors without crashes or unrelated changes; AC-021 verifies no unnecessary features/dependencies; AC-022 verifies synthetic data; AC-032 verifies functional tests in Edge or Chrome. UI validation (AC-018) is separate from persistence validation (AC-013 and AC-019). All 32 criteria must be verified, without requiring one automated test per criterion.

## Approved assumptions and resolutions

AS-001–AS-006: one user, local desktop browser, same-machine data, small volume, internet may be needed only for initial installation, and no migration/integration is required. OQ-001–OQ-012 resolve: ID/title/status only; mandatory trimmed 1–255-character title; Pending default; duplicate titles; native confirmation; permanent deletion; oldest-first order; All default; Completed remains editable; Edge or Chrome; qualitative responsiveness; no extra attributes.

## Out of scope

Authentication, authorization, multiple users, roles, sharing, collaboration, cloud deployment, synchronization, external APIs/integrations, extra task attributes, search, advanced filtering, custom sorting, pagination, undo/recovery, import/export, backups, reporting, analytics, dashboards, mobile apps, performance/load/cross-browser testing, database-failure simulation, production scalability, and high availability.

## Change control

HA-001 through HA-010 approve the complete baseline. Version 0.2 is authoritative; no requirements decisions remain open. Changes require explicit human approval.
