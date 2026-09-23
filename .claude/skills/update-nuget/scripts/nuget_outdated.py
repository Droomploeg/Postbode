#!/usr/bin/env python3
"""Find (and optionally apply) newer NuGet versions for every package pinned in a repo.

Scans *.csproj, *.fsproj, *.props and *.targets for PackageReference / PackageVersion /
GlobalPackageReference items with an inline Version attribute. Unlike
`dotnet list package --outdated`, this also sees references inside conditional
ItemGroups that are inactive on the current OS (e.g. linux-x64 runtime packs on a Mac).

Usage:
  nuget_outdated.py [root]                          # report as JSON
  nuget_outdated.py [root] --apply                  # rewrite versions in place
  nuget_outdated.py [root] --apply --skip Id1,Id2   # apply, but leave these alone
"""
import json
import re
import sys
import urllib.request
from pathlib import Path

ITEM = re.compile(
    r'<(?P<tag>PackageReference|PackageVersion|GlobalPackageReference)\b'
    r'[^>]*?\bInclude="(?P<id>[^"]+)"[^>]*?\bVersion="(?P<ver>[^"$]+)"'
)
SKIP_DIRS = {"bin", "obj", "node_modules", ".git"}
PATTERNS = ("*.csproj", "*.fsproj", "*.props", "*.targets")


def parse_version(v):
    core, _, pre = v.partition("-")
    nums = [int(p) if p.isdigit() else 0 for p in core.split("+")[0].split(".")]
    nums += [0] * (4 - len(nums))
    # A release sorts after any prerelease of the same core version.
    return (nums, pre == "", pre)


def latest_version(package_id, allow_prerelease, cache={}):
    key = package_id.lower()
    if key not in cache:
        url = f"https://api.nuget.org/v3-flatcontainer/{key}/index.json"
        try:
            with urllib.request.urlopen(url, timeout=30) as resp:
                cache[key] = json.load(resp)["versions"]
        except Exception as exc:  # noqa: BLE001 - report and keep going
            print(f"warning: could not query {package_id}: {exc}", file=sys.stderr)
            cache[key] = []
    versions = [v for v in cache[key] if allow_prerelease or "-" not in v]
    return max(versions, key=parse_version) if versions else None


def project_files(root):
    for pattern in PATTERNS:
        for path in root.rglob(pattern):
            if not SKIP_DIRS.intersection(path.relative_to(root).parts):
                yield path


def main():
    args = sys.argv[1:]
    apply = "--apply" in args
    skip = set()
    positional = []
    i = 0
    while i < len(args):
        if args[i] == "--skip":
            skip = {s.strip().lower() for s in args[i + 1].split(",") if s.strip()}
            i += 2
            continue
        if not args[i].startswith("--"):
            positional.append(args[i])
        i += 1
    root = Path(positional[0] if positional else ".").resolve()

    updates = []
    for path in sorted(project_files(root)):
        # newline="" keeps CRLF/LF exactly as it is, so the diff only shows version changes.
        with path.open(encoding="utf-8", newline="") as f:
            text = f.read()
        changed = text
        for m in ITEM.finditer(text):
            pid, current = m.group("id"), m.group("ver")
            latest = latest_version(pid, allow_prerelease="-" in current)
            if not latest or parse_version(latest) <= parse_version(current):
                continue
            skipped = pid.lower() in skip
            updates.append({
                "file": str(path.relative_to(root)),
                "package": pid,
                "from": current,
                "to": latest,
                "major": parse_version(latest)[0][0] > parse_version(current)[0][0],
                "skipped": skipped,
            })
            if apply and not skipped:
                old = m.group(0)
                changed = changed.replace(old, old[: -len(f'"{current}"')] + f'"{latest}"', 1)
        if apply and changed != text:
            with path.open("w", encoding="utf-8", newline="") as f:
                f.write(changed)

    print(json.dumps(updates, indent=2))


if __name__ == "__main__":
    main()
