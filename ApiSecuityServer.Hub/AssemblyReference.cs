using System.Reflection;

namespace ApiSecuityServer;

internal static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}