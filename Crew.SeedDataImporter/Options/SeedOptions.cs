namespace Crew.SeedDataImporter.Options;

public sealed class SeedOptions
{
    public string ExcelPath { get; set; } = "SeedData/seed-data.xlsx";
    public bool OverwriteExisting { get; set; }
}
