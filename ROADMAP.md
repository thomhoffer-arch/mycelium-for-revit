# loam-revit-connector — Roadmap

A Revit **model source** for the Loam orchestrator. It exposes Revit data over MCP tools that
satisfy the [Revit model-source contract](https://github.com/thomhoffer-arch/loam) (canonical:
`docs/connectors/REVIT_MODEL_SOURCE_CONTRACT.md` in `loam`). It is interchangeable with PDRA — two
implementations, one contract.

> **Role boundary (never crosses this line).** The connector exposes *raw Revit data via MCP tools*.
> It does **not** build spine records, run a provenance ledger, or carry triage/compliance logic —
> **Loam** does all of that. Every item below keeps the connector a thin translator.

## Status — done (v0.2)

The five read primitives Loam calls today are implemented, contract-correct, and now share their
**exact** implementation with PDRA (the five files under `src/Pdra/Tools/` are PDRA `main` verbatim;
the connector wraps them as MCP tools without re-deriving any Revit logic):

- [x] `get_model_revision`
- [x] `filter_elements_by_scope_box`
- [x] `get_element_by_uniqueid`
- [x] `get_element_by_ifcguid`
- [x] `get_door_rooms`
- [x] MCP-over-HTTP server (initialize / tools/list / tools/call), bearer auth, `content[0].text`
      JSON framing
- [x] Multi-targeted: `net48` (Revit 2024) and `net8.0-windows` (Revit 2025/2026)

This is feature-complete for everything Loam does **today** (freshness, zone resolution,
classification/finance enrichment, door compliance, deleted-vs-fixed).

## Near-term — polish (no contract change)

- [ ] **BUILD.md** — prereqs (installed Revit for the API DLLs, matching .NET: net48 for 2024,
      net8.0-windows for 2025/2026), build (`dotnet build -c Release -f <tfm> -p:RevitVersion=<year>`),
      install (copy DLL + `.addin` to `%APPDATA%\Autodesk\Revit\Addins\<year>\`), run, and pointing
      Loam at it (`LOAM_MODEL_SOURCE=revit-connector`).
- [ ] **Self-test script** — call each tool against a sample model and check the response shape
      against `docs/CONTRACT.md`, so field-name drift fails loudly instead of silently breaking
      Loam ingestion.

## Robustness — accuracy of the existing primitives

- [ ] **Batching / performance** — `get_element_by_ifcguid` scans all elements per call; cache the
      `IFC_GUID → element` index per document revision for large models.
- [ ] **Linked-model elements** stay flagged `from_link: true` and are never silently merged into the
      host set (Loam drops them; keep that guarantee).
- [ ] **Error semantics** — structured JSON error payload (not just HTTP 500) so Loam can surface a
      clean per-tool warning.

## Profiles — beyond `nl`

> PDRA-style tools take field-name args at call time (`door_params`, `room_params`) rather than
> reading a profile JSON. Profile config — if needed at all — lives Loam-side now.

- [ ] If a firm/standard needs different default door/room params or BIP fallback chains, surface
      them via tool args from Loam, not connector-side config.

## Write-back — FUTURE (additive, gated)

Loam declares `edit_element` / `create_workitem` in its propose→approve layer but does **not**
execute them against Revit yet. When Loam wires Revit write-back, the connector gains the matching
write primitives — **additively** (contract semver: additive → minor bump):

- [ ] `edit_element` — set parameter(s) on an element. Executes the Revit transaction; returns the
      new state. **Loam** owns propose → human-approve → ledger; the connector performs the approved
      op and reports the result.
- [ ] `create_workitem` — create the Revit-side artefact for a coordination action.
- [ ] **Reversible / transactional** — wrap writes in a named transaction group so a change can be
      rolled back; surface a transaction id Loam can record.

Do **not** add write tools until Loam calls them — they stay unbuilt to keep the read surface the
only attack surface.

## Packaging

- [ ] **GitHub release** with a prebuilt DLL (+ `.addin`) per Revit version, so users install
      without compiling. (Optional — only when distributing beyond your own machine.)

## Non-goals (keep these in Loam, never here)

- Spine records (identity / freshness / provenance) — **Loam** constructs them from these fields.
- The provenance ledger — **Loam** owns it.
- Triage, compliance verdicts, € exposure, profiles' *rule* logic — **Loam**.
- Any classification *crosswalk* / accumulated judgment — that's Loam's private moat.
