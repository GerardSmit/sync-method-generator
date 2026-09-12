namespace Zomp.SyncMethodGenerator.Helpers;

internal static class CompilationExtensions
{
    public static (ISet<string> EnumerableMembers, ISet<string> QueryableMembers) GetLinqMembers(this Compilation compilation)
    {
        INamedTypeSymbol? linqEnumerable = null;
        INamedTypeSymbol? linqQueryable = null;
        foreach (var reference in compilation.References)
        {
            var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
            linqEnumerable ??= assemblySymbol?.GetTypeByMetadataName("System.Linq.Enumerable");
            linqQueryable ??= assemblySymbol?.GetTypeByMetadataName("System.Linq.Queryable");
            if (linqEnumerable != null && linqQueryable != null)
            {
                break;
            }
        }

        return (new HashSet<string>(linqEnumerable?.MemberNames ?? []), new HashSet<string>(linqQueryable?.MemberNames ?? []));
    }
}
