# Repository Cleanup Design

| Field | Value |
|---|---|
| Status | Approved design |
| Date | 2026-07-25 |
| Scope | Repository organization and Git hygiene |
| Implementation status | Not started |

## 1. Objective

Organize the Hotel Booking repository without changing application behavior, project build behavior, or historical project evidence.

The cleanup will:

- Remove an unused vendored tool bundle.
- Organize current, reference, archived, and evidence documentation.
- Create clear documentation indexes.
- Improve `.gitignore` for repository hygiene.
- Preserve all application projects, solution files, generated artifacts, test results, configuration values, and unfinished user work.

## 2. Approved Cleanup Policy

The selected approach is a structured documentation hub with conservative technical scope.

### 2.1 Preserve historical evidence

All legacy documents and screenshots are preserved. They are moved to explicit archive, reference, or asset locations rather than deleted.

### 2.2 Preserve technical artifacts

The following are intentionally retained:

- Root and project-level solution files
- `bin/`
- `obj/`
- `Logs/`
- `scriptcs_bin/`
- `TestResults/*.trx`
- Project `Program.cs` files
- `.http` files
- Project `launchSettings.json` files
- All `appsettings.json` files
- Existing project SDK declarations and project references

Although many of these files are generated or redundant, removing them was explicitly excluded by the approved cleanup policy.

### 2.3 Secret handling is excluded

Credential-bearing configuration files are not sanitized, ignored, rotated, or removed from Git history in this cleanup.

This is an accepted scope exclusion, not a statement that committed credentials are safe. Secret rotation and history remediation require a separate security task.

## 3. Root Directory After Cleanup

The following items remain at the repository root:

```text
.git/
.gitignore
.agents/
.vscode/
AGENTS.md
CLAUDE.md
GEMINI.md
HotelBooking.sln
README.md
Scripts/
HotelBooking.api/
HotelBooking.application/
HotelBooking.infrastructure/
HotelBooking.test/
HotelBooking.webapp/
docs/
```

Nested project solution files remain inside their existing project directories.

## 4. Exact Delete Manifest

The only tracked content removed is the approved baseline set of 145 tracked paths under:

```text
.superpowers/
```

Rationale:

- It contains a vendored third-party tool/plugin bundle.
- The approved baseline contains 145 tracked files.
- No project file references it.
- Active local skills are provided through `.agents/`.
- Deleted content remains recoverable from Git history.

Before removal, implementation must compare the current tracked set with the approved 145-path baseline and enumerate every untracked or ignored descendant separately. Only the tracked baseline paths may be removed. If an untracked or ignored descendant appears, it must be reported and left untouched. The physical directory may be removed only when it is empty after the tracked removals.

No other directory or file is authorized for deletion.

## 5. Exact Move Manifest

### 5.1 Legacy architecture and requirements

```text
HLD.md
-> docs/archive/architecture/legacy-high-level-design.md

REQUIREMENTS.md
-> docs/archive/requirements/legacy-user-stories.md
```

The authoritative HLD remains:

```text
docs/architecture/high-level-design.md
```

### 5.2 Legacy feature designs

```text
2.Design/
-> docs/archive/designs/2026-06/
```

All 14 design documents are retained with their existing content.

### 5.3 Project structure references

```text
PROJECT_STRUCTURE.md
-> docs/reference/project-structure/backend-and-application.md

PROJECT_STRUCTURE_WEBAPP.md
-> docs/reference/project-structure/webapp.md

PROJECT_STRUCTURE_TEST.md
-> docs/reference/project-structure/tests.md
```

These documents remain reference material and are not treated as authoritative architecture.

### 5.4 Screenshot evidence

```text
Screenshots/
-> docs/assets/screenshots/
```

The existing groups are retained:

```text
Admin/
Auth/
Owner/
Room/
Search/
Tests/
```

`Search/` and `Tests/` are currently empty. Git does not retain empty directories, and the cleanup will not add marker files only to preserve them.

The move must preserve all 46 currently tracked screenshot files.

### 5.5 Workflow documents

The workflow directories remain:

```text
docs/superpowers/specs/
docs/superpowers/plans/
```

The two currently untracked project-rules documents are preserved and included in the repository during cleanup:

```text
docs/superpowers/specs/2026-06-28-project-rules-design.md
docs/superpowers/plans/2026-06-28-project-rules.md
```

## 6. New Documentation Indexes

Create:

```text
docs/README.md
docs/archive/README.md
```

### 6.1 `docs/README.md`

The main documentation index distinguishes:

- Authoritative architecture
- Current requirements status
- Workflow specifications and plans
- Reference documents
- Historical documents
- Screenshot evidence

No authoritative detailed requirements catalog exists at the time of cleanup. The index must state this explicitly. The archived requirements remain a historical baseline, while the target User Story and Acceptance Criteria catalog is planned work following HLD approval.

### 6.2 `docs/archive/README.md`

The archive index states that archived documents:

- Are preserved for historical context.
- May describe outdated behavior or architecture.
- Must not be treated as the current source of truth.
- Should not be updated with new requirements.

## 7. README Updates

Update the root `README.md` to:

- Link to `docs/README.md`.
- Link to the authoritative HLD.
- Link to archived requirements only as historical material.
- Link to project-structure reference documents at their new locations.
- Preserve the existing `Scripts/` database setup link.

The cleanup does not rewrite the full README or change product claims unrelated to moved paths.

## 8. `.gitignore` Design

All `.gitignore` comments are written in English.

### 8.1 Tooling rules

Add or normalize:

```gitignore
.agents/
.superpowers/
```

`.agents/` remains local-only. `.superpowers/` prevents the removed vendor bundle from being reintroduced.

Remove ignore rules for:

```text
AGENTS.md
GEMINI.md
```

These files are tracked project standards. `CLAUDE.md` also remains tracked.

### 8.2 Additional repository-hygiene rules

Add:

```gitignore
artifacts/
coverage/
coverage-report/
*.binlog
BenchmarkDotNet.Artifacts/
.sonarqube/
.history/
*.orig
*.rej
Desktop.ini
```

### 8.3 Explicitly omitted rules

Do not add new ignore rules for:

- Secret or local configuration files
- `scriptcs_bin/`
- Project-level solution files
- Existing tracked test results

Existing build, IDE, NuGet, test, publish, and operating-system rules remain unless normalized for readability.

## 9. Safety Controls

- Do not use `git clean`.
- Do not use a recursive wildcard deletion against the workspace root.
- Resolve and validate every delete or move target before mutation.
- Use Git-aware moves for tracked files.
- Remove only the approved 145 tracked `.superpowers/` paths.
- Report and preserve any unexpected untracked or ignored `.superpowers/` descendant.
- Preserve the unfinished user change:

```text
HotelBooking.webapp/Pages/User/Owner/OwnerDashboard.razor
```

- Do not stage unrelated code or configuration.
- Do not delete the untracked project-rules documents.
- Do not alter generated artifacts even when they are ignored.

## 10. Link Handling

After moving files:

- Update root README links.
- Update internal Markdown links affected by the move.
- Verify no live reference points to the old locations:

```text
HLD.md
REQUIREMENTS.md
2.Design/
Screenshots/
PROJECT_STRUCTURE.md
PROJECT_STRUCTURE_WEBAPP.md
PROJECT_STRUCTURE_TEST.md
```

- Preserve the existing `Scripts/` link because that directory does not move.
- Mark archive links as historical.

## 11. Verification

### 11.1 Git checks

```text
git diff --check
git status --short
git diff --name-status
git check-ignore -v --no-index -- .agents/probe .superpowers/probe
git diff --cached --check
git diff --cached --name-status
```

Verify:

- The approved `.superpowers/` baseline paths are absent from the tracked file list.
- Any unexpected untracked or ignored `.superpowers/` descendant remains untouched and is reported.
- No unexpected file is deleted.
- Staging uses exact paths from the approved move/delete/edit manifest. It must not use broad patterns such as `docs/**`.
- The exact staged-path set matches a generated allowlist before commit.
- The unfinished Owner Dashboard change remains unstaged.

### 11.2 Content preservation checks

Verify:

- The authoritative HLD exists.
- The draw.io HLD parses successfully and contains four pages.
- All 14 legacy feature-design documents exist in the archive.
- All 46 tracked screenshots exist under `docs/assets/screenshots/`.
- All three project-structure reference documents exist.
- Both project-rules workflow documents exist.
- The root solution still lists all five projects.
- Nested solutions, generated artifacts, test results, and configuration files remain.

Before mutation, create a protected-artifact baseline outside the repository:

- Path set and Git blob ID for all six tracked solution files.
- Path set and Git blob ID for all 27 tracked `scriptcs_bin` files.
- Path set and Git blob ID for both tracked `TestResults` files.
- Path set and Git blob ID for all five tracked `appsettings.json` files.
- Relative path, byte length, and SHA-256 hash for every existing ignored file under `bin/`, `obj/`, and `Logs/`.

Repeat the same snapshot after cleanup and require exact equality. The temporary baseline must remain outside the repository and must never be staged.

### 11.3 Reference checks

Search for references to every old path. Any remaining result must be either:

- Updated to the new path, or
- Intentionally quoted by the cleanup design or implementation plan.

### 11.4 Build and test policy

Build and test are not run during this cleanup because they would mutate the generated artifacts that the approved policy requires preserving byte-for-byte. Verification instead proves that no code or project file is staged and that the protected-artifact snapshots are identical.

## 12. Commit Strategy

1. Commit this cleanup design as a standalone documentation commit.
2. Obtain user review of the committed design.
3. Create a detailed implementation plan.
4. Execute the cleanup in one atomic commit:

```text
chore: organize repository structure
```

The cleanup commit may contain only:

- `.gitignore`
- `README.md`
- `docs/README.md`
- `docs/archive/README.md`
- Exact old and new paths from the approved document and screenshot move manifest
- The two approved project-rules workflow documents
- Removal of the approved 145 tracked `.superpowers/` baseline paths

The implementation plan must materialize this list as an exact staged-path allowlist. The commit is blocked if `git diff --cached --name-only` contains any path outside that allowlist or omits an expected path.

## 13. Acceptance Criteria

The cleanup is complete when:

1. The approved tracked root documents are moved as specified. Unexpected local root entries are reported and left untouched.
2. The approved tracked `.superpowers/` content is removed and the path is ignored; any unexpected local descendant is preserved and reported.
3. Historical documents and screenshots are preserved at their approved destinations.
4. Documentation indexes clearly distinguish authoritative, reference, and archived material.
5. `.gitignore` matches the approved design.
6. All affected links resolve.
7. No code, project, solution, generated artifact, test result, or configuration value is changed.
8. The existing Owner Dashboard work remains intact and outside the cleanup commit.
9. Verification reports no unexpected deletion or staged file.
10. Protected tracked and ignored artifact snapshots match their pre-cleanup baselines exactly.
