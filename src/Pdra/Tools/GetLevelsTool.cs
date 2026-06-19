using Autodesk.Revit.DB;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PDRA.Services.Ai.Tools.Queries
{
    /// <summary>
    /// Lists every Level in the active document, sorted by elevation. Useful for
    /// building floor-by-floor filters before calling scope-box or room tools.
    /// </summary>
    public sealed class GetLevelsTool : IPdraTool
    {
        public string Name => "pdra_get_levels";
        public string Description =>
            "List all levels (storeys) in the active document, sorted by elevation. " +
            "Returns unique_id, id, name, elevation_ft (internal Revit units), and " +
            "elevation_user_units (document display units). Use these ids to drive " +
            "pdra_filter_elements_by_scope_box or pdra_get_rooms.";

        public Reversibility Reversibility => Reversibility.Reversible;
        public Verifiability Verifiability => Verifiability.Auto;

        public JsonNode InputSchema => JsonHelpers.EmptyObjectSchema();

        public ToolResult Run(ToolContext ctx, JsonElement args)
        {
            var doc = ctx.UiApp.ActiveUIDocument?.Document;
            if (doc is null) return ToolResult.Error("No active document.");

            var levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .ToList();

            var rows = new JsonArray();
            foreach (var lvl in levels)
            {
                rows.Add(new JsonObject
                {
                    ["unique_id"]            = lvl.UniqueId,
                    ["id"]                   = lvl.Id.Value,
                    ["name"]                 = lvl.Name,
                    ["elevation_ft"]         = lvl.Elevation,
                    ["elevation_user_units"] = lvl.get_Parameter(BuiltInParameter.LEVEL_ELEV)?.AsValueString(),
                });
            }

            return ToolResult.Ok(JsonHelpers.Serialize(new JsonObject
            {
                ["count"]  = rows.Count,
                ["levels"] = rows,
            }));
        }
    }
}
