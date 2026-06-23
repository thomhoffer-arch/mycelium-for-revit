using System.Text.Json;
using System.Text.Json.Nodes;

namespace PDRA.Services.Ai.Tools.Queries
{
    /// <summary>
    /// Returns the active document's project identity from
    /// <see cref="Autodesk.Revit.DB.Document.ProjectInformation"/> so an external
    /// tool can attribute and join data to the right project without any manual seeding.
    /// <c>name</c> and <c>number</c> are the join keys; <c>client</c>/<c>address</c>/
    /// <c>building</c> are optional context. <c>title</c> and <c>path</c> come from the
    /// document itself — always populated, so a blank-ProjectInformation model still
    /// self-identifies by its .rvt filename.
    /// </summary>
    public sealed class GetProjectInfoTool : IPdraTool
    {
        public string Name        => "pdra_get_project_info";
        public string Description =>
            "Returns the active Revit document's project identity. Includes name and number from " +
            "Document.ProjectInformation (required join keys), plus optional client, address and building. " +
            "Also returns title (Document.Title) and path (Document.PathName) — always populated — so a " +
            "model with blank ProjectInformation still self-identifies by its .rvt filename. " +
            "name falls back to title when ProjectInformation.Name is empty.";

        public Reversibility Reversibility => Reversibility.Reversible;
        public Verifiability Verifiability => Verifiability.Auto;

        public JsonNode InputSchema => JsonHelpers.EmptyObjectSchema();

        public ToolResult Run(ToolContext ctx, JsonElement args)
        {
            var doc = ctx.UiApp.ActiveUIDocument?.Document;
            if (doc is null) return ToolResult.Error("No active document.");

            var pi    = doc.ProjectInformation;
            var title = doc.Title    ?? "";
            var path  = doc.PathName ?? "";
            var name  = (pi?.Name ?? "").Length > 0 ? pi!.Name : title;

            return ToolResult.Ok(JsonHelpers.Serialize(new JsonObject
            {
                ["name"]     = name,
                ["number"]   = pi?.Number       ?? "",
                ["client"]   = pi?.ClientName   ?? "",
                ["address"]  = pi?.Address      ?? "",
                ["building"] = pi?.BuildingName ?? "",
                ["title"]    = title,
                ["path"]     = path,
            }));
        }
    }
}
