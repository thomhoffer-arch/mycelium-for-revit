namespace PDRA.Services.Ai.Tools
{
    /// <summary>
    /// How recoverable an action is. Derived from Tomašev et al. §2.2.3
    /// ("Reversibility" as a first-class task characteristic).
    /// <list type="bullet">
    /// <item><description><b>Reversible</b> — pure query / no side effect.</description></item>
    /// <item><description><b>ModelOnly</b> — changes the Revit document; Ctrl+Z works
    /// but state is still mutated.</description></item>
    /// <item><description><b>External</b> — writes to disk, exports a file, calls out;
    /// Revit undo doesn't reach.</description></item>
    /// </list>
    /// </summary>
    public enum Reversibility
    {
        Reversible,
        ModelOnly,
        External,
    }

    /// <summary>
    /// How an outcome can be checked.
    /// <list type="bullet">
    /// <item><description><b>Auto</b> — manifest + post-condition self-check is sufficient.</description></item>
    /// <item><description><b>Render</b> — correctness only visible by exporting the view
    /// (pdra_export_view_image).</description></item>
    /// <item><description><b>Human</b> — subjective: naming, layout aesthetics — requires
    /// human judgement.</description></item>
    /// </list>
    /// </summary>
    public enum Verifiability
    {
        Auto,
        Render,
        Human,
    }
}
