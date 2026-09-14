# Research: Dashboard Visits and Access Registration

## Decisions

### Reuse one visit-registration modal

**Decision**: Move the visit form into a reusable feature component and render it from both the dashboard and Visits page.

**Rationale**: The dashboard previously opened an unrelated access-entry workflow, which wrote incomplete visit-like data. One form ensures both entry points collect the receiving apartment.

**Alternatives considered**: Duplicating the form in the dashboard was rejected because validation and fields would drift.

### Keep resident and vehicle movement in the append-only access log

**Decision**: Add an explicit exit event to the existing tenant-scoped access audit log; entry continues to use its existing resident and vehicle categories.

**Rationale**: The log already retains timestamp, category, person/vehicle details, and the recording gatehouse profile, so it is the appropriate audited history without introducing a parallel record store.

**Alternatives considered**: Treating movements as Visits was rejected because a visit requires a destination apartment and has a separate lifecycle.

### Preserve historical visit compatibility

**Decision**: Require a destination apartment in the shared user interface while leaving the existing API capable of reading legacy records without an apartment.

**Rationale**: Existing visit records and integrations may omit the field, but new operator-created visits must include it.

**Alternatives considered**: Immediately making the database column non-null was rejected because it could invalidate existing operational data.
