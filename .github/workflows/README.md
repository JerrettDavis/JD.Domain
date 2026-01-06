# GitHub Actions Workflows

This directory contains comprehensive, white-label GitHub Actions workflows for automated CI/CD, documentation, security, and repository management.

## Overview

All workflows are designed to be:
- **White-label**: Parameterized with environment variables for easy reuse
- **Modular**: Each workflow has a specific, focused purpose
- **Comprehensive**: Cover all aspects of modern .NET development
- **Production-ready**: Based on industry best practices

## Workflows

### 1. CI/CD Pipeline (`ci.yml`)

**Purpose**: Main continuous integration and delivery pipeline

**Triggers**:
- Push to `main` branch
- Pull requests to `main`
- Manual dispatch

**Features**:
- **Drift Detection**: Ensures generated files are in sync
- **Multi-platform Testing**: Tests on Ubuntu, Windows, and macOS
- **Multi-version Testing**: Tests .NET 8, 9, and 10
- **Code Coverage**: Uploads coverage to Codecov
- **Automated Releases**: Uses Nerdbank.GitVersioning for semantic versioning
- **Selective Packing**: Only packs infrastructure projects from src/ directory
- **Package Publishing**: Publishes to both NuGet.org (when API key is present) and GitHub Packages
- **GitHub Releases**: Creates release with changelog content and auto-generated notes
- **Resilient Publishing**: Includes error handling and verification steps

**Environment Variables**:
```yaml
SOLUTION_NAME: JD.Domain.sln          # Your solution file
PROJECT_NAME: JD.Domain                # Your project name
DOTNET_VERSION: '10.0.x'              # Primary .NET version
DOTNET_TEST_VERSIONS: '["8.0.x", "9.0.x", "10.0.x"]'  # Test versions
```

**Required Secrets**:
- `NUGET_API_KEY` or `NUGET_TOKEN`: NuGet.org API key for package publishing (optional)
- `CODECOV_TOKEN`: Codecov token for coverage uploads (optional)

**Release Workflow**:
- Triggers on push to `main` branch
- Only publishes stable releases (non-prerelease versions)
- Skips if tag already exists
- Packs only projects in `src/` directory (excludes samples and tests)
- Publishes to NuGet.org if API key is present (supports both `NUGET_API_KEY` and `NUGET_TOKEN`)
- Always publishes to GitHub Packages
- Creates GitHub release with changelog content extracted from CHANGELOG.md
- Includes build artifacts (.nupkg files) in the release

### 2. Documentation (`docfx.yml`)

**Purpose**: Build and deploy DocFX documentation to GitHub Pages

**Triggers**:
- Push to `main` (deploys)
- Pull requests (validates build)
- Changes to docs, XML comments, or docfx.json

**Features**:
- **Automated Build**: Builds DocFX site from markdown and XML comments
- **Validation**: Treats warnings as errors
- **PR Comments**: Posts build status on pull requests
- **GitHub Pages Deployment**: Automatic deployment to GitHub Pages
- **Build Verification**: Validates output before deployment

**Environment Variables**:
```yaml
SOLUTION_NAME: JD.Domain.sln
DOCFX_CONFIG: docfx.json
DOTNET_VERSION: '10.0.x'
```

**Setup Required**:
1. Enable GitHub Pages in repository settings
2. Set source to "GitHub Actions"
3. Ensure `docfx.json` exists in repository root

### 3. Security Analysis (`codeql.yml`)

**Purpose**: Automated security vulnerability scanning with CodeQL

**Triggers**:
- Push to `main`
- Pull requests
- Weekly schedule (Mondays at midnight UTC)
- Manual dispatch

**Features**:
- **Security & Quality Queries**: Comprehensive ruleset
- **SARIF Upload**: Results available in GitHub Security tab
- **Multiple Languages**: Configurable for different languages

**Environment Variables**:
```yaml
SOLUTION_NAME: JD.Domain.sln
DOTNET_VERSION: '10.0.x'
```

### 4. Pull Request Validation (`pr.yml`)

**Purpose**: Fast validation checks for pull requests

**Triggers**:
- Pull request opened, synchronized, or reopened
- Only runs on non-draft PRs

**Features**:
- **PR Title Validation**: Enforces conventional commit format
- **Merge Conflict Detection**: Warns about conflicts early
- **Binary File Check**: Prevents accidental binary commits
- **Quick Build & Test**: Fast feedback loop
- **Code Formatting**: Validates code style
- **PR Size Analysis**: Comments on PR complexity
- **Commit Message Linting**: Validates commit conventions

**Environment Variables**:
```yaml
SOLUTION_NAME: JD.Domain.sln
DOTNET_VERSION: '10.0.x'
```

### 5. Automatic Labeling (`labeler.yml`)

**Purpose**: Automatically label PRs based on changed files

**Triggers**:
- Pull request opened, synchronized, or reopened

**Features**:
- **Path-based Labels**: Labels based on file changes
- **Size Labels**: Adds size labels (XS, S, M, L, XL, XXL)
- **Type Labels**: Extracts type from PR title (feat, fix, etc.)
- **Package Labels**: Labels per package affected

**Configuration**: See `.github/labeler.yml` for label rules

### 6. Stale Items Management (`stale.yml`)

**Purpose**: Manage inactive issues and pull requests

**Triggers**:
- Daily schedule (midnight UTC)
- Manual dispatch

**Features**:
- **Configurable Timeframes**: 60 days for issues, 30 days for PRs
- **Grace Periods**: 7 days for issues, 14 days for PRs
- **Exemptions**: Respects labels, milestones, assignees, and draft status
- **Friendly Messages**: Informative stale and close messages
- **Statistics**: Reports on stale items

**Exempt Labels**:
- Issues: `keep-open`, `pinned`, `security`, `bug`, `enhancement`
- PRs: `keep-open`, `pinned`, `security`, `work-in-progress`, `blocked`

## Configuration Files

### GitVersion.yml

Semantic versioning configuration using GitVersion.

**Version Strategy**:
- **main branch**: Releases (e.g., 1.2.3)
- **develop branch**: Alpha builds (e.g., 1.2.3-alpha.4)
- **release branches**: Beta builds (e.g., 1.2.3-beta.1)
- **feature branches**: Feature builds (e.g., 1.2.3-feature-name.5)
- **PR branches**: PR builds (e.g., 1.2.3-PullRequest123.2)

**Version Bumping**:
- `feat:` commits increment minor version
- `fix:`, `perf:`, etc. increment patch version
- `BREAKING CHANGE:` or `!:` increments major version

### labeler.yml

Automatic labeling rules based on file paths.

**Label Categories**:
- `area/*`: Documentation, CI/CD, tests, samples
- `package/*`: Individual package changes
- `type/*`: Change type (feat, fix, docs, etc.)
- `size/*`: PR size (XS to XXL)
- `dependencies`: Dependency updates
- `breaking-change`: Breaking API changes

## Reusing These Workflows

To use these workflows in another project:

1. **Copy the workflows directory**:
   ```bash
   cp -r .github/workflows /path/to/new/project/.github/
   cp GitVersion.yml /path/to/new/project/
   ```

2. **Update environment variables** in each workflow:
   - `SOLUTION_NAME`: Your solution file name
   - `PROJECT_NAME`: Your project name
   - `DOTNET_VERSION`: Your .NET version

3. **Update labeler.yml** paths:
   - Replace `JD.Domain` with your project name
   - Update package paths to match your structure
   - Add/remove labels as needed

4. **Configure repository secrets** (as needed):
   - `NUGET_API_KEY` or `NUGET_TOKEN`: For NuGet.org package publishing (optional)
   - `CODECOV_TOKEN`: For code coverage (optional)

5. **Enable GitHub Pages**:
   - Repository Settings → Pages
   - Source: GitHub Actions

6. **Create initial labels** (optional):
   ```bash
   gh label create "size/XS" --color "0e8a16"
   gh label create "size/S" --color "0e8a16"
   gh label create "size/M" --color "fbca04"
   gh label create "size/L" --color "d93f0b"
   gh label create "size/XL" --color "b60205"
   gh label create "size/XXL" --color "b60205"
   gh label create "type/feat" --color "0e8a16"
   gh label create "type/fix" --color "d73a4a"
   gh label create "type/docs" --color "0075ca"
   # ... add more as needed
   ```

## Best Practices

### For Contributors

1. **PR Titles**: Use conventional commits format
   ```
   feat: add new feature
   fix: resolve bug in validation
   docs: update README
   ```

2. **Draft PRs**: Mark WIP PRs as draft to skip some checks

3. **Keep PRs Small**: Aim for < 250 lines changed

4. **Update Documentation**: Changes to code should update docs

### For Maintainers

1. **Review Security Alerts**: Check CodeQL results regularly

2. **Monitor Stale Items**: Review stale issues/PRs weekly

3. **Update Dependencies**: Keep actions up to date

4. **Rotate Secrets**: Update NUGET_API_KEY periodically

## Troubleshooting

### CI Build Fails

1. Check drift detection - run `dotnet build` locally
2. Verify all tests pass locally
3. Check code formatting with `dotnet format --verify-no-changes`

### Documentation Build Fails

1. Validate `docfx.json` configuration
2. Check for broken markdown links
3. Ensure XML documentation is enabled in all projects

### Release Not Publishing

1. Verify `NUGET_API_KEY` or `NUGET_TOKEN` secret is set (if publishing to NuGet.org)
2. Check Nerdbank.GitVersioning configuration in `version.json`
3. Ensure version doesn't already exist (check tags)
4. Check that commit is on `main` branch
5. Verify it's a stable release (not a prerelease)
6. Check that all tests passed before the release job

### Labels Not Applied

1. Verify `labeler.yml` paths match your structure
2. Check that labels exist in repository
3. Ensure workflow has `pull-requests: write` permission

## Support

For issues or questions about these workflows:
1. Check workflow run logs in Actions tab
2. Review this README
3. Open an issue with the `area/ci-cd` label

## License

These workflows are part of the JD.Domain project and use the same license.
