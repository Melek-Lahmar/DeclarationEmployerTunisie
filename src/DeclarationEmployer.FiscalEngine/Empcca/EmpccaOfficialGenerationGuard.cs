namespace DeclarationEmployer.FiscalEngine.Empcca;

public sealed class EmpccaOfficialGenerationGuard
{
    public IReadOnlyList<string> Validate(EmpccaGenerationArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        var issues = artifact.BlockingIssues.ToList();
        var isDecemp = artifact.FileName == "DECEMP_25";
        var expectedLength = isDecemp ? 38 : 399;

        if (isDecemp && artifact.Lines.Count != 51)
            issues.Add("DECEMP_25 doit contenir exactement 51 enregistrements.");
        if (!isDecemp && artifact.Lines.Count < 3)
            issues.Add("Un fichier annexe doit contenir un entete, au moins une ligne et une fin.");

        for (var index = 0; index < artifact.Lines.Count; index++)
        {
            var line = artifact.Lines[index];
            if (line.Length != expectedLength)
                issues.Add($"La ligne {index + 1} contient {line.Length} caracteres au lieu de {expectedLength}.");
            try
            {
                FixedWidthFormatter.EnsureAscii(line);
            }
            catch (ArgumentException)
            {
                issues.Add($"La ligne {index + 1} contient un caractere non ASCII imprimable.");
            }
        }

        if (!artifact.IsOfficial)
            issues.Add("L'artefact est une previsualisation technique et ne peut pas etre marque officiel.");

        return issues.Distinct(StringComparer.Ordinal).ToArray();
    }

    public void EnsureOfficialGenerationAllowed(IEnumerable<EmpccaGenerationArtifact> artifacts)
    {
        ArgumentNullException.ThrowIfNull(artifacts);
        var issues = artifacts.SelectMany(Validate).Distinct(StringComparer.Ordinal).ToArray();
        if (issues.Length > 0)
            throw new InvalidOperationException("Generation officielle bloquee : " + string.Join(" ", issues));
    }
}
