# Kraken CLI

Command-line interface for interacting with the Kraken API using API key authentication.

## Prerequisites

- .NET 9.0 or later
- A valid Kraken API key (starting with `krk_`)

## Configuration

The CLI can be configured with the following environment variables:

- `KRAKEN_API_URL`: Base URL for the Kraken API (defaults to `http://localhost:5000`)

Alternatively, you can pass the `--BaseUrl` parameter to override the base URL.

## Usage

### Common Parameters

All commands require the following parameters:

- `--Action`: The action to perform (CreateRelease, CreateDeployment, UploadPackage)
- `--OrgId`: Organization ID or slug
- `--WorkspaceID`: Workspace ID or slug
- `--ProjectId`: Project ID or slug
- `--ApiKey`: Your Kraken API key

### Create Release

Creates a new release with specified artifacts and/or registry images.

#### Example with Artifacts

```bash
dotnet run --project Kraken.Cli.csproj \
  --Action=CreateRelease \
  --OrgId=my-org \
  --WorkspaceID=my-workspace \
  --ProjectId=my-project \
  --ApiKey=krk_your_api_key_here \
  --Version=1.0.0 \
  --Packages="artifact1=1.0.0;artifact2=2.0.0" \
  --Name="Release 1.0.0"
```

#### Example with Registry Images

```bash
dotnet run --project Kraken.Cli.csproj \
  --Action=CreateRelease \
  --OrgId=my-org \
  --WorkspaceID=my-workspace \
  --ProjectId=my-project \
  --ApiKey=krk_your_api_key_here \
  --Version=1.0.0 \
  --RegistryImages="myapp=1.0.0;nginx=latest" \
  --Name="Release 1.0.0"
```

#### Example with Both

```bash
dotnet run --project Kraken.Cli.csproj \
  --Action=CreateRelease \
  --OrgId=my-org \
  --WorkspaceID=my-workspace \
  --ProjectId=my-project \
  --ApiKey=krk_your_api_key_here \
  --Version=1.0.0 \
  --Packages="artifact1=1.0.0" \
  --RegistryImages="myapp=1.0.0;nginx=latest" \
  --Name="Release 1.0.0"
```

Parameters:
- `--Version`: Release version (required)
- `--Packages`: Semicolon-separated list of package_name=version pairs (required if --RegistryImages is not provided)
- `--RegistryImages`: Semicolon-separated list of image_name=version pairs (required if --Packages is not provided)
- `--Name`: Release name (optional)

**Note:** You must specify at least one of `--Packages` or `--RegistryImages`, but you can specify both.

### Create Deployment

Creates a new deployment for a specific environment and release. You can specify the release using either a ReleaseId or a Version string.

#### Option 1: Using Release ID

```bash
dotnet run --project Kraken.Cli.csproj \
  --Action=CreateDeployment \
  --OrgId=my-org \
  --WorkspaceID=my-workspace \
  --ProjectId=my-project \
  --ApiKey=krk_your_api_key_here \
  --EnvironmentId=my-environment \
  --ReleaseId=my-release-id
```

#### Option 2: Using Release Version

```bash
dotnet run --project Kraken.Cli.csproj \
  --Action=CreateDeployment \
  --OrgId=my-org \
  --WorkspaceID=my-workspace \
  --ProjectId=my-project \
  --ApiKey=krk_your_api_key_here \
  --EnvironmentId=my-environment \
  --Version=1.0.0
```

Parameters:
- `--EnvironmentId`: Environment ID or slug (required)

### Upload Package

(Not yet implemented)

```bash
dotnet run --project Kraken.Cli.csproj \
  --Action=UploadPackage \
  --OrgId=my-org \
  --WorkspaceID=my-workspace \
  --ProjectId=my-project \
  --ApiKey=krk_your_api_key_here \
  --File=/path/to/package.zip \
  --Version=1.0.0
```

## Examples

### Using a custom API URL

```bash
# Via parameter
dotnet run --project Kraken.Cli.csproj \
  --Action=CreateRelease \
  --BaseUrl=https://api.kraken.example.com \
  --OrgId=my-org \
  --WorkspaceID=my-workspace \
  --ProjectId=my-project \
  --ApiKey=krk_your_api_key_here \
  --Version=1.0.0 \
  --Packages="artifact1=1.0.0"

# Via environment variable
export KRAKEN_API_URL=https://api.kraken.example.com
dotnet run --project Kraken.Cli.csproj \
  --Action=CreateRelease \
  --OrgId=my-org \
  --WorkspaceID=my-workspace \
  --ProjectId=my-project \
  --ApiKey=krk_your_api_key_here \
  --Version=1.0.0 \
  --Packages="artifact1=1.0.0"
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
