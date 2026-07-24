# Repository Cleanup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Organize project documentation and local tooling while proving that every protected code, solution, configuration, test-result, and generated artifact remains unchanged.

**Architecture:** Perform a manifest-driven cleanup in the existing workspace. Capture immutable pre-cleanup baselines outside the repository, use Git-aware moves for tracked content, remove only the approved tracked `.superpowers` files, and block the commit unless the staged path set exactly matches a generated allowlist.

**Tech Stack:** Git, PowerShell, Markdown, draw.io XML, .NET solution metadata

---

## File Structure

### Files created

- `docs/README.md` — main documentation index and authority guide.
- `docs/archive/README.md` — archive status and usage rules.

### Files modified

- `.gitignore` — English comments and approved repository-hygiene rules.
- `README.md` — links to the new documentation structure.
- `docs/reference/project-structure/backend-and-application.md` — solution tree paths updated after its Git move.

### Files moved

- `HLD.md` → `docs/archive/architecture/legacy-high-level-design.md`
- `REQUIREMENTS.md` → `docs/archive/requirements/legacy-user-stories.md`
- `PROJECT_STRUCTURE.md` → `docs/reference/project-structure/backend-and-application.md`
- `PROJECT_STRUCTURE_WEBAPP.md` → `docs/reference/project-structure/webapp.md`
- `PROJECT_STRUCTURE_TEST.md` → `docs/reference/project-structure/tests.md`
- `2.Design/*` → `docs/archive/designs/2026-06/*`
- `Screenshots/*` → `docs/assets/screenshots/*`

### Existing untracked files included

- `docs/superpowers/plans/2026-06-28-project-rules.md`
- `docs/superpowers/specs/2026-06-28-project-rules-design.md`

### Tracked files removed

- The exact approved baseline of 145 tracked paths below `.superpowers/`.

### Protected files

- `HotelBooking.webapp/Pages/User/Owner/OwnerDashboard.razor`
- All six tracked solution files
- All 27 tracked files below `HotelBooking.webapp/scriptcs_bin/`
- Both tracked files below `HotelBooking.test/TestResults/`
- All tracked and local `appsettings*.json` files
- Every existing file below project `bin/`, `obj/`, and `Logs/` directories
- Every existing physical file below `HotelBooking.webapp/scriptcs_bin/`

## Task 1: Validate the Workspace and Capture the Baseline

**Files:**

- Read: repository working tree and Git index
- Create outside repository: `C:\tmp\hotel-booking-cleanup-baseline-2026-07-25\`

- [ ] **Step 1: Verify prerequisites and the exact workspace root**

Run:

```powershell
$requiredCommands = @('git', 'rg')
foreach ($command in $requiredCommands) {
    Get-Command $command -ErrorAction Stop | Out-Null
    Write-Output "REQUIRED_COMMAND=$command"
}

$repoRoot = (Resolve-Path '.').Path
$expectedRoot = 'E:\Cybersoft\FinalProject\Hotel_Blazor'
if ($repoRoot -ne $expectedRoot) {
    throw "Unexpected repository root: $repoRoot"
}
Write-Output "REPOSITORY_ROOT=$repoRoot"
```

Expected:

```text
REQUIRED_COMMAND=git
REQUIRED_COMMAND=rg
REPOSITORY_ROOT=E:\Cybersoft\FinalProject\Hotel_Blazor
```

- [ ] **Step 2: Refuse to proceed when the index already contains changes**

Run:

```powershell
$preExistingStaged = @(git diff --cached --name-only)
if ($LASTEXITCODE -ne 0) { throw 'Failed to inspect the Git index.' }
if ($preExistingStaged.Count -ne 0) {
    $preExistingStaged | ForEach-Object { Write-Output "PREEXISTING_STAGED=$_" }
    throw 'The Git index is not clean.'
}
git status --short
if ($LASTEXITCODE -ne 0) { throw 'Failed to inspect the initial Git status.' }
```

Expected status:

```text
 M HotelBooking.webapp/Pages/User/Owner/OwnerDashboard.razor
?? docs/superpowers/plans/2026-06-28-project-rules.md
?? docs/superpowers/specs/2026-06-28-project-rules-design.md
```

Stop if any additional staged, modified, deleted, or untracked path appears. Report it instead of changing it.

- [ ] **Step 3: Create a new external baseline directory**

Run:

```powershell
$baselineRoot = 'C:\tmp\hotel-booking-cleanup-baseline-2026-07-25'
if (Test-Path -LiteralPath $baselineRoot) {
    throw "Baseline directory already exists: $baselineRoot"
}
New-Item -ItemType Directory -Path $baselineRoot | Out-Null
Write-Output "BASELINE_ROOT=$baselineRoot"
```

Expected:

```text
BASELINE_ROOT=C:\tmp\hotel-booking-cleanup-baseline-2026-07-25
```

- [ ] **Step 4: Capture tracked path manifests**

Run:

```powershell
$baselineRoot = 'C:\tmp\hotel-booking-cleanup-baseline-2026-07-25'

git ls-files '.superpowers/**' |
    Sort-Object |
    Set-Content -Encoding utf8 "$baselineRoot\superpowers-tracked.txt"
if ($LASTEXITCODE -ne 0) { throw 'Failed to capture tracked .superpowers paths.' }

git ls-files '2.Design/**' |
    Sort-Object |
    Set-Content -Encoding utf8 "$baselineRoot\legacy-designs-tracked.txt"
if ($LASTEXITCODE -ne 0) { throw 'Failed to capture tracked legacy-design paths.' }

git ls-files 'Screenshots/**' |
    Sort-Object |
    Set-Content -Encoding utf8 "$baselineRoot\screenshots-tracked.txt"
if ($LASTEXITCODE -ne 0) { throw 'Failed to capture tracked screenshot paths.' }

$superpowersCount = @(Get-Content "$baselineRoot\superpowers-tracked.txt").Count
$legacyDesignCount = @(Get-Content "$baselineRoot\legacy-designs-tracked.txt").Count
$screenshotCount = @(Get-Content "$baselineRoot\screenshots-tracked.txt").Count

Write-Output "SUPERPOWERS_TRACKED=$superpowersCount"
Write-Output "LEGACY_DESIGNS_TRACKED=$legacyDesignCount"
Write-Output "SCREENSHOTS_TRACKED=$screenshotCount"

if ($superpowersCount -ne 145) { throw 'Unexpected .superpowers baseline.' }
if ($legacyDesignCount -ne 14) { throw 'Unexpected legacy-design baseline.' }
if ($screenshotCount -ne 46) { throw 'Unexpected screenshot baseline.' }
```

Expected:

```text
SUPERPOWERS_TRACKED=145
LEGACY_DESIGNS_TRACKED=14
SCREENSHOTS_TRACKED=46
```

- [ ] **Step 5: Prove that `.superpowers` has no unexpected physical descendants**

Run:

```powershell
$repoRoot = (Resolve-Path '.').Path
$baselineRoot = 'C:\tmp\hotel-booking-cleanup-baseline-2026-07-25'

$trackedSuperpowers = @(
    Get-Content "$baselineRoot\superpowers-tracked.txt" |
    ForEach-Object { $_.Replace('/', '\') }
)

$physicalSuperpowers = @(
    Get-ChildItem -LiteralPath '.superpowers' -Recurse -File -Force |
    ForEach-Object {
        $_.FullName.Substring($repoRoot.Length + 1)
    } |
    Sort-Object
)

$superpowersDifference = @(
    Compare-Object $trackedSuperpowers $physicalSuperpowers
)

if ($superpowersDifference.Count -ne 0) {
    $superpowersDifference | Format-Table -AutoSize
    throw 'Unexpected tracked, untracked, or ignored content exists under .superpowers.'
}

Write-Output "SUPERPOWERS_PHYSICAL_MATCH=True"

$expectedSuperpowersDirectories = @('.superpowers')
foreach ($path in $trackedSuperpowers) {
    $parent = Split-Path -Parent $path
    while ($parent -and $parent.StartsWith('.superpowers')) {
        $expectedSuperpowersDirectories += $parent
        $parent = Split-Path -Parent $parent
    }
}
$expectedSuperpowersDirectories = @(
    $expectedSuperpowersDirectories | Sort-Object -Unique
)

$physicalSuperpowersDirectories = @('.superpowers') + @(
    Get-ChildItem -LiteralPath '.superpowers' -Recurse -Directory -Force |
    ForEach-Object { $_.FullName.Substring($repoRoot.Length + 1) }
) | Sort-Object -Unique

$unexpectedSuperpowersDirectories = @(
    Compare-Object `
        $expectedSuperpowersDirectories `
        $physicalSuperpowersDirectories
)

$superpowersReparsePoints = @(
    Get-ChildItem -LiteralPath '.superpowers' -Recurse -Force |
    Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint }
)

if ($unexpectedSuperpowersDirectories.Count -ne 0) {
    $unexpectedSuperpowersDirectories | Format-Table -AutoSize
    throw 'Unexpected empty or additional directory exists under .superpowers.'
}
if ($superpowersReparsePoints.Count -ne 0) {
    $superpowersReparsePoints | Select-Object FullName, Attributes
    throw 'A reparse point exists under .superpowers.'
}

Write-Output "SUPERPOWERS_DIRECTORY_MATCH=True"
Write-Output "SUPERPOWERS_REPARSE_POINTS=0"
```

Expected:

```text
SUPERPOWERS_PHYSICAL_MATCH=True
SUPERPOWERS_DIRECTORY_MATCH=True
SUPERPOWERS_REPARSE_POINTS=0
```

Stop if this check fails. Do not remove any `.superpowers` content.

- [ ] **Step 6: Capture Git blob IDs for protected tracked files**

Run:

```powershell
$baselineRoot = 'C:\tmp\hotel-booking-cleanup-baseline-2026-07-25'

$solutionPaths = @(git ls-files '*.sln')
if ($LASTEXITCODE -ne 0) { throw 'Failed to list tracked solution files.' }
$scriptcsPaths = @(git ls-files 'HotelBooking.webapp/scriptcs_bin/**')
if ($LASTEXITCODE -ne 0) { throw 'Failed to list tracked scriptcs files.' }
$testResultPaths = @(git ls-files 'HotelBooking.test/TestResults/**')
if ($LASTEXITCODE -ne 0) { throw 'Failed to list tracked test-result files.' }
$appSettingsPaths = @(git ls-files '*/appsettings*.json')
if ($LASTEXITCODE -ne 0) { throw 'Failed to list tracked appsettings files.' }

$protectedTrackedPaths = @(
    $solutionPaths
    $scriptcsPaths
    $testResultPaths
    $appSettingsPaths
) | Sort-Object -Unique

$protectedTrackedRows = foreach ($path in $protectedTrackedPaths) {
    $blob = git hash-object -- $path
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to hash protected tracked file: $path"
    }
    [pscustomobject]@{
        Path = $path
        Blob = $blob
    }
}

$protectedTrackedRows |
    Export-Csv -NoTypeInformation -Encoding utf8 "$baselineRoot\protected-tracked.csv"

$solutionCount = @($protectedTrackedPaths | Where-Object { $_ -like '*.sln' }).Count
$scriptcsCount = @($protectedTrackedPaths | Where-Object { $_ -like 'HotelBooking.webapp/scriptcs_bin/*' }).Count
$testResultCount = @($protectedTrackedPaths | Where-Object { $_ -like 'HotelBooking.test/TestResults/*' }).Count
$trackedAppSettingsCount = @($protectedTrackedPaths | Where-Object { $_ -like '*/appsettings*.json' }).Count

Write-Output "SOLUTIONS=$solutionCount"
Write-Output "SCRIPTCS_TRACKED=$scriptcsCount"
Write-Output "TEST_RESULTS_TRACKED=$testResultCount"
Write-Output "APPSETTINGS_TRACKED=$trackedAppSettingsCount"

if ($solutionCount -ne 6) { throw 'Unexpected solution baseline.' }
if ($scriptcsCount -ne 27) { throw 'Unexpected scriptcs baseline.' }
if ($testResultCount -ne 2) { throw 'Unexpected test-result baseline.' }
if ($trackedAppSettingsCount -ne 5) { throw 'Unexpected appsettings baseline.' }
```

Expected:

```text
SOLUTIONS=6
SCRIPTCS_TRACKED=27
TEST_RESULTS_TRACKED=2
APPSETTINGS_TRACKED=5
```

- [ ] **Step 7: Capture SHA-256 hashes for protected physical artifacts**

Run:

```powershell
$repoRoot = (Resolve-Path '.').Path
$baselineRoot = 'C:\tmp\hotel-booking-cleanup-baseline-2026-07-25'

$generatedRoots = @(
    'HotelBooking.api\bin',
    'HotelBooking.api\obj',
    'HotelBooking.api\Logs',
    'HotelBooking.application\bin',
    'HotelBooking.application\obj',
    'HotelBooking.application\Logs',
    'HotelBooking.infrastructure\bin',
    'HotelBooking.infrastructure\obj',
    'HotelBooking.infrastructure\Logs',
    'HotelBooking.test\bin',
    'HotelBooking.test\obj',
    'HotelBooking.test\Logs',
    'HotelBooking.webapp\bin',
    'HotelBooking.webapp\obj',
    'HotelBooking.webapp\Logs',
    'HotelBooking.webapp\scriptcs_bin'
)

$generatedFiles = foreach ($root in $generatedRoots) {
    if (Test-Path -LiteralPath $root) {
        Get-ChildItem -LiteralPath $root -Recurse -File -Force
    }
}

$localAppSettings = Get-ChildItem -Recurse -File -Force -Filter 'appsettings*.json' |
    Where-Object { $_.FullName -notlike '*\.git\*' }

$protectedPhysicalFiles = @($generatedFiles) + @($localAppSettings) |
    Sort-Object FullName -Unique

$protectedPhysicalRows = foreach ($file in $protectedPhysicalFiles) {
    [pscustomobject]@{
        Path = $file.FullName.Substring($repoRoot.Length + 1)
        Length = $file.Length
        Sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
}

$protectedPhysicalRows |
    Export-Csv -NoTypeInformation -Encoding utf8 "$baselineRoot\protected-physical.csv"

Write-Output "PROTECTED_PHYSICAL_FILES=$($protectedPhysicalRows.Count)"
Write-Output "PROTECTED_PHYSICAL_BYTES=$(($protectedPhysicalRows | Measure-Object Length -Sum).Sum)"
```

Expected: a positive file count and byte count. Preserve these exact values for the post-cleanup comparison.

- [ ] **Step 8: Capture the original status**

Run:

```powershell
$baselineRoot = 'C:\tmp\hotel-booking-cleanup-baseline-2026-07-25'

git status --porcelain=v1 |
    Set-Content -Encoding utf8 "$baselineRoot\status-before.txt"
if ($LASTEXITCODE -ne 0) { throw 'Failed to capture the initial Git status.' }
Get-Content "$baselineRoot\status-before.txt"

$ownerDashboardPath = 'HotelBooking.webapp\Pages\User\Owner\OwnerDashboard.razor'
$ownerDashboardHash = (
    Get-FileHash -LiteralPath $ownerDashboardPath -Algorithm SHA256
).Hash
$ownerDashboardHash |
    Set-Content -Encoding utf8 "$baselineRoot\owner-dashboard.sha256"
Write-Output "OWNER_DASHBOARD_SHA256=$ownerDashboardHash"

$ignoredRootEntries = foreach ($entry in Get-ChildItem -Force) {
    if ($entry.Name -eq '.git') { continue }
    git check-ignore -q --no-index -- $entry.Name
    $ignoreExitCode = $LASTEXITCODE
    if ($ignoreExitCode -gt 1) {
        throw "Failed to inspect ignore state for: $($entry.Name)"
    }
    if ($ignoreExitCode -eq 0) { $entry.Name }
}
$ignoredRootEntries |
    Sort-Object |
    Set-Content -Encoding utf8 "$baselineRoot\ignored-root-before.txt"
$ignoredRootEntries |
    Sort-Object |
    ForEach-Object { Write-Output "IGNORED_ROOT_ENTRY=$_" }
```

Expected:

```text
 M HotelBooking.webapp/Pages/User/Owner/OwnerDashboard.razor
?? docs/superpowers/plans/2026-06-28-project-rules.md
?? docs/superpowers/specs/2026-06-28-project-rules-design.md
OWNER_DASHBOARD_SHA256=<64-character SHA-256 value>
IGNORED_ROOT_ENTRY=.agents
IGNORED_ROOT_ENTRY=.vscode
```

The SHA-256 value is recorded evidence, not a value to type manually. Unexpected ignored root entries are reported and preserved.

## Task 2: Create Documentation Indexes

**Files:**

- Create: `docs/README.md`
- Create: `docs/archive/README.md`

- [ ] **Step 1: Create the main documentation index**

Use `apply_patch` to create `docs/README.md` with exactly:

```markdown
# Hotel Booking Documentation

This directory contains architecture, requirements, workflow, reference, historical, and evidence documentation for the Hotel Booking Platform.

## Source of Truth

| Area | Authoritative source |
|---|---|
| High-Level Architecture | [High-Level Design](architecture/high-level-design.md) |
| Architecture Diagrams | [Platform HLD draw.io source](architecture/diagrams/01-platform-hld.drawio) |
| Detailed Requirements | No authoritative detailed catalog exists yet |

The target User Story and Acceptance Criteria catalog is planned work following approval of the HLD. The archived requirements are historical input only.

## Workflow Specifications and Plans

- [Workflow specifications](superpowers/specs/)
- [Implementation plans](superpowers/plans/)

These documents capture approved designs and execution plans. A completed plan does not automatically become an authoritative product requirement.

## Reference Documentation

- [Backend and Application structure](reference/project-structure/backend-and-application.md)
- [Web App structure](reference/project-structure/webapp.md)
- [Test structure](reference/project-structure/tests.md)

Reference documents describe the repository at a point in time and may require updates after structural changes.

## Historical Documentation

- [Archive index](archive/README.md)
- [Legacy High-Level Design](archive/architecture/legacy-high-level-design.md)
- [Legacy User Stories and Acceptance Criteria](archive/requirements/legacy-user-stories.md)
- [Legacy feature designs](archive/designs/2026-06/)

Archived documents are not the current source of truth.

## Evidence

- [Feature screenshots](assets/screenshots/)
```

- [ ] **Step 2: Create the archive index**

Use `apply_patch` to create `docs/archive/README.md` with exactly:

```markdown
# Documentation Archive

This directory preserves historical project documents for context, traceability, and graduation-project evidence.

Archived documents:

- May describe outdated behavior, structure, or architecture.
- Must not be treated as the current source of truth.
- Must not receive new product requirements.
- May be referenced when migrating requirements into an approved current specification.

## Contents

- [Legacy High-Level Design](architecture/legacy-high-level-design.md)
- [Legacy User Stories and Acceptance Criteria](requirements/legacy-user-stories.md)
- [Legacy feature designs from June 2026](designs/2026-06/)
```

- [ ] **Step 3: Verify the new indexes**

Run:

```powershell
Get-Content -Raw 'docs\README.md'
Get-Content -Raw 'docs\archive\README.md'
```

Expected: both files contain the exact English content above and no incomplete marker text.

## Task 3: Move Historical and Reference Documentation

**Files:**

- Move: five root Markdown documents
- Move: 14 files below `2.Design/`

- [ ] **Step 1: Create exact destination directories**

Run:

```powershell
$destinationDirectories = @(
    'docs\archive\architecture',
    'docs\archive\requirements',
    'docs\archive\designs',
    'docs\reference\project-structure'
)

foreach ($directory in $destinationDirectories) {
    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory | Out-Null
    }
}
```

Expected: all four destination directories exist.

- [ ] **Step 2: Move the legacy HLD and requirements**

Run:

```powershell
git mv -- HLD.md docs/archive/architecture/legacy-high-level-design.md
if ($LASTEXITCODE -ne 0) { throw 'Failed to move HLD.md.' }
git mv -- REQUIREMENTS.md docs/archive/requirements/legacy-user-stories.md
if ($LASTEXITCODE -ne 0) { throw 'Failed to move REQUIREMENTS.md.' }
```

Expected: the two root files are absent and both destination files exist.

- [ ] **Step 3: Move project-structure references**

Run:

```powershell
git mv -- PROJECT_STRUCTURE.md docs/reference/project-structure/backend-and-application.md
if ($LASTEXITCODE -ne 0) { throw 'Failed to move PROJECT_STRUCTURE.md.' }
git mv -- PROJECT_STRUCTURE_WEBAPP.md docs/reference/project-structure/webapp.md
if ($LASTEXITCODE -ne 0) { throw 'Failed to move PROJECT_STRUCTURE_WEBAPP.md.' }
git mv -- PROJECT_STRUCTURE_TEST.md docs/reference/project-structure/tests.md
if ($LASTEXITCODE -ne 0) { throw 'Failed to move PROJECT_STRUCTURE_TEST.md.' }
```

Expected: the three root files are absent and all three destination files exist.

- [ ] **Step 4: Update the moved repository tree**

Use `apply_patch` to replace the obsolete repository tree in
`docs/reference/project-structure/backend-and-application.md`:

```text
Hotel_Blazor/                          # Solution root (.NET 9)
├── HotelBooking.api/                  # ASP.NET Core Web API (entry point)
├── HotelBooking.application/          # Business logic layer (Clean Architecture)
├── HotelBooking.infrastructure/       # Data access layer (EF Core, Repositories)
├── HotelBooking.webapp/               # Blazor Server frontend
├── HotelBooking.test/                 # xUnit test project
├── Scripts/                           # SQL scripts and migration helpers
├── Screenshots/                       # Feature screenshots for documentation
├── PROJECT_STRUCTURE.md               # ← This file
├── PROJECT_STRUCTURE_WEBAPP.md        # Frontend structure details
├── PROJECT_STRUCTURE_TEST.md          # Test layer structure details
└── Hotel_Blazor.sln
```

with:

```text
Hotel_Blazor/                          # Solution root (.NET 9)
├── HotelBooking.api/                  # ASP.NET Core Web API (entry point)
├── HotelBooking.application/          # Business logic layer (Clean Architecture)
├── HotelBooking.infrastructure/       # Data access layer (EF Core, Repositories)
├── HotelBooking.webapp/               # Blazor Server frontend
├── HotelBooking.test/                 # xUnit test project
├── Scripts/                           # SQL scripts and migration helpers
├── docs/
│   ├── architecture/                  # Authoritative architecture
│   ├── reference/project-structure/
│   ├── archive/                       # Historical documents
│   └── assets/screenshots/            # Project screenshots
├── README.md
└── HotelBooking.sln                   # Root solution file
```

Do not change any other content in this historical reference document.

- [ ] **Step 5: Move all legacy feature designs**

Run:

```powershell
git mv -- 2.Design docs/archive/designs/2026-06
if ($LASTEXITCODE -ne 0) { throw 'Failed to move 2.Design.' }
```

Expected:

```powershell
$movedDesignCount = @(
    Get-ChildItem -LiteralPath 'docs\archive\designs\2026-06' -File
).Count
Write-Output "MOVED_DESIGNS=$movedDesignCount"
if ($movedDesignCount -ne 14) {
    throw 'Legacy design count changed during move.'
}
```

Expected output:

```text
MOVED_DESIGNS=14
```

## Task 4: Move Screenshot Evidence

**Files:**

- Move: 46 tracked files below `Screenshots/`

- [ ] **Step 1: Create the asset parent directory**

Run:

```powershell
if (-not (Test-Path -LiteralPath 'docs\assets')) {
    New-Item -ItemType Directory -Path 'docs\assets' | Out-Null
}
```

- [ ] **Step 2: Move the screenshot tree**

Run:

```powershell
git mv -- Screenshots docs/assets/screenshots
if ($LASTEXITCODE -ne 0) { throw 'Failed to move Screenshots.' }
```

- [ ] **Step 3: Verify screenshot preservation**

Run:

```powershell
$movedScreenshotFiles = @(
    Get-ChildItem -LiteralPath 'docs\assets\screenshots' -Recurse -File -Force
)
Write-Output "MOVED_SCREENSHOTS=$($movedScreenshotFiles.Count)"
if ($movedScreenshotFiles.Count -ne 46) {
    throw 'Screenshot count changed during move.'
}
```

Expected:

```text
MOVED_SCREENSHOTS=46
```

The empty `Search/` and `Tests/` directories are not recreated.

## Task 5: Remove the Approved Vendored Tool Files

**Files:**

- Remove: the exact 145 tracked `.superpowers/**` baseline paths

- [ ] **Step 1: Repeat the physical-versus-tracked safety check**

Repeat Task 1 Step 5 immediately before deletion.

Expected:

```text
SUPERPOWERS_PHYSICAL_MATCH=True
SUPERPOWERS_DIRECTORY_MATCH=True
SUPERPOWERS_REPARSE_POINTS=0
```

- [ ] **Step 2: Remove only the validated tracked tree**

Run:

```powershell
git rm -r -- .superpowers
if ($LASTEXITCODE -ne 0) { throw 'Failed to remove the validated .superpowers tree.' }
```

Expected:

- Git reports removal of the approved tracked files.
- `git ls-files '.superpowers/**'` returns no paths.
- The physical directory is absent because the preflight proved it contained no additional files.

- [ ] **Step 3: Verify no unrelated deletion**

Run:

```powershell
$deletedPaths = @(
    git diff --cached --diff-filter=D --name-only
)
if ($LASTEXITCODE -ne 0) { throw 'Failed to inspect staged deletions.' }

$allowedDeletedPrefixes = @(
    '.superpowers/',
    'HLD.md',
    'REQUIREMENTS.md',
    'PROJECT_STRUCTURE.md',
    'PROJECT_STRUCTURE_WEBAPP.md',
    'PROJECT_STRUCTURE_TEST.md',
    '2.Design/',
    'Screenshots/'
)

$unexpectedDeletedPaths = foreach ($path in $deletedPaths) {
    $allowed = $false
    foreach ($prefix in $allowedDeletedPrefixes) {
        if ($path -eq $prefix -or $path.StartsWith($prefix)) {
            $allowed = $true
            break
        }
    }
    if (-not $allowed) { $path }
}

if (@($unexpectedDeletedPaths).Count -ne 0) {
    $unexpectedDeletedPaths
    throw 'Unexpected deletion detected.'
}
```

Expected: no unexpected path and exit code `0`.

## Task 6: Update `.gitignore` and Root Documentation Links

**Files:**

- Modify: `.gitignore`
- Modify: `README.md:71`

- [ ] **Step 1: Replace `.gitignore` with the approved content**

Use `apply_patch` to make `.gitignore` exactly:

```gitignore
# Build outputs
bin/
obj/
[Bb]uild/
artifacts/
[Ll]og/
[Ll]ogs/

# Visual Studio and VS Code
.vs/
.vscode/
.history/
*.user
*.suo
*.userosscache
*.sln.docstates

# JetBrains Rider
.idea/
*.iml

# Local AI-agent tooling
.agents/
.superpowers/

# ASP.NET Core local configuration
appsettings.Development.json
appsettings.*.json

# Entity Framework Core database-first workflow
migrations/

# NuGet
*.nupkg
*.snupkg
*.nuspec
packages/
project.lock.json
project.fragment.lock.json
project.assets.json
*.nuget.props
*.nuget.targets
*.nuget.cache

# Cache and temporary files
*.dbmdl
*.bak
*.log
*.tlog
*.tmp
*.temp
*.cache
*.binlog
*.orig
*.rej
*.vspscc
*.vssscc
*.vshost.exe*
*.pdb
*.mdb

# Test, coverage, benchmark, and analysis outputs
TestResults/
coverage/
coverage-report/
*.coverage
*.coveragexml
BenchmarkDotNet.Artifacts/
.sonarqube/

# Publish outputs
[Pp]ublish/
*.Publish.xml

# Operating-system files
.DS_Store
Thumbs.db
Desktop.ini
```

- [ ] **Step 2: Update the README documentation links**

Use `apply_patch` to replace the block at `README.md:71-75` with:

```markdown
> 📖 **Project documentation:**
>
> - [Documentation Index](docs/README.md)
> - [Authoritative High-Level Design](docs/architecture/high-level-design.md)
> - [Backend & Application Structure](docs/reference/project-structure/backend-and-application.md)
> - [Frontend (Blazor) Structure](docs/reference/project-structure/webapp.md)
> - [Test Structure](docs/reference/project-structure/tests.md)
> - [Legacy Requirements — Historical Only](docs/archive/requirements/legacy-user-stories.md)
```

Do not modify the existing `Scripts/` database link or unrelated README content.

- [ ] **Step 3: Verify the approved ignore rules**

Run:

```powershell
git check-ignore -v --no-index -- .agents/probe .superpowers/probe
if ($LASTEXITCODE -ne 0) { throw 'Expected ignore rules did not match both probes.' }
```

Expected: both paths are matched by their explicit `.gitignore` rules.

- [ ] **Step 4: Verify tracked project standards remain tracked**

Run:

```powershell
$standards = @('AGENTS.md', 'CLAUDE.md', 'GEMINI.md')
foreach ($standard in $standards) {
    git ls-files --error-unmatch -- $standard | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Tracked project standard is missing: $standard"
    }
    Write-Output "TRACKED_STANDARD=$standard"
}
```

Expected:

```text
TRACKED_STANDARD=AGENTS.md
TRACKED_STANDARD=CLAUDE.md
TRACKED_STANDARD=GEMINI.md
```

## Task 7: Verify Documentation and Protected Artifacts

**Files:**

- Read: all moved and protected paths
- Read outside repository: baseline CSV and text files

- [ ] **Step 1: Verify required destination counts**

Run:

```powershell
$legacyDesignCount = @(
    Get-ChildItem 'docs\archive\designs\2026-06' -File
).Count

$screenshotCount = @(
    Get-ChildItem 'docs\assets\screenshots' -Recurse -File -Force
).Count

$referenceCount = @(
    Get-ChildItem 'docs\reference\project-structure' -File
).Count

Write-Output "LEGACY_DESIGNS=$legacyDesignCount"
Write-Output "SCREENSHOTS=$screenshotCount"
Write-Output "PROJECT_REFERENCES=$referenceCount"

if ($legacyDesignCount -ne 14) { throw 'Legacy designs are incomplete.' }
if ($screenshotCount -ne 46) { throw 'Screenshots are incomplete.' }
if ($referenceCount -ne 3) { throw 'Project references are incomplete.' }
```

Expected:

```text
LEGACY_DESIGNS=14
SCREENSHOTS=46
PROJECT_REFERENCES=3
```

- [ ] **Step 2: Verify required individual files**

Run:

```powershell
$requiredFiles = @(
    'docs\README.md',
    'docs\archive\README.md',
    'docs\architecture\high-level-design.md',
    'docs\architecture\diagrams\01-platform-hld.drawio',
    'docs\archive\architecture\legacy-high-level-design.md',
    'docs\archive\requirements\legacy-user-stories.md',
    'docs\reference\project-structure\backend-and-application.md',
    'docs\reference\project-structure\webapp.md',
    'docs\reference\project-structure\tests.md',
    'docs\superpowers\plans\2026-06-28-project-rules.md',
    'docs\superpowers\specs\2026-06-28-project-rules-design.md'
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path -LiteralPath $file)) {
        throw "Required file missing: $file"
    }
    Write-Output "REQUIRED_FILE=$file"
}
```

Expected: all 11 required files are listed.

- [ ] **Step 3: Verify the draw.io source**

Run:

```powershell
$drawio = [xml](
    Get-Content -Raw 'docs\architecture\diagrams\01-platform-hld.drawio'
)
$drawioPages = @($drawio.mxfile.diagram)
Write-Output "DRAWIO_PAGES=$($drawioPages.Count)"
$drawioPages | ForEach-Object { Write-Output "DRAWIO_PAGE=$($_.name)" }
if ($drawioPages.Count -ne 4) {
    throw 'The HLD draw.io page count changed.'
}
```

Expected:

```text
DRAWIO_PAGES=4
DRAWIO_PAGE=System Context
DRAWIO_PAGE=Container Architecture
DRAWIO_PAGE=Application Modules
DRAWIO_PAGE=Deployment View
```

- [ ] **Step 4: Verify the root solution**

Run:

```powershell
$solutionProjects = @(
    Select-String -Path 'HotelBooking.sln' -Pattern '^Project\('
)
Write-Output "ROOT_SOLUTION_PROJECTS=$($solutionProjects.Count)"
if ($solutionProjects.Count -ne 5) {
    throw 'The root solution project count changed.'
}
```

Expected:

```text
ROOT_SOLUTION_PROJECTS=5
```

- [ ] **Step 5: Verify that no live documentation uses an old path**

Run:

```powershell
$oldPathPattern = 'HLD\.md|REQUIREMENTS\.md|2\.Design/|Screenshots/|PROJECT_STRUCTURE(_WEBAPP|_TEST)?\.md'

$liveReferences = @(
    rg -n $oldPathPattern . `
        -g '*.md' `
        -g '!.agents/**' `
        -g '!docs/superpowers/specs/2026-07-25-repository-cleanup-design.md' `
        -g '!docs/superpowers/plans/2026-07-25-repository-cleanup.md'
)
$referenceScanExitCode = $LASTEXITCODE
if ($referenceScanExitCode -gt 1) {
    throw "Old-path reference scan failed with exit code $referenceScanExitCode."
}

if ($liveReferences.Count -ne 0) {
    $liveReferences
    throw 'A Markdown document still references an old path.'
}

Write-Output 'LIVE_OLD_PATH_REFERENCES=0'
```

Expected:

```text
LIVE_OLD_PATH_REFERENCES=0
```

Only the approved cleanup design and this implementation plan are excluded because they intentionally record the old paths. Archived Markdown is scanned too.

- [ ] **Step 6: Compare protected tracked Git blobs**

Run:

```powershell
$baselineRoot = 'C:\tmp\hotel-booking-cleanup-baseline-2026-07-25'

$expectedProtectedTracked = @(
    Import-Csv "$baselineRoot\protected-tracked.csv"
)

$currentSolutionPaths = @(git ls-files '*.sln')
if ($LASTEXITCODE -ne 0) { throw 'Failed to list current tracked solution files.' }
$currentScriptcsPaths = @(git ls-files 'HotelBooking.webapp/scriptcs_bin/**')
if ($LASTEXITCODE -ne 0) { throw 'Failed to list current tracked scriptcs files.' }
$currentTestResultPaths = @(git ls-files 'HotelBooking.test/TestResults/**')
if ($LASTEXITCODE -ne 0) { throw 'Failed to list current tracked test-result files.' }
$currentAppSettingsPaths = @(git ls-files '*/appsettings*.json')
if ($LASTEXITCODE -ne 0) { throw 'Failed to list current tracked appsettings files.' }

$currentProtectedTrackedPaths = @(
    $currentSolutionPaths
    $currentScriptcsPaths
    $currentTestResultPaths
    $currentAppSettingsPaths
) | Sort-Object -Unique

$actualProtectedTracked = foreach ($path in $currentProtectedTrackedPaths) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Protected tracked file missing from the worktree: $path"
    }
    $blob = git hash-object -- $path
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to hash protected tracked file: $path"
    }
    [pscustomobject]@{
        Path = $path
        Blob = $blob
    }
}

$trackedDifference = @(
    Compare-Object `
        ($expectedProtectedTracked | ConvertTo-Csv -NoTypeInformation) `
        ($actualProtectedTracked | ConvertTo-Csv -NoTypeInformation)
)

if ($trackedDifference.Count -ne 0) {
    $trackedDifference
    throw 'A protected tracked file changed.'
}

Write-Output 'PROTECTED_TRACKED_MATCH=True'
```

Expected:

```text
PROTECTED_TRACKED_MATCH=True
```

- [ ] **Step 7: Compare protected physical artifact hashes**

Run:

```powershell
$repoRoot = (Resolve-Path '.').Path
$baselineRoot = 'C:\tmp\hotel-booking-cleanup-baseline-2026-07-25'

$expectedProtectedPhysical = @(
    Import-Csv "$baselineRoot\protected-physical.csv"
)

$generatedRoots = @(
    'HotelBooking.api\bin',
    'HotelBooking.api\obj',
    'HotelBooking.api\Logs',
    'HotelBooking.application\bin',
    'HotelBooking.application\obj',
    'HotelBooking.application\Logs',
    'HotelBooking.infrastructure\bin',
    'HotelBooking.infrastructure\obj',
    'HotelBooking.infrastructure\Logs',
    'HotelBooking.test\bin',
    'HotelBooking.test\obj',
    'HotelBooking.test\Logs',
    'HotelBooking.webapp\bin',
    'HotelBooking.webapp\obj',
    'HotelBooking.webapp\Logs',
    'HotelBooking.webapp\scriptcs_bin'
)

$generatedFiles = foreach ($root in $generatedRoots) {
    if (Test-Path -LiteralPath $root) {
        Get-ChildItem -LiteralPath $root -Recurse -File -Force
    }
}

$localAppSettings = Get-ChildItem -Recurse -File -Force -Filter 'appsettings*.json' |
    Where-Object { $_.FullName -notlike '*\.git\*' }

$protectedPhysicalFiles = @($generatedFiles) + @($localAppSettings) |
    Sort-Object FullName -Unique

$actualProtectedPhysical = foreach ($file in $protectedPhysicalFiles) {
    [pscustomobject]@{
        Path = $file.FullName.Substring($repoRoot.Length + 1)
        Length = [string]$file.Length
        Sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
}

$physicalDifference = @(
    Compare-Object `
        ($expectedProtectedPhysical | ConvertTo-Csv -NoTypeInformation) `
        ($actualProtectedPhysical | ConvertTo-Csv -NoTypeInformation)
)

if ($physicalDifference.Count -ne 0) {
    $physicalDifference
    throw 'A protected physical artifact changed.'
}

Write-Output 'PROTECTED_PHYSICAL_MATCH=True'
```

Expected:

```text
PROTECTED_PHYSICAL_MATCH=True
```

- [ ] **Step 8: Verify the unfinished code change remains unstaged**

Run:

```powershell
$baselineRoot = 'C:\tmp\hotel-booking-cleanup-baseline-2026-07-25'
$ownerDashboardPath = 'HotelBooking.webapp\Pages\User\Owner\OwnerDashboard.razor'
$expectedOwnerDashboardHash = (
    Get-Content -Raw "$baselineRoot\owner-dashboard.sha256"
).Trim()
$actualOwnerDashboardHash = (
    Get-FileHash -LiteralPath $ownerDashboardPath -Algorithm SHA256
).Hash

$ownerDashboardStatus = @(
    git status --short -- $ownerDashboardPath
)
if ($LASTEXITCODE -ne 0) { throw 'Failed to inspect Owner Dashboard status.' }
$ownerDashboardStaged = @(
    git diff --cached --name-only -- $ownerDashboardPath
)
if ($LASTEXITCODE -ne 0) { throw 'Failed to inspect Owner Dashboard staged state.' }

Write-Output "OWNER_DASHBOARD_HASH_MATCH=$($actualOwnerDashboardHash -eq $expectedOwnerDashboardHash)"
Write-Output "OWNER_DASHBOARD_STATUS=$ownerDashboardStatus"
Write-Output "OWNER_DASHBOARD_STAGED_COUNT=$($ownerDashboardStaged.Count)"

if ($actualOwnerDashboardHash -ne $expectedOwnerDashboardHash) {
    throw 'The Owner Dashboard content changed after baseline capture.'
}
if ($ownerDashboardStatus -ne ' M HotelBooking.webapp/Pages/User/Owner/OwnerDashboard.razor') {
    throw 'The Owner Dashboard working-tree state changed.'
}
if ($ownerDashboardStaged.Count -ne 0) {
    throw 'The Owner Dashboard was staged.'
}
```

Expected:

```text
OWNER_DASHBOARD_HASH_MATCH=True
OWNER_DASHBOARD_STATUS= M HotelBooking.webapp/Pages/User/Owner/OwnerDashboard.razor
OWNER_DASHBOARD_STAGED_COUNT=0
```

## Task 8: Stage with an Exact Allowlist

**Files:**

- Stage only the approved cleanup paths

- [ ] **Step 1: Stage only the exact edited and preserved workflow documents**

The `git mv` and `git rm` operations already stage their affected paths. Stage the remaining exact files:

```powershell
git add -- `
    .gitignore `
    README.md `
    docs/README.md `
    docs/archive/README.md `
    docs/superpowers/plans/2026-06-28-project-rules.md `
    docs/superpowers/specs/2026-06-28-project-rules-design.md
if ($LASTEXITCODE -ne 0) { throw 'Failed to stage the exact cleanup documents.' }
```

- [ ] **Step 2: Generate the exact expected staged path set**

Run:

```powershell
$baselineRoot = 'C:\tmp\hotel-booking-cleanup-baseline-2026-07-25'

$superpowersOld = @(
    Get-Content "$baselineRoot\superpowers-tracked.txt"
)

$designOld = @(
    Get-Content "$baselineRoot\legacy-designs-tracked.txt"
)
$designNew = @(
    $designOld |
    ForEach-Object {
        $_ -replace '^2\.Design/', 'docs/archive/designs/2026-06/'
    }
)

$screenshotOld = @(
    Get-Content "$baselineRoot\screenshots-tracked.txt"
)
$screenshotNew = @(
    $screenshotOld |
    ForEach-Object {
        $_ -replace '^Screenshots/', 'docs/assets/screenshots/'
    }
)

$fixedExpected = @(
    '.gitignore',
    'README.md',
    'HLD.md',
    'REQUIREMENTS.md',
    'PROJECT_STRUCTURE.md',
    'PROJECT_STRUCTURE_WEBAPP.md',
    'PROJECT_STRUCTURE_TEST.md',
    'docs/README.md',
    'docs/archive/README.md',
    'docs/archive/architecture/legacy-high-level-design.md',
    'docs/archive/requirements/legacy-user-stories.md',
    'docs/reference/project-structure/backend-and-application.md',
    'docs/reference/project-structure/webapp.md',
    'docs/reference/project-structure/tests.md',
    'docs/superpowers/plans/2026-06-28-project-rules.md',
    'docs/superpowers/specs/2026-06-28-project-rules-design.md'
)

$expectedStagedPaths = @(
    $fixedExpected +
    $superpowersOld +
    $designOld +
    $designNew +
    $screenshotOld +
    $screenshotNew
) | Sort-Object -Unique

$expectedStagedPaths |
    Set-Content -Encoding utf8 "$baselineRoot\expected-staged-paths.txt"

Write-Output "EXPECTED_STAGED_PATHS=$($expectedStagedPaths.Count)"
if ($expectedStagedPaths.Count -ne 281) {
    throw 'The expected staged-path count changed.'
}
```

Expected:

```text
EXPECTED_STAGED_PATHS=281
```

- [ ] **Step 3: Compare the actual index with the exact allowlist**

Run:

```powershell
$baselineRoot = 'C:\tmp\hotel-booking-cleanup-baseline-2026-07-25'
$expectedStagedPaths = @(
    Get-Content "$baselineRoot\expected-staged-paths.txt"
)

$actualStagedPaths = @(
    git diff --cached --name-only --no-renames |
    Sort-Object -Unique
)
if ($LASTEXITCODE -ne 0) { throw 'Failed to enumerate the staged path set.' }

$stagedDifference = @(
    Compare-Object $expectedStagedPaths $actualStagedPaths
)

if ($stagedDifference.Count -ne 0) {
    $stagedDifference | Format-Table -AutoSize
    throw 'The staged path set does not match the approved allowlist.'
}

Write-Output "ACTUAL_STAGED_PATHS=$($actualStagedPaths.Count)"
Write-Output 'STAGED_ALLOWLIST_MATCH=True'
```

Expected:

```text
ACTUAL_STAGED_PATHS=281
STAGED_ALLOWLIST_MATCH=True
```

- [ ] **Step 4: Validate staged content**

Run:

```powershell
git diff --cached --check
if ($LASTEXITCODE -ne 0) { throw 'Staged content failed git diff --check.' }
git diff --cached --stat
if ($LASTEXITCODE -ne 0) { throw 'Failed to inspect the staged diff stat.' }
git diff --cached --name-status --no-renames
if ($LASTEXITCODE -ne 0) { throw 'Failed to inspect staged name-status.' }

$stagedCode = @(
    git diff --cached --name-only |
    Where-Object {
        $_ -like 'HotelBooking.api/*' -or
        $_ -like 'HotelBooking.application/*' -or
        $_ -like 'HotelBooking.infrastructure/*' -or
        $_ -like 'HotelBooking.test/*' -or
        $_ -like 'HotelBooking.webapp/*'
    }
)
if ($LASTEXITCODE -ne 0) { throw 'Failed to inspect staged project paths.' }

if ($stagedCode.Count -ne 0) {
    $stagedCode
    throw 'A project file was staged.'
}

Write-Output 'STAGED_PROJECT_FILES=0'
```

Expected:

- `git diff --cached --check` exits `0`.
- `STAGED_PROJECT_FILES=0`.

- [ ] **Step 5: Repeat artifact preservation checks immediately before commit**

Repeat Task 7 Steps 6–8.

Expected:

```text
PROTECTED_TRACKED_MATCH=True
PROTECTED_PHYSICAL_MATCH=True
OWNER_DASHBOARD_HASH_MATCH=True
OWNER_DASHBOARD_STAGED_COUNT=0
```

## Task 9: Commit and Verify the Cleanup

**Files:**

- Commit: exactly the 281 approved old/new paths

- [ ] **Step 1: Create the atomic cleanup commit**

Run:

```powershell
$baselineRoot = 'C:\tmp\hotel-booking-cleanup-baseline-2026-07-25'

$preCommitSha = (git rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Failed to resolve the pre-commit HEAD.' }
$preCommitSha |
    Set-Content -Encoding utf8 "$baselineRoot\pre-commit-sha.txt"

git commit -m "chore: organize repository structure"
if ($LASTEXITCODE -ne 0) { throw 'Cleanup commit failed.' }

$cleanupCommitSha = (git rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Failed to resolve the cleanup commit SHA.' }
$cleanupCommitParent = (git rev-parse "$cleanupCommitSha^").Trim()
if ($LASTEXITCODE -ne 0) { throw 'Failed to resolve the cleanup commit parent.' }

if ($cleanupCommitSha -eq $preCommitSha) {
    throw 'HEAD did not advance after the cleanup commit.'
}
if ($cleanupCommitParent -ne $preCommitSha) {
    throw 'The cleanup commit is not a direct child of the intended parent.'
}

$cleanupCommitSha |
    Set-Content -Encoding utf8 "$baselineRoot\cleanup-commit-sha.txt"
Write-Output "CLEANUP_COMMIT_SHA=$cleanupCommitSha"
Write-Output "CLEANUP_COMMIT_PARENT_MATCH=True"
```

Expected: one commit is created, its parent is the recorded pre-commit `HEAD`, and no hook or commit output reports a project file outside the allowlist.

- [ ] **Step 2: Verify the committed path set**

Run:

```powershell
$baselineRoot = 'C:\tmp\hotel-booking-cleanup-baseline-2026-07-25'
$expectedStagedPaths = @(
    Get-Content "$baselineRoot\expected-staged-paths.txt"
)
$cleanupCommitSha = (
    Get-Content -Raw "$baselineRoot\cleanup-commit-sha.txt"
).Trim()

$committedPaths = @(
    git show --pretty='' --name-only --no-renames $cleanupCommitSha |
    Where-Object { $_ } |
    Sort-Object -Unique
)
if ($LASTEXITCODE -ne 0) { throw 'Failed to inspect the cleanup commit.' }

$commitDifference = @(
    Compare-Object $expectedStagedPaths $committedPaths
)

if ($commitDifference.Count -ne 0) {
    $commitDifference | Format-Table -AutoSize
    throw 'The commit path set does not match the approved allowlist.'
}

Write-Output "COMMITTED_PATHS=$($committedPaths.Count)"
Write-Output 'COMMIT_ALLOWLIST_MATCH=True'
```

Expected:

```text
COMMITTED_PATHS=281
COMMIT_ALLOWLIST_MATCH=True
```

- [ ] **Step 3: Run final repository checks**

Run:

```powershell
git diff --check
if ($LASTEXITCODE -ne 0) { throw 'Unstaged content failed git diff --check.' }
git diff --name-status --no-renames
if ($LASTEXITCODE -ne 0) { throw 'Failed to inspect unstaged name-status.' }
git status --short
if ($LASTEXITCODE -ne 0) { throw 'Failed to inspect final Git status.' }
git check-ignore -v --no-index -- .agents/probe .superpowers/probe
if ($LASTEXITCODE -ne 0) { throw 'Expected final ignore rules did not match.' }

$ignoredRootEntriesAfter = foreach ($entry in Get-ChildItem -Force) {
    if ($entry.Name -eq '.git') { continue }
    git check-ignore -q --no-index -- $entry.Name
    $ignoreExitCode = $LASTEXITCODE
    if ($ignoreExitCode -gt 1) {
        throw "Failed to inspect final ignore state for: $($entry.Name)"
    }
    if ($ignoreExitCode -eq 0) { $entry.Name }
}
$ignoredRootEntriesAfter |
    Sort-Object |
    ForEach-Object { Write-Output "IGNORED_ROOT_ENTRY_AFTER=$_" }

$superpowersTrackedAfter = @(git ls-files '.superpowers/**')
if ($LASTEXITCODE -ne 0) { throw 'Failed to inspect final .superpowers tracking state.' }
$rootSolutionProjectsAfter = @(
    Select-String -Path 'HotelBooking.sln' -Pattern '^Project\('
)

Write-Output "SUPERPOWERS_TRACKED_AFTER=$($superpowersTrackedAfter.Count)"
Write-Output "ROOT_SOLUTION_PROJECTS_AFTER=$($rootSolutionProjectsAfter.Count)"
```

Expected:

```text
 M HotelBooking.webapp/Pages/User/Owner/OwnerDashboard.razor
IGNORED_ROOT_ENTRY_AFTER=.agents
IGNORED_ROOT_ENTRY_AFTER=.vscode
SUPERPOWERS_TRACKED_AFTER=0
ROOT_SOLUTION_PROJECTS_AFTER=5
```

The two project-rules documents no longer appear as untracked because they are included in the cleanup commit. Report and preserve any additional ignored root entry instead of deleting it.

- [ ] **Step 4: Repeat the final protected-artifact and unfinished-work comparison**

Repeat Task 7 Steps 6–8.

Expected:

```text
PROTECTED_TRACKED_MATCH=True
PROTECTED_PHYSICAL_MATCH=True
OWNER_DASHBOARD_HASH_MATCH=True
OWNER_DASHBOARD_STAGED_COUNT=0
```

- [ ] **Step 5: Report completion evidence**

Report:

- Commit SHA and subject.
- Exact committed path count.
- Counts for legacy designs, screenshots, and project references.
- Draw.io page count.
- Protected tracked and physical artifact comparison results.
- Remaining working-tree status.
- External baseline directory:

```text
C:\tmp\hotel-booking-cleanup-baseline-2026-07-25
```

Do not delete the external baseline until the user accepts the completed cleanup.
