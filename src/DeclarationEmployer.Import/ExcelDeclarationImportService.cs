using System.Globalization;
using ClosedXML.Excel;
using DeclarationEmployer.Domain.Declarations;

namespace DeclarationEmployer.Import;

public sealed class ExcelDeclarationImportService : IExcelDeclarationImportService
{
    public Task<ExcelImportParseResult> ParseAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("Le fichier Excel ne contient aucune feuille.");

        var usedRange = worksheet.RangeUsed();
        if (usedRange is null)
        {
            return Task.FromResult(new ExcelImportParseResult
            {
                Issues =
                [
                    new ExcelImportValidationIssue
                    {
                        RowNumber = 0,
                        Code = "EMPTY_FILE",
                        Message = "Le fichier Excel est vide."
                    }
                ]
            });
        }

        var headerRow = worksheet.Row(usedRange.FirstRow().RowNumber());
        var lastRowNumber = usedRange.LastRow().RowNumber();
        var columnMap = BuildColumnMap(headerRow);
        var issues = ValidateRequiredColumns(columnMap, headerRow.RowNumber());

        var rows = new List<ExcelImportParsedRow>();
        if (issues.Count > 0)
        {
            return Task.FromResult(new ExcelImportParseResult
            {
                Rows = rows,
                Issues = issues
            });
        }

        for (var rowNumber = headerRow.RowNumber() + 1; rowNumber <= lastRowNumber; rowNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var row = worksheet.Row(rowNumber);
            if (IsEmptyRow(row, columnMap.Values))
            {
                continue;
            }

            var parsedRow = new ExcelImportParsedRow
            {
                RowNumber = rowNumber,
                BeneficiaryIdentifierType = GetStringValue(row, columnMap, ExcelImportColumnNames.IdentifierType),
                BeneficiaryIdentifier = GetStringValue(row, columnMap, ExcelImportColumnNames.Identifier),
                BeneficiaryName = GetStringValue(row, columnMap, ExcelImportColumnNames.BeneficiaryName),
                OperationType = GetStringValue(row, columnMap, ExcelImportColumnNames.OperationType),
                FiscalCategory = GetStringValue(row, columnMap, ExcelImportColumnNames.FiscalCategory),
                GrossAmount = GetDecimalValue(row, columnMap, ExcelImportColumnNames.GrossAmount, issues, true),
                TaxableAmount = GetDecimalValue(row, columnMap, ExcelImportColumnNames.TaxableAmount, issues, true),
                Rate = GetDecimalValue(row, columnMap, ExcelImportColumnNames.Rate, issues, true),
                WithheldAmount = GetDecimalValue(row, columnMap, ExcelImportColumnNames.WithheldAmount, issues, true),
                PaymentDate = GetDateValue(row, columnMap, ExcelImportColumnNames.PaymentDate, issues),
                DocumentReference = GetStringValue(row, columnMap, ExcelImportColumnNames.DocumentReference),
                Notes = GetStringValue(row, columnMap, ExcelImportColumnNames.Notes),
                Address = GetStringValue(row, columnMap, ExcelImportColumnNames.Address),
                Country = GetStringValue(row, columnMap, ExcelImportColumnNames.Country),
                IsResident = GetBooleanValue(row, columnMap, ExcelImportColumnNames.IsResident, issues)
            };

            ValidateRequiredString(parsedRow.RowNumber, ExcelImportColumnNames.IdentifierType, parsedRow.BeneficiaryIdentifierType, issues, "Le type d'identifiant est obligatoire.");
            ValidateRequiredString(parsedRow.RowNumber, ExcelImportColumnNames.Identifier, parsedRow.BeneficiaryIdentifier, issues, "L'identifiant est obligatoire.");
            ValidateRequiredString(parsedRow.RowNumber, ExcelImportColumnNames.BeneficiaryName, parsedRow.BeneficiaryName, issues, "Le nom du beneficiaire est obligatoire.");
            ValidateRequiredString(parsedRow.RowNumber, ExcelImportColumnNames.OperationType, parsedRow.OperationType, issues, "Le type d'operation est obligatoire.");

            if (!string.IsNullOrWhiteSpace(parsedRow.BeneficiaryIdentifierType) &&
                !Enum.TryParse<BeneficiaryIdentifierType>(parsedRow.BeneficiaryIdentifierType, true, out _))
            {
                issues.Add(new ExcelImportValidationIssue
                {
                    RowNumber = parsedRow.RowNumber,
                    ColumnName = ExcelImportColumnNames.IdentifierType,
                    Code = "INVALID_IDENTIFIER_TYPE",
                    Message = "Le type d'identifiant est invalide."
                });
            }

            ValidateMin(parsedRow.RowNumber, ExcelImportColumnNames.GrossAmount, parsedRow.GrossAmount, 0m, issues, "Le montant brut doit etre superieur ou egal a 0.");
            ValidateMin(parsedRow.RowNumber, ExcelImportColumnNames.TaxableAmount, parsedRow.TaxableAmount, 0m, issues, "Le montant imposable doit etre superieur ou egal a 0.");
            ValidateRange(parsedRow.RowNumber, ExcelImportColumnNames.Rate, parsedRow.Rate, 0m, 100m, issues, "Le taux doit etre compris entre 0 et 100.");
            ValidateMin(parsedRow.RowNumber, ExcelImportColumnNames.WithheldAmount, parsedRow.WithheldAmount, 0m, issues, "La retenue doit etre superieure ou egale a 0.");

            parsedRow.IsValid = !issues.Any(x => x.RowNumber == parsedRow.RowNumber);
            rows.Add(parsedRow);
        }

        return Task.FromResult(new ExcelImportParseResult
        {
            Rows = rows,
            Issues = issues
        });
    }

    private static Dictionary<string, int> BuildColumnMap(IXLRow headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.CellsUsed())
        {
            var value = cell.GetString();
            if (ExcelImportColumnNames.TryResolve(value, out var canonicalName) && !map.ContainsKey(canonicalName))
            {
                map[canonicalName] = cell.Address.ColumnNumber;
            }
        }

        return map;
    }

    private static List<ExcelImportValidationIssue> ValidateRequiredColumns(Dictionary<string, int> columnMap, int headerRowNumber)
    {
        var issues = new List<ExcelImportValidationIssue>();
        foreach (var requiredColumn in ExcelImportColumnNames.RequiredColumns)
        {
            if (!columnMap.ContainsKey(requiredColumn))
            {
                issues.Add(new ExcelImportValidationIssue
                {
                    RowNumber = headerRowNumber,
                    ColumnName = requiredColumn,
                    Code = "MISSING_REQUIRED_COLUMN",
                    Message = $"La colonne obligatoire '{requiredColumn}' est absente."
                });
            }
        }

        return issues;
    }

    private static bool IsEmptyRow(IXLRow row, IEnumerable<int> usedColumns)
    {
        return usedColumns.All(column => string.IsNullOrWhiteSpace(row.Cell(column).GetString()));
    }

    private static string? GetStringValue(IXLRow row, Dictionary<string, int> columnMap, string columnName)
    {
        if (!columnMap.TryGetValue(columnName, out var columnNumber))
        {
            return null;
        }

        var value = row.Cell(columnNumber).GetString().Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static decimal? GetDecimalValue(
        IXLRow row,
        Dictionary<string, int> columnMap,
        string columnName,
        ICollection<ExcelImportValidationIssue> issues,
        bool required)
    {
        if (!columnMap.TryGetValue(columnName, out var columnNumber))
        {
            return null;
        }

        var rawValue = row.Cell(columnNumber).GetString().Trim();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            if (required)
            {
                issues.Add(new ExcelImportValidationIssue
                {
                    RowNumber = row.RowNumber(),
                    ColumnName = columnName,
                    Code = "REQUIRED_VALUE",
                    Message = $"La valeur '{columnName}' est obligatoire."
                });
            }

            return null;
        }

        if (decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantValue) ||
            decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.GetCultureInfo("fr-FR"), out invariantValue) ||
            decimal.TryParse(rawValue.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out invariantValue))
        {
            return invariantValue;
        }

        issues.Add(new ExcelImportValidationIssue
        {
            RowNumber = row.RowNumber(),
            ColumnName = columnName,
            Code = "INVALID_DECIMAL",
            Message = $"La valeur '{columnName}' n'est pas un nombre valide."
        });

        return null;
    }

    private static DateTime? GetDateValue(
        IXLRow row,
        Dictionary<string, int> columnMap,
        string columnName,
        ICollection<ExcelImportValidationIssue> issues)
    {
        if (!columnMap.TryGetValue(columnName, out var columnNumber))
        {
            return null;
        }

        var cell = row.Cell(columnNumber);
        var rawValue = cell.GetString().Trim();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        if (cell.TryGetValue<DateTime>(out var dateValue) ||
            DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue) ||
            DateTime.TryParse(rawValue, CultureInfo.GetCultureInfo("fr-FR"), DateTimeStyles.None, out dateValue))
        {
            return dateValue;
        }

        issues.Add(new ExcelImportValidationIssue
        {
            RowNumber = row.RowNumber(),
            ColumnName = columnName,
            Code = "INVALID_DATE",
            Message = $"La date '{columnName}' est invalide."
        });

        return null;
    }

    private static bool? GetBooleanValue(
        IXLRow row,
        Dictionary<string, int> columnMap,
        string columnName,
        ICollection<ExcelImportValidationIssue> issues)
    {
        if (!columnMap.TryGetValue(columnName, out var columnNumber))
        {
            return null;
        }

        var rawValue = row.Cell(columnNumber).GetString().Trim();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        if (bool.TryParse(rawValue, out var boolValue))
        {
            return boolValue;
        }

        if (rawValue is "1" or "oui" or "Oui" or "OUI")
        {
            return true;
        }

        if (rawValue is "0" or "non" or "Non" or "NON")
        {
            return false;
        }

        issues.Add(new ExcelImportValidationIssue
        {
            RowNumber = row.RowNumber(),
            ColumnName = columnName,
            Code = "INVALID_BOOLEAN",
            Message = $"La valeur '{columnName}' doit etre booleenne."
        });

        return null;
    }

    private static void ValidateRequiredString(
        int rowNumber,
        string columnName,
        string? value,
        ICollection<ExcelImportValidationIssue> issues,
        string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add(new ExcelImportValidationIssue
            {
                RowNumber = rowNumber,
                ColumnName = columnName,
                Code = "REQUIRED_VALUE",
                Message = message
            });
        }
    }

    private static void ValidateMin(
        int rowNumber,
        string columnName,
        decimal? value,
        decimal minValue,
        ICollection<ExcelImportValidationIssue> issues,
        string message)
    {
        if (value.HasValue && value.Value < minValue)
        {
            issues.Add(new ExcelImportValidationIssue
            {
                RowNumber = rowNumber,
                ColumnName = columnName,
                Code = "OUT_OF_RANGE",
                Message = message
            });
        }
    }

    private static void ValidateRange(
        int rowNumber,
        string columnName,
        decimal? value,
        decimal minValue,
        decimal maxValue,
        ICollection<ExcelImportValidationIssue> issues,
        string message)
    {
        if (value.HasValue && (value.Value < minValue || value.Value > maxValue))
        {
            issues.Add(new ExcelImportValidationIssue
            {
                RowNumber = rowNumber,
                ColumnName = columnName,
                Code = "OUT_OF_RANGE",
                Message = message
            });
        }
    }
}
