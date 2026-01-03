# Use CLI Tools

Command-line tools for automation.

## Goal
Use jd-domain CLI for snapshots, diffs, and CI/CD.

## Steps

```bash
# Install
dotnet tool install -g JD.Domain.Cli

# Create snapshot
jd-domain snapshot --manifest domain.json --output v1.0.0.json

# Compare
jd-domain diff v1.0.0.json v2.0.0.json --format md

# Migration plan
jd-domain migrate-plan v1.0.0.json v2.0.0.json
```

## See Also
- [Version Management Tutorial](../tutorials/version-management.md)
