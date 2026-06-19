using Autodesk.Revit.UI;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PDRA.Services.Ai.Tools
{
    /// <summary>
    /// A single Revit operation Claude can invoke. Implementations always run
    /// on the Revit UI thread (via <see cref="RevitEventDispatcher"/>); they
    /// may touch the Revit API freely.
    /// </summary>
    public interface IPdraTool
    {
        string Name { get; }
        string Description { get; }

        /// <summary>JSON Schema describing the tool's input arguments
        /// (the value of <c>input_schema</c> in Anthropic's tool definition).</summary>
        JsonNode InputSchema { get; }

        ToolResult Run(ToolContext ctx, JsonElement args);

        /// <summary>How recoverable this tool's effect is. Read-only queries return
        /// <see cref="Reversibility.Reversible"/>; document writes return ModelOnly;
        /// file exports return <see cref="Reversibility.External"/>.</summary>
        Reversibility Reversibility { get; }

        /// <summary>How this tool's outcome can be verified. Most tools return Auto
        /// (manifest self-check). View-affecting tools return Render; tools with
        /// subjective outcomes return Human.</summary>
        Verifiability Verifiability { get; }
    }

    public sealed record ToolContext(UIApplication UiApp);

    public sealed record ToolResult(bool IsError, string Text)
    {
        public static ToolResult Ok(string text)    => new(false, text);
        public static ToolResult Error(string text) => new(true,  text);
    }
}
