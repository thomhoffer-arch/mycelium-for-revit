# loam-revit-connector — Revit Model Source for the Connective Spine

This repository will implement a **model source** for Autodesk Revit, exposing Revit data via **MCP tools** to satisfy the [Connective Spine contract](https://github.com/thomhoffer-arch/Mycelium) (canonical in Mycelium).

---

## Status: Early Stage
- **1 commit**: Contract defined, implementation pending.
- **Target**: Expose the following 5 primitives (aligned with PDRA):
  1. `get_model_revision` — Returns the current Revit model revision.
  2. `filter_elements_by_scope_box` — Filters Revit elements by a scope box.
  3. `get_element_by_uniqueid` — Retrieves an element by Revit UniqueId *(singular)*.
  4. `get_element_by_ifcguid` — Retrieves an element by IFC GUID *(singular)*.
  5. `get_door_rooms` — Retrieves door-room relationships.

> **Note**: Tool names are **snake_case** (wire format), not camelCase. The camelCase names are Loam's internal method names and are not used on the wire.

---

## How It Works
- **Does not emit spine records**: This connector is a **model source**. It exposes raw Revit data via MCP tools.
- **Spine records are constructed by Loam**: The [Loam orchestrator](https://github.com/thomhoffer-arch/Loam) consumes these tools, constructs **spine records** (identity + freshness + provenance event), and maintains the **provenance ledger**.

---

## Contract
The full **Revit Model Source Contract** (including request/response JSON shapes, endpoint, and field names) is defined in:
👉 [Loam: `docs/connectors/REVIT_MODEL_SOURCE_CONTRACT.md`](https://github.com/thomhoffer-arch/Loam/blob/main/docs/connectors/REVIT_MODEL_SOURCE_CONTRACT.md)

---

## Compatibility
- **Spine Version**: `v0.1` (see [Mycelium](https://github.com/thomhoffer-arch/Mycelium)).
- **Orchestrator**: Designed for [Loam](https://github.com/thomhoffer-arch/Loam).
- **Endpoint**: `http://127.0.0.1:47100/mcp` (optional bearer auth).

---

## See Also
- [Mycelium (Connective Spine)](https://github.com/thomhoffer-arch/Mycelium)
- [Loam (Orchestrator)](https://github.com/thomhoffer-arch/Loam)
- [PDRA (Revit MCP Tools)](https://github.com/thomhoffer-arch/PDRA)