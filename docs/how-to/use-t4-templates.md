# Use T4 Templates

Integrate with T4 for code generation.

## Goal
Load domain manifests in T4 templates for custom code generation.

## Steps

```
<#@ assembly name="JD.Domain.T4.Shims" #>
<#@ import namespace="JD.Domain.T4.Shims" #>
<#
var loader = new T4ManifestLoader();
var manifest = loader.LoadFromFile("domain.json");
#>
```

## See Also
- [API: T4ManifestLoader](../../api/JD.Domain.T4.Shims.T4ManifestLoader.yml)
