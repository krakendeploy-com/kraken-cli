# Kraken CLI

Command-line interface for interacting with the Kraken API using API key authentication.

## Prerequisites

- .NET 9.0 or later
- A valid Kraken API key (starting with `krk_`)

## Configuration

The CLI can be configured with the following environment variables:

- `KRAKEN_API_URL`: Base URL for the Kraken API (defaults to `https://api.krakendeploy.com`)

Alternatively, you can pass the `--base-url` parameter to override the base URL.

## Usage

### Common Parameters

All commands require the following parameters:

- `--action`: The action to perform (`create-release`, `create-deployment`)
- `--org-id`: Organization ID or slug
- `--workspace-id`: Workspace ID or slug
- `--project-id`: Project ID or slug
- `--api-key`: Your Kraken API key (must start with `krk_`)

### Create Release

Creates a new release with specified artifacts and/or registry images.

**Important:** Artifacts and registry images must be specified in the format `name:version:artifact-source-slug`, where:
- `name`: The artifact/image name
- `version`: The version string
- `artifact-source-slug`: The slug identifier for the artifact source (e.g., `kraken-git`, `docker-hub`)

#### Example with Artifacts

```bash
dotnet run --project Kraken.Cli.csproj -- \
  --action create-release \
  --org-id my-org \
  --workspace-id my-workspace \
  --project-id my-project \
  --api-key krk_your_api_key_here \
  --version 1.0.0 \
  --packages "artifact1:1.0.0:kraken-git;artifact2:2.0.0:azure-artifacts"
```

#### Example with Registry Images

```bash
dotnet run --project Kraken.Cli.csproj -- \
  --action create-release \
  --org-id my-org \
  --workspace-id my-workspace \
  --project-id my-project \
  --api-key krk_your_api_key_here \
  --version 1.0.0 \
  --registry-images "myapp:1.0.0:docker-hub;nginx:latest:ghcr"
```

#### Example with Both

```bash
dotnet run --project Kraken.Cli.csproj -- \
  --action create-release \
  --org-id my-org \
  --workspace-id my-workspace \
  --project-id my-project \
  --api-key krk_your_api_key_here \
  --version 1.0.0 \
  --packages "artifact1:1.0.0:kraken-git" \
  --registry-images "myapp:1.0.0:docker-hub;nginx:latest:ghcr"
```

Parameters:
- `--version`: Release version (required)
- `--packages`: Semicolon-separated list of `name:version:slug` entries (required if `--registry-images` is not provided)
- `--registry-images`: Semicolon-separated list of `name:version:slug` entries (required if `--packages` is not provided)

**Note:** You must specify at least one of `--packages` or `--registry-images`, but you can specify both.

### Create Deployment

Creates a new deployment for a specific environment and release. You can specify the release using either a release ID or a version string.

#### Option 1: Using Release ID

```bash
dotnet run --project Kraken.Cli.csproj -- \
  --action create-deployment \
  --org-id my-org \
  --workspace-id my-workspace \
  --project-id my-project \
  --api-key krk_your_api_key_here \
  --environment-id my-environment \
  --release-id my-release-id
```

#### Option 2: Using Release Version

```bash
dotnet run --project Kraken.Cli.csproj -- \
  --action create-deployment \
  --org-id my-org \
  --workspace-id my-workspace \
  --project-id my-project \
  --api-key krk_your_api_key_here \
  --environment-id my-environment \
  --version 1.0.0
```

Parameters:
- `--environment-id`: Environment ID or slug (required)
- `--release-id`: Specific release ID (required if `--version` is not provided)
- `--version`: Release version string (required if `--release-id` is not provided)

**Note:** You must specify either `--release-id` or `--version`, but not both.

## Examples

### Using a custom API URL

```bash
# Via parameter
dotnet run --project Kraken.Cli.csproj -- \
  --action create-release \
  --base-url https://api.kraken.example.com \
  --org-id my-org \
  --workspace-id my-workspace \
  --project-id my-project \
  --api-key krk_your_api_key_here \
  --version 1.0.0 \
  --packages "artifact1:1.0.0:kraken-git"

# Via environment variable (Windows CMD)
set KRAKEN_API_URL=https://api.kraken.example.com
dotnet run --project Kraken.Cli.csproj -- \
  --action create-release \
  --org-id my-org \
  --workspace-id my-workspace \
  --project-id my-project \
  --api-key krk_your_api_key_here \
  --version 1.0.0 \
  --packages "artifact1:1.0.0:kraken-git"

# Via environment variable (PowerShell)
$env:KRAKEN_API_URL="https://api.kraken.example.com"
dotnet run --project Kraken.Cli.csproj -- \
  --action create-release \
  --org-id my-org \
  --workspace-id my-workspace \
  --project-id my-project \
  --api-key krk_your_api_key_here \
  --version 1.0.0 \
  --packages "artifact1:1.0.0:kraken-git"
```

### Multiple Artifacts

You can specify multiple artifacts or registry images by separating them with semicolons:

```bash
dotnet run --project Kraken.Cli.csproj -- \
  --action create-release \
  --org-id my-org \
  --workspace-id my-workspace \
  --project-id my-project \
  --api-key krk_your_api_key_here \
  --version 1.0.0 \
  --packages "api:1.0.0:kraken-git;web:1.0.0:kraken-git;worker:1.0.0:azure-artifacts" \
  --registry-images "myapp:1.0.0:docker-hub;nginx:1.21:docker-hub"
```

## API Endpoints

The CLI uses the following API endpoints (all require API key authentication):

- `POST /organization/{organizationId}/workspaces/{workspaceId}/projects/{projectId}/releases/create` - Create release
- `POST /organization/{organizationId}/workspaces/{workspaceId}/projects/{projectId}/environments/{environmentId}/deployments/releases/{releaseId}/create` - Create deployment by release ID
- `POST /organization/{organizationId}/workspaces/{workspaceId}/projects/{projectId}/environments/{environmentId}/deployments/version/{version}/create` - Create deployment by release version

## Error Handling

The CLI provides clear error messages for common issues:

- Missing required parameters
- Invalid API key format
- Empty values
- HTTP errors from the API

## Security

- API keys are validated to ensure they start with `krk_`
- API keys are passed via the Authorization header as Bearer tokens
- All requests use the configured base URL (configurable for HTTPS in production)
