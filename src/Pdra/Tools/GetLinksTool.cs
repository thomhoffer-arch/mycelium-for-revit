using Autodesk.Revit.DB;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PDRA.Services.Ai.Tools.Queries
{
    /// <summary>
    /// Lists all RevitLinkInstance elements in the active document, whether loaded
    /// or not. For loaded links, exposes the link document's projectKey — the same
    /// key pdra_get_element_by_uniqueid stamps on elements found inside that link,
    /// so callers can join them without guessing.
    /// </summary>
    public sealed class GetLinksTool : IPdraTool
    {
        public string Name => "pdra_get_links";
        public string Description =>
            "List all Revit links in the active document. For each link: unique_id, name, " +
            "loaded (bool). Loaded links also include title and project_key — the same " +
            "projectKey that pdra_get_element_by_uniqueid stamps on elements found inside " +
            "that link, so you can join cross-link element results without ambiguity.";

        public Reversibility Reversibility => Reversibility.Reversible;
        public Verifiability Verifiability => Verifiability.Auto;

        public JsonNode InputSchema => JsonHelpers.EmptyObjectSchema();

        public ToolResult Run(ToolContext ctx, JsonElement args)
        {
            var doc = ctx.UiApp.ActiveUIDocument?.Document;
            if (doc is null) return ToolResult.Error("No active document.");

            var links = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .OrderBy(l => l.Name)
                .ToList();

            var rows = new JsonArray();
            foreach (var link in links)
            {
                var linkDoc = link.GetLinkDocument();
                var loaded  = linkDoc != null;

                var row = new JsonObject
                {
                    ["unique_id"] = link.UniqueId,
                    ["id"]        = link.Id.Value,
                    ["name"]      = link.Name,
                    ["loaded"]    = loaded,
                };

                if (loaded)
                {
                    row["title"]       = linkDoc!.Title;
                    row["project_key"] = SpineKeys.ProjectKey(linkDoc!);
                }

                rows.Add(row);
            }

            return ToolResult.Ok(JsonHelpers.Serialize(new JsonObject
            {
                ["count"] = rows.Count,
                ["links"] = rows,
            }));
        }
    }
}
