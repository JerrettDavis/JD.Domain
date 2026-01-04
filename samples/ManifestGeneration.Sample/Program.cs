using ManifestGeneration.Sample;

Console.WriteLine("=== JD.Domain Manifest Generation Sample ===");
Console.WriteLine();

// The manifest is automatically generated from the entity classes
// marked with [DomainEntity] and [DomainValueObject] attributes
var manifest = ECommerceManifest.GeneratedManifest;

Console.WriteLine($"Manifest Name: {manifest.Name}");
Console.WriteLine($"Version: {manifest.Version}");
Console.WriteLine($"Sources: {string.Join(", ", manifest.Sources.Select(s => s.Type))}");
Console.WriteLine($"Created At: {manifest.CreatedAt}");
Console.WriteLine();

Console.WriteLine($"Entities: {manifest.Entities.Count}");
foreach (var entity in manifest.Entities)
{
    Console.WriteLine($"  - {entity.Name}");
    Console.WriteLine($"    Type: {entity.TypeName}");
    Console.WriteLine($"    Table: {entity.SchemaName}.{entity.TableName}");
    Console.WriteLine($"    Properties: {entity.Properties.Count}");

    if (entity.KeyProperties.Count > 0)
    {
        Console.WriteLine($"    Keys: {string.Join(", ", entity.KeyProperties)}");
    }

    foreach (var prop in entity.Properties)
    {
        var required = prop.IsRequired ? "required" : "optional";
        var maxLen = prop.MaxLength.HasValue ? $", MaxLength={prop.MaxLength}" : "";
        Console.WriteLine($"      - {prop.Name}: {prop.TypeName} ({required}{maxLen})");
    }

    Console.WriteLine();
}

Console.WriteLine($"Value Objects: {manifest.ValueObjects.Count}");
foreach (var vo in manifest.ValueObjects)
{
    Console.WriteLine($"  - {vo.Name}");
    Console.WriteLine($"    Type: {vo.TypeName}");
    Console.WriteLine($"    Properties: {vo.Properties.Count}");

    foreach (var prop in vo.Properties)
    {
        var required = prop.IsRequired ? "required" : "optional";
        var maxLen = prop.MaxLength.HasValue ? $", MaxLength={prop.MaxLength}" : "";
        Console.WriteLine($"      - {prop.Name}: {prop.TypeName} ({required}{maxLen})");
    }

    Console.WriteLine();
}

Console.WriteLine("=== Generation Complete ===");
Console.WriteLine();
Console.WriteLine("The manifest was automatically generated at compile-time");
Console.WriteLine("from the entity classes marked with [DomainEntity] and [DomainValueObject].");
Console.WriteLine();
Console.WriteLine("NO MANUAL STRING WRITING REQUIRED!");
