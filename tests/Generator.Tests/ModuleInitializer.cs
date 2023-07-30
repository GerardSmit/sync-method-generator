﻿namespace Generator.Tests;

public static class ModuleInitializer
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void Initialize()
    {
        VerifySourceGenerators.Initialize();
        VerifierSettings.RegisterFileConverter<RunResultWithIgnoreList>(Convert);
    }

    // From https://github.com/VerifyTests/Verify.SourceGenerators/issues/67#issuecomment-1536710180
    private static ConversionResult Convert(RunResultWithIgnoreList target, IReadOnlyDictionary<string, object> context)
    {
        var exceptions = new List<Exception>();
        var targets = new List<Target>();
        foreach (var result in target.Result.Results)
        {
            if (result.Exception != null)
            {
                exceptions.Add(result.Exception);
            }

            var collection = result.GeneratedSources
                .Where(x => !target.IgnoredFiles.Contains(x.HintName))
                .OrderBy(x => x.HintName)
                .Select(SourceToTarget);
            targets.AddRange(collection);
        }

        if (exceptions.Count == 1)
        {
            throw exceptions.First();
        }

        if (exceptions.Count > 1)
        {
            throw new AggregateException(exceptions);
        }

        if (target.Result.Diagnostics.Any())
        {
            var info = new
            {
                target.Result.Diagnostics,
            };
            return new(info, targets);
        }

        return new(null, targets);
    }

    private static Target SourceToTarget(GeneratedSourceResult source)
    {
        var data = $"""
            //HintName: {source.HintName}
            {source.SourceText}
            """;
        return new("cs", data, Path.GetFileNameWithoutExtension(source.HintName));
    }

    internal sealed class RunResultWithIgnoreList
    {
        public required GeneratorDriverRunResult Result { get; init; }

        public List<string> IgnoredFiles { get; init; } = new();
    }
}
