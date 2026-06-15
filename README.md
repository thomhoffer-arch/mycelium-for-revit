# loam-revit-connector

A **Revit model source** for the [Loam](https://github.com/thomhoffer-arch/Loam) orchestrator. Exposes Revit data over **MCP** so Loam can build spine records — the connector itself does **not** emit spine records and does **not** maintain a provenance ledger.

The five Revit ops are **PDRA's implementations verbatim** (vendored under `src/Pdra/`). One contract, one implementation, two front-ends (PDRA and this connector).

Authoritative wire contract: [REVIT_MODEL_SOURCE_CONTRACT.md](https://github.com/thomhoffer-arch/loam/blob/main/docs/connectors/REVIT_MODEL_SOURCE_CONTRACT.md) (Loam) — `docs/CONTRACT.md` here is a local mirror. See `ROADMAP.md` for what's done / next / out-of-scope.

---

## Contract

Transport: **MCP over Streamable HTTP**, JSON-RPC (`initialize` → `tools/call`). Tool output is returned as a JSON string in `result.content[0].text`. Optional bearer auth via `Authorization: Bearer <token>`.

Defaults:

| env var | default |
|---|---|
| `LOAM_REVIT_LISTEN` | `http://127.0.0.1:47100/mcp` |
| `LOAM_REVIT_TOKEN`  | _unset = no auth_ |

The five tools — snake_case wire names, dispatched to PDRA's `IPdraTool` implementations:

1. `get_model_revision`
2. `filter_elements_by_scope_box`
3. `get_element_by_uniqueid`
4. `get_element_by_ifcguid`
5. `get_door_rooms`

PDRA names the tools `pdra_get_model_revision` etc. internally; the MCP listener accepts **both** forms but advertises the unprefixed contract names via `tools/list`.

---

## Build

Multi-targeted: `net48` for Revit 2024, `net8.0-windows` for Revit 2025/2026. From repo root:

```powershell
# Revit 2024 (.NET Framework 4.8)
dotnet build .\LoamRevitConnector.sln -c Release -f net48 -p:RevitVersion=2024

# Revit 2025 (.NET 8)
dotnet build .\LoamRevitConnector.sln -c Release -f net8.0-windows -p:RevitVersion=2025

# Revit 2026 (.NET 8)
dotnet build .\LoamRevitConnector.sln -c Release -f net8.0-windows -p:RevitVersion=2026
```

Override the API path if Revit isn't at the default location:
```powershell
-p:RevitApiDir="D:\Autodesk\Revit 2025"
```

Prereqs: .NET SDK 8+ with the **.NET Framework 4.8 Developer Pack** (for 2024 builds), and `RevitAPI.dll` / `RevitAPIUI.dll` present in the per-version Revit install dir (they ship with Revit; not redistributable).

## Install

Copy the build output into the version-matching add-in folder:

```powershell
$year = "2025"  # or 2024 / 2026
$tfm  = if ($year -eq "2024") { "net48" } else { "net8.0-windows" }
$dst  = "$env:APPDATA\Autodesk\Revit\Addins\$year"
New-Item -ItemType Directory -Force $dst | Out-Null
Copy-Item -Recurse -Force .\src\bin\Release\$tfm\* $dst
```

Launch Revit; the MCP server starts on port 47100. Point Loam at it:

```
LOAM_MODEL_SOURCE=revit-connector
LOAM_REVIT_URL=http://127.0.0.1:47100/mcp
```

---

## Repo layout

```
src/
  App.cs                            # IExternalApplication entry — boots MCP server
  Mcp/
    McpServer.cs                    # HttpListener + JSON-RPC dispatcher → IPdraTool
  RevitBridge/
    RevitContext.cs                 # ExternalEvent marshalling to UI thread
  Pdra/                             # PDRA main @ 9df8a97 — VERBATIM, do not fork
    IPdraTool.cs / ToolMetadata.cs / PdraJson.cs / JsonHelpers.cs
    SpineKeys.cs / ElementContextReader.cs
    Tools/
      GetModelRevisionTool.cs
      FilterElementsByScopeBoxTool.cs
      GetElementByUniqueIdTool.cs
      GetElementByIfcGuidTool.cs
      GetDoorRoomsTool.cs
  LoamRevitConnector.addin
  LoamRevitConnector.csproj
docs/
  CONTRACT.md                       # field-level Loam contract reference (mirror)
ROADMAP.md
```

Files under `src/Pdra/` are PDRA `main` verbatim — bug fixes go upstream to PDRA, then re-vendor here. Don't fork in-tree.

---

## See also

- [Loam (orchestrator)](https://github.com/thomhoffer-arch/Loam)
- [Mycelium (Connective Spine)](https://github.com/thomhoffer-arch/Mycelium)
- [PDRA (Revit MCP tools — source of `src/Pdra/`)](https://github.com/thomhoffer-arch/PDRA)
- [Full contract spec (REVIT_MODEL_SOURCE_CONTRACT.md)](https://github.com/thomhoffer-arch/loam/blob/main/docs/connectors/REVIT_MODEL_SOURCE_CONTRACT.md)
