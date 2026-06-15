using Autodesk.Revit.DB;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PDRA.Services.Ai.Tools.Queries
{
    /// <summary>
    /// Returns a revision/freshness stamp for the active document so an external
    /// tool (e.g. ClashControl) can tell whether it is in sync with the live model.
    /// </summary>
    public sealed class GetModelRevisionTool : IPdraTool
    {
        public string Name        => "pdra_get_model_revision";
        public string Description =>
            "Returns a freshness stamp for the active Revit document: version_guid + number_of_saves " +
            "(from Document.GetDocumentVersion — changes on each save), has_unsaved_changes, title and " +
            "path. Use this to check whether another live tool (e.g. ClashControl) is talking about the " +
            "same model state before joining their data. Note: version_guid only advances on save, so " +
            "has_unsaved_changes flags in-session edits that the guid does not yet reflect.";

        public Reversibility Reversibility => Reversibility.Reversible;

        public JsonNode InputSchema => JsonHelpers.EmptyObjectSchema();

        public ToolResult Run(ToolContext ctx, JsonElement args)
        {
            var doc = ctx.UiApp.ActiveUIDocument?.Document;
            if (doc is null) return ToolResult.Error("No active document.");

            var v = Document.GetDocumentVersion(doc);

            return ToolResult.Ok(JsonHelpers.Serialize(new JsonObject
            {
                ["title"]               = doc.Title,
                ["path"]                = string.IsNullOrEmpty(doc.PathName) ? null : doc.PathName,
                ["version_guid"]        = v?.VersionGUID.ToString(),
                ["number_of_saves"]     = v?.NumberOfSaves,
                ["has_unsaved_changes"] = doc.IsModified,
                ["is_workshared"]       = doc.IsWorkshared,
            }));
        }
    }
}
