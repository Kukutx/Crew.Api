namespace Crew.SeedDataImporter.Options;

public sealed class SeedOptions
{
    public string ExcelPath { get; set; } = "seed-data.xlsx";
    public bool OverwriteExisting { get; set; }
}
