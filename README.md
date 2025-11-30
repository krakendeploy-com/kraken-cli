# Kraken CLI

Command-line interface for interacting with Kraken Deploy API to create releases and deployments.

## GitHub Action Usage

The easiest way to use Kraken CLI in GitHub Actions is with the action syntax:

```yaml
- name: Create Release
  uses: krakendeploy-com/kraken-cli@v1.0.13
  with:
    action: create-release
    org-id: ${{ secrets.KRAKEN_ORG_ID }}
    workspace-id: ${{ secrets.KRAKEN_WORKSPACE_ID }}
    project-id: ${{ secrets.KRAKEN_PROJECT_ID }}
    api-key: ${{ secrets.KRAKEN_API_KEY }}
    version: 1.0.${{ github.run_number }}
    registry-images: kraken-api:1.0.${{ github.run_number }}:${{ secrets.KRAKEN_ARTIFACT_SOURCE }}

- name: Create Deployment
  uses: krakendeploy-com/kraken-cli@v1.0.13
  with:
    action: create-deployment
    org-id: ${{ secrets.KRAKEN_ORG_ID }}
    workspace-id: ${{ secrets.KRAKEN_WORKSPACE_ID }}
    project-id: ${{ secrets.KRAKEN_PROJECT_ID }}
    api-key: ${{ secrets.KRAKEN_API_KEY }}
    environment-id: ${{ secrets.KRAKEN_ENVIRONMENT_ID }}
    version: 1.0.${{ github.run_number }}
```

See [GitHub Action Inputs](#github-action-inputs) for all available options.

## Installation

### Download Pre-built Binaries

Download the latest release for your platform from the [releases page](https://github.com/krakendeploy-com/kraken-cli/releases/latest):

- **Linux x64**: `kraken-cli-linux-x64`
- **Windows x64**: `kraken-cli-win-x64.exe`
- **macOS x64**: `kraken-cli-osx-x64`
- **macOS ARM64**: `kraken-cli-osx-arm64`

### Quick Installation (Linux/macOS)

```bash
# Download and install
curl -L -o kraken-cli https://github.com/krakendeploy-com/kraken-cli/releases/latest/download/kraken-cli-linux-x64
chmod +x kraken-cli
sudo mv kraken-cli /usr/local/bin/

# Verify installation
kraken-cli --help
```

### Build from Source

```bash
git clone https://github.com/krakendeploy-com/kraken-cli.git
cd kraken-cli
dotnet publish src/Kraken.Cli/Kraken.Cli.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

## Usage

### Create a Release

Create a new release with packages and/or registry images:

```bash
kraken-cli \
  --action create-release \
  --org-id "org_abc123" \
  --workspace-id "wks_def456" \
  --project-id "prj_ghi789" \
  --api-key "krk_your_api_key" \
  --version "1.0.123" \
  --packages "package1:1.0.0:nuget-source-id;package2:2.0.0:nuget-source-id" \
  --registry-images "image1:1.0.0:docker-registry-id;image2:2.0.0:docker-registry-id"
```

### Create a Deployment (by Release ID)

Deploy a specific release to an environment:

```bash
kraken-cli \
  --action create-deployment \
  --org-id "org_abc123" \
  --workspace-id "wks_def456" \
  --project-id "prj_ghi789" \
  --api-key "krk_your_api_key" \
  --environment-id "env_prod123" \
  --release-id "rel_xyz789"
```

### Create a Deployment (by Version)

Deploy a release by version number:

```bash
kraken-cli \
  --action create-deployment \
  --org-id "org_abc123" \
  --workspace-id "wks_def456" \
  --project-id "prj_ghi789" \
  --api-key "krk_your_api_key" \
  --environment-id "env_prod123" \
  --version "1.0.123"
```

## Parameters

### Common Parameters (All Actions)

| Parameter | Required | Description |
|-----------|----------|-------------|
| `--action` | Yes | Action to perform: `create-release` or `create-deployment` |
| `--org-id` | Yes | Organization ID |
| `--workspace-id` | Yes | Workspace ID |
| `--project-id` | Yes | Project ID |
| `--api-key` | Yes | API key (must start with `krk_`) |
| `--base-url` | No | Base API URL (default: `https://api.krakendeploy.com`) |

### Create Release Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `--version` | Yes | Release version (e.g., `1.0.123`) |
| `--packages` | No* | Semicolon-separated list of packages in format `name:version:artifact-source-slug-id` |
| `--registry-images` | No* | Semicolon-separated list of images in format `name:version:artifact-source-slug-id` |

*At least one of `--packages` or `--registry-images` must be provided.

### Create Deployment Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `--environment-id` | Yes | Target environment ID |
| `--release-id` | No* | Specific release ID to deploy |
| `--version` | No* | Release version to deploy |

*Either `--release-id` or `--version` must be provided (but not both).

## Examples

### Example 1: Create Release with Docker Images

```bash
kraken-cli \
  --action create-release \
  --org-id "org_abc123" \
  --workspace-id "wks_def456" \
  --project-id "prj_api" \
  --api-key "krk_secret_key" \
  --version "1.0.42" \
  --registry-images "kraken-api:1.0.42:ghcr-source-id"
```

### Example 2: Create Release with NuGet Packages

```bash
kraken-cli \
  --action create-release \
  --org-id "org_abc123" \
  --workspace-id "wks_def456" \
  --project-id "prj_lib" \
  --api-key "krk_secret_key" \
  --version "2.1.5" \
  --packages "Kraken.Shared:2.1.5:nuget-source-id"
```

### Example 3: Create Release with Multiple Artifacts

```bash
kraken-cli \
  --action create-release \
  --org-id "org_abc123" \
  --workspace-id "wks_def456" \
  --project-id "prj_full" \
  --api-key "krk_secret_key" \
  --version "3.0.0" \
  --packages "Package.A:3.0.0:nuget-id;Package.B:3.0.0:nuget-id" \
  --registry-images "app-api:3.0.0:docker-id;app-worker:3.0.0:docker-id"
```

### Example 4: Deploy by Release ID

```bash
kraken-cli \
  --action create-deployment \
  --org-id "org_abc123" \
  --workspace-id "wks_def456" \
  --project-id "prj_api" \
  --api-key "krk_secret_key" \
  --environment-id "env_production" \
  --release-id "rel_specific123"
```

### Example 5: Deploy by Version

```bash
kraken-cli \
  --action create-deployment \
  --org-id "org_abc123" \
  --workspace-id "wks_def456" \
  --project-id "prj_api" \
  --api-key "krk_secret_key" \
  --environment-id "env_staging" \
  --version "1.0.42"
```

## GitHub Action Inputs

When using the Kraken CLI as a GitHub Action, the following inputs are available:

### Common Inputs (All Actions)

| Input | Required | Description |
|-------|----------|-------------|
| `action` | Yes | Action to perform: `create-release`, `create-deployment`, or `upload-artifact` |
| `org-id` | Yes | Organization ID |
| `workspace-id` | Yes | Workspace ID |
| `api-key` | Yes | API key for authentication |
| `base-url` | No | Base API URL (default: `https://api.krakendeploy.com`) |

### Create Release Inputs

| Input | Required | Description |
|-------|----------|-------------|
| `project-id` | Yes | Project ID |
| `version` | Yes | Release version (e.g., `1.0.123`) |
| `packages` | No* | Semicolon-separated list of packages in format `name:version:source-id` |
| `registry-images` | No* | Semicolon-separated list of images in format `name:version:source-id` |

*At least one of `packages` or `registry-images` must be provided.

### Create Deployment Inputs

| Input | Required | Description |
|-------|----------|-------------|
| `project-id` | Yes | Project ID |
| `environment-id` | Yes | Target environment ID |
| `release-id` | No* | Specific release ID to deploy |
| `version` | No* | Release version to deploy |

*Either `release-id` or `version` must be provided (but not both).

### Upload Artifact Inputs

| Input | Required | Description |
|-------|----------|-------------|
| `name` | Yes | Artifact name |
| `version` | Yes | Artifact version |
| `file` | Yes | Path to the file to upload |

### Complete GitHub Action Examples

#### Example: Build, Release, and Deploy

```yaml
name: Build and Deploy

on:
  push:
    branches: [ master ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Set version
        id: version
        run: echo "semver=1.0.${{ github.run_number }}" >> $GITHUB_OUTPUT
      
      # Your build steps here...
      
      - name: Create Release
        uses: krakendeploy-com/kraken-cli@v1
        with:
          action: create-release
          org-id: ${{ secrets.KRAKEN_ORG_ID }}
          workspace-id: ${{ secrets.KRAKEN_WORKSPACE_ID }}
          project-id: ${{ secrets.KRAKEN_PROJECT_ID }}
          api-key: ${{ secrets.KRAKEN_API_KEY }}
          version: ${{ steps.version.outputs.semver }}
          registry-images: my-app:${{ steps.version.outputs.semver }}:${{ secrets.KRAKEN_ARTIFACT_SOURCE }}
      
      - name: Deploy to Production
        uses: krakendeploy-com/kraken-cli@v1
        with:
          action: create-deployment
          org-id: ${{ secrets.KRAKEN_ORG_ID }}
          workspace-id: ${{ secrets.KRAKEN_WORKSPACE_ID }}
          project-id: ${{ secrets.KRAKEN_PROJECT_ID }}
          api-key: ${{ secrets.KRAKEN_API_KEY }}
          environment-id: ${{ secrets.KRAKEN_ENVIRONMENT_ID_PROD }}
          version: ${{ steps.version.outputs.semver }}
```

#### Example: Multiple Packages

```yaml
- name: Create Release with Multiple Packages
  uses: krakendeploy-com/kraken-cli@v1
  with:
    action: create-release
    org-id: ${{ secrets.KRAKEN_ORG_ID }}
    workspace-id: ${{ secrets.KRAKEN_WORKSPACE_ID }}
    project-id: ${{ secrets.KRAKEN_PROJECT_ID }}
    api-key: ${{ secrets.KRAKEN_API_KEY }}
    version: ${{ steps.version.outputs.semver }}
    packages: Package.A:1.0.0:nuget-source;Package.B:1.0.0:nuget-source
    registry-images: api:1.0.0:docker-source;worker:1.0.0:docker-source
```

## Output Messages

The CLI provides clear feedback:

- ✅ Success messages with green checkmarks
- ❌ Error messages with red X marks
- 📦 Package/artifact information
- 🐳 Docker image information
- ⚠️ Warnings for invalid inputs

## Error Handling

The CLI validates inputs and provides helpful error messages:

- Missing required parameters
- Invalid API key format
- Empty version or artifact lists
- HTTP errors with response details

## API Key Format

API keys must start with `krk_`. You can generate an API key from your Kraken Deploy dashboard:

1. Navigate to **Settings** → **API Keys**
2. Click **Create API Key**
3. Copy and store the key securely

## Base URL Override

For testing or on-premises deployments, override the base URL:

```bash
kraken-cli \
  --base-url "https://custom-api.example.com" \
  --action create-release \
  # ... other parameters
```

## Development

### Prerequisites

- .NET 9.0 SDK or later

### Build

```bash
dotnet build src/Kraken.Cli.sln
```

### Run

```bash
dotnet run --project src/Kraken.Cli/Kraken.Cli.csproj -- --action create-release --help
```

### Publish

```bash
dotnet publish src/Kraken.Cli/Kraken.Cli.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o ./publish
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

[Add your license here]

## Support

For issues or questions:
- Open an issue on GitHub
- Visit [krakendeploy.com](https://krakendeploy.com)

