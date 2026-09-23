---
name: update-nuget
description: Routine NuGet maintenance for a .NET repo — pull latest main, create a `maintenance/{yyyy-MM-dd}` branch, update all NuGet packages (including ones in OS-conditional ItemGroups) and vulnerable transitive dependencies, build and run the tests, and open a pull request when everything passes. Use when the user asks to update/upgrade NuGet packages or dependencies, do "maintenance", bump package versions, or fix vulnerable packages.
---

# Update NuGet packages

End-to-end maintenance run. Stop and report (do **not** open a PR) if the build or
tests stay red after the fallback steps below.

## 1. Start from a clean, current main

The sandbox blocks writes to `.git` and to project files from subprocesses, so run
the git commands, `--apply` and `dotnet test` unsandboxed.

```bash
git status --porcelain          # must be empty (untracked .claude/ is fine) — otherwise stop and ask
git switch main && git pull --ff-only
BRANCH="maintenance/$(date +%Y-%m-%d)"
git switch -c "$BRANCH"         # if it exists already, append -2, -3, …
```

## 2. Record the baseline

Run `dotnet build` on main first. If main itself does not build, stop: a package
update PR must not hide an existing failure.

## 3. Find and apply updates

```bash
python3 .claude/skills/update-nuget/scripts/nuget_outdated.py .            # report
python3 .claude/skills/update-nuget/scripts/nuget_outdated.py . --apply    # apply
```

The script queries nuget.org directly and scans every `*.csproj` / `*.props` /
`*.targets`, so it also bumps references in conditional ItemGroups that are
inactive on this machine (e.g. `Aspire.*.linux-x64` on a Mac) — `dotnet list
package --outdated` misses those. Packages already on a prerelease stay on the
prerelease channel; stable ones only move to stable versions. Major bumps are
flagged `"major": true` — they are applied too, but mention them in the PR.

If the script cannot write files under the sandbox, rerun it with the sandbox
disabled (it only edits project files).

Keep package families in lockstep: all `Aspire.*`, `Microsoft.Extensions.*`,
`Microsoft.AspNetCore.*`, `Azure.*` versions that were equal before must be equal
after. If nuget.org has a newer version for only part of a family, check that the
mix restores cleanly.

Also respect the `csharp-architecture` skill: no new third-party packages — this
job only bumps existing references.

## 4. Vulnerable transitive dependencies

```bash
dotnet restore
dotnet list package --vulnerable --include-transitive
```

For each vulnerable transitive package, first check whether a direct update
above already resolved it. If not, add an explicit `PackageReference` with the
lowest patched version to the project that pulls it in, with a comment
`<!-- Pin transitive dependency: <advisory> -->`. This mirrors the
`vulnerability-scan` job in CI, which fails the build on any hit. The scan can
take several minutes — run it in the background, in parallel with the tests.

## 5. Build and test

```bash
dotnet build --configuration Release
dotnet test --configuration Release
```

`dotnet test` needs the sandbox disabled (MSBuild named pipes) and Docker running
(Aspire starts real containers). Run it in the background if it is slow.

If build or tests fail:
1. Read the errors; fix small API changes in code when the fix is obvious and
   local (keep it minimal, follow the project's CLAUDE.md).
2. Otherwise revert the culprit and retry:
   `nuget_outdated.py . --apply --skip Id1,Id2` after `git checkout -- .`
   (skip the whole family, not one member).
3. Still failing → stop, leave the branch local, report what broke.

## 6. Commit

Commit only the project files that changed (plus code fixes, if any):

```
Update nuget packages
```

Follow the repo's commit conventions; signing is on — never bypass it.

## 7. Push and open the PR

```bash
git push -u origin "$BRANCH"
git remote get-url origin
```

- **GitHub** remote → `gh pr create --base main --head "$BRANCH" --title "Update nuget packages" --body …`
  (`gh` reads `~/.config/gh`, which the sandbox blocks — run it unsandboxed).
- **Forgejo** remote (`forgejo.droomploeg.lan`) → use the API recipe from
  `~/CLAUDE.md` (`git credential fill` + `curl …/api/v1/repos/<org>/<repo>/pulls`).
  Never echo the password. `gh` does not work there.

PR body (explain the *why*):

```markdown
Routine NuGet maintenance.

## Updated packages
| Package | From | To |
|---|---|---|
| … | … | … |

## Notes
- Major version bumps: … (or "none")
- Skipped (breaks build/tests): … (or "none")
- Vulnerable transitive dependencies: … (or "none found")

## Verification
- `dotnet build` ✅
- `dotnet test` ✅ (<passed>/<total>)
```

End with the attribution line required by the session. Report the PR URL to the user.
