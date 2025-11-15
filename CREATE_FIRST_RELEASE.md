# Step-by-Step: Create First Kraken CLI Release

## Execute These Commands

Run these commands in order to create your first CLI release:

### Step 1: Navigate to kraken-cli repository
```bash
cd C:\Users\Sebastian\Documents\Work\Kraken\kraken-cli
```

### Step 2: Check current status
```bash
git status
```

### Step 3: Add the workflow files (if not already committed)
```bash
git add .github\workflows\*.yml
git add README.md
git status
```

### Step 4: Commit the workflow files
```bash
git commit -m "Add GitHub Actions workflows for CLI release"
```

### Step 5: Push to GitHub
```bash
git push
```

### Step 6: Create and push the v1.0.0 tag
```bash
git tag v1.0.0
git push origin v1.0.0
```

---

## What Happens Next

1. **Workflow Triggers**: Pushing the tag `v1.0.0` will trigger the `release-cli.yml` workflow
2. **Build Process**: GitHub Actions will:
   - Build the CLI for Linux x64, Windows x64, macOS x64, and macOS ARM64
   - Create 4 executable files
   - Create a GitHub release tagged `v1.0.0`
   - Upload all 4 executables as release assets
3. **Timeline**: Should complete in 2-3 minutes

---

## Monitor the Release

After pushing the tag, check the progress:

1. **Go to**: https://github.com/krakendeploy-com/kraken-cli/actions
2. **Look for**: "Release Kraken CLI" workflow running
3. **Wait for**: Green checkmark (success)

---

## Verify the Release

Once complete, verify the release exists:

1. **Go to**: https://github.com/krakendeploy-com/kraken-cli/releases
2. **You should see**: Release `v1.0.0` with these files:
   - `kraken-cli-linux-x64`
   - `kraken-cli-win-x64.exe`
   - `kraken-cli-osx-x64`
   - `kraken-cli-osx-arm64`

---

## Test the Download

Test that the CLI can be downloaded:

```bash
curl -L -o test-cli https://github.com/krakendeploy-com/kraken-cli/releases/latest/download/kraken-cli-linux-x64
```

Check the file:
```bash
file test-cli
```

Should output something like:
```
test-cli: ELF 64-bit LSB executable, x86-64
```

---

## After Release is Complete

Once the release is successful, you can:

1. **Re-run your failed workflow** in the kraken repository
2. **Or push a new commit** to trigger it again

The CLI will now download successfully and your workflows will work!

---

## Alternative: Manual Workflow Trigger

If you prefer not to use tags, you can manually trigger the release:

1. Go to: https://github.com/krakendeploy-com/kraken-cli/actions
2. Click on "Release Kraken CLI" workflow
3. Click "Run workflow" button
4. Enter version: `v1.0.0`
5. Click "Run workflow"

This does the same thing as pushing a tag.

---

## Troubleshooting

### If the workflow fails:

1. **Check the logs** in GitHub Actions
2. **Common issues**:
   - .NET SDK not found (shouldn't happen on ubuntu-latest)
   - Project path wrong (check it's `src/Kraken.Cli/Kraken.Cli.csproj`)
   - Permission issues (workflow has `contents: write` permission)

### If files don't appear in the release:

- Check the workflow completed all steps
- Verify the "Create Release" step succeeded
- Look at the "Rename executables" step output

---

## Quick Command Summary

Copy and paste these commands all at once:

```bash
cd C:\Users\Sebastian\Documents\Work\Kraken\kraken-cli
git add .
git commit -m "Add GitHub Actions workflows for CLI release"
git push
git tag v1.0.0
git push origin v1.0.0
```

Then wait ~3 minutes and check: https://github.com/krakendeploy-com/kraken-cli/releases

---

## Next Steps After Release

1. ✅ Verify release exists at /releases/latest
2. ✅ Go back to your kraken repository
3. ✅ Re-run the failed workflow or push a new commit
4. ✅ The CLI download will now succeed!

