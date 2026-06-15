using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PDRA.Services.Json
{
    /// <summary>
    /// Shared, correctly-initialized <see cref="JsonSerializerOptions"/> instances.
    ///
    /// Why this exists: .NET 8 marks <see cref="JsonSerializerOptions"/> instances
    /// as read-only on first use. Reflection-based serialization (which the project
    /// relies on for plain POCOs) requires a <see cref="TypeInfoResolver"/> to be set
    /// before that lock-in happens. Constructing the options inline at each call site
    /// forgets the resolver, and sometimes the runtime throws
    ///   "JsonSerializerOptions instance must specify a TypeInfoResolver setting
    ///    before being marked as read-only."
    ///
    /// All call sites should use one of the cached instances below.
    /// </summary>
    public static class PdraJson
    {
        private static readonly DefaultJsonTypeInfoResolver Resolver = new();

        /// <summary>Compact (no whitespace), default property names. For tool result payloads.</summary>
        public static readonly JsonSerializerOptions Compact = new()
        {
            WriteIndented    = false,
            TypeInfoResolver = Resolver,
        };

        /// <summary>Indented, default property names. For human-readable file output.</summary>
        public static readonly JsonSerializerOptions Indented = new()
        {
            WriteIndented    = true,
            TypeInfoResolver = Resolver,
        };

        /// <summary>Indented + camelCase. For composite definitions and other camelCase-on-disk contracts.</summary>
        public static readonly JsonSerializerOptions IndentedCamelCase = new()
        {
            WriteIndented        = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver     = Resolver,
        };
    }
}
