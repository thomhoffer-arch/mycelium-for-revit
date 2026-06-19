// Required for C# record types when targeting .NET Framework 4.8, which does not
// ship System.Runtime.CompilerServices.IsExternalInit. The compiler emits a
// reference to this type for init-only setters; providing it here satisfies that.
#if NETFRAMEWORK
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif
