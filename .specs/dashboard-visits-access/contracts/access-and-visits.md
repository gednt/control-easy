# Access and Visit Contract

## Visit registration

Both the dashboard and Visits page use the same visit form. New form submissions include a non-empty apartment selection along with visitor name and document.

## Access audit event

The existing access-log create operation accepts a new event state, `exited`.

| State | Permitted categories | Photo required |
| --- | --- | --- |
| `exited` | `dweller`, `vehicle` | No |

The operation rejects `exited` for visitor and service-provider categories with a validation response.
