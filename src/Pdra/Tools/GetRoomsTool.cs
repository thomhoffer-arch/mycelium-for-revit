using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PDRA.Services.Ai.Tools.Queries
{
    /// <summary>
    /// Enumerates placed rooms (Autodesk.Revit.DB.Architecture.Room) in the active
    /// document. Unplaced rooms (area == 0) are excluded unless include_unplaced=true.
    /// </summary>
    public sealed class GetRoomsTool : IPdraTool
    {
        public string Name => "pdra_get_rooms";
        public string Description =>
            "Enumerate rooms in the active document. Returns room number, name, level, " +
            "area_sf (square feet), area_user_units (display units), and unique_id. " +
            "Unplaced rooms (no bounding area) are filtered out by default — pass " +
            "include_unplaced=true to include them. Supports limit and fields filtering.";

        public Reversibility Reversibility => Reversibility.Reversible;
        public Verifiability Verifiability => Verifiability.Auto;

        public JsonNode InputSchema => new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["include_unplaced"] = new JsonObject
                {
                    ["type"]        = "boolean",
                    ["description"] = "Include rooms that are not yet bounded/placed (area == 0).",
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

            var inclUnplaced = args.ValueKind == JsonValueKind.Object
                && args.TryGetProperty("include_unplaced", out var iuProp)
                && iuProp.ValueKind == JsonValueKind.True;

            var limit  = args.GetLimit();
            var fields = args.GetFields();

            IEnumerable<Room> query = new FilteredElementCollector(doc)
                .OfClass(typeof(Room))
                .Cast<Room>();

            if (!inclUnplaced)
                query = query.Where(r => r.Area > 0);

            var all  = query.OrderBy(r => r.Number).ToList();
            var page = all.Take(limit).ToList();

            var rows = new JsonArray();
            foreach (var room in page)
            {
                var row = new JsonObject
                {
                    ["unique_id"] = room.UniqueId,
                    ["id"]        = room.Id.Value,
                    ["number"]    = room.Number,
                    ["name"]      = room.Name,
                    ["area_sf"]   = room.Area,
                };

                var areaStr = room.get_Parameter(BuiltInParameter.ROOM_AREA)?.AsValueString();
                if (!string.IsNullOrEmpty(areaStr)) row["area_user_units"] = areaStr;

                var level = ElementContextReader.ResolveLevel(room);
                if (level != null) row["level"] = level;

                rows.Add(JsonHelpers.Project(row, fields));
            }

            return ToolResult.Ok(JsonHelpers.Serialize(new JsonObject
            {
                ["total"]     = all.Count,
                ["count"]     = rows.Count,
                ["truncated"] = rows.Count < all.Count,
                ["rooms"]     = rows,
            }));
        }
    }
}
