using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PDRA.Services.Ai.Tools.Queries
{
    /// <summary>
    /// Lists all non-sheet views in the active document. Excludes view templates
    /// and ViewSheets (use pdra_get_sheets for those). Useful for finding the view
    /// whose id can be passed to pdra_filter_elements_by_scope_box.
    /// </summary>
    public sealed class GetViewsTool : IPdraTool
    {
        public string Name => "pdra_get_views";
        public string Description =>
            "List all non-sheet views in the active document (floor plans, sections, " +
            "elevations, 3D views, drafting views, etc.). Excludes view templates and " +
            "ViewSheets. Filter by view_type (e.g. FloorPlan, CeilingPlan, Section, " +
            "Elevation, ThreeD, DraftingView, Legend, Schedule). Plan views include a " +
            "level object when one is associated. Supports limit and fields.";

        public Reversibility Reversibility => Reversibility.Reversible;
        public Verifiability Verifiability => Verifiability.Auto;

        public JsonNode InputSchema => new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["view_type"] = new JsonObject
                {
                    ["type"]        = "string",
                    ["description"] = "Filter to a single ViewType by name: FloorPlan, CeilingPlan, " +
                                      "Elevation, Section, Detail, ThreeD, Schedule, DraftingView, " +
                                      "Legend, EngineeringPlan, AreaPlan. Omit for all.",
                },
                ["limit"]  = JsonHelpers.LimitSchemaProp(),
                ["fields"] = JsonHelpers.FieldsSchemaProp(),
            },
            ["additionalProperties"] = false,
        };

        public ToolResult Run(ToolContext ctx, JsonElement args)
        {
            var doc = ctx.UiApp.ActiveUIDocument?.Document;
            if (doc is null) return ToolResult.Error("No active document.");

            args.TryGetString("view_type", out var filterType);
            var limit  = args.GetLimit();
            var fields = args.GetFields();

            IEnumerable<View> query = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate && !(v is ViewSheet));

            if (!string.IsNullOrEmpty(filterType))
            {
                if (!Enum.TryParse<ViewType>(filterType, ignoreCase: true, out var vt))
                    return ToolResult.Error(
                        $"Unknown ViewType '{filterType}'. Valid values: FloorPlan, CeilingPlan, " +
                        "Elevation, Section, Detail, ThreeD, Schedule, DraftingView, Legend, " +
                        "EngineeringPlan, AreaPlan, Walkthrough, Rendering.");
                query = query.Where(v => v.ViewType == vt);
            }

            var all  = query.OrderBy(v => v.ViewType.ToString()).ThenBy(v => v.Name).ToList();
            var page = all.Take(limit).ToList();

            var rows = new JsonArray();
            foreach (var view in page)
            {
                var row = new JsonObject
                {
                    ["unique_id"] = view.UniqueId,
                    ["id"]        = view.Id.Value,
                    ["name"]      = view.Name,
                    ["view_type"] = view.ViewType.ToString(),
                };

                var level = ElementContextReader.ResolveLevel(view);
                if (level != null) row["level"] = level;

                rows.Add(JsonHelpers.Project(row, fields));
            }

            return ToolResult.Ok(JsonHelpers.Serialize(new JsonObject
            {
                ["total"]     = all.Count,
                ["count"]     = rows.Count,
                ["truncated"] = rows.Count < all.Count,
                ["views"]     = rows,
            }));
        }
    }
}
