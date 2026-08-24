#!/usr/bin/env bash
#
# Every asset Unity imports from this package needs a sibling .meta file.
#
# Local folder installs ("com.jest.sdk": "file:../../") are mutable, so Unity
# generates missing .meta files on import and the problem stays invisible.
# Consumers install from a git URL or tarball, which Unity treats as immutable:
# it cannot write .meta files there, so an asset without one is never imported.
# A script in that state silently drops out of the assembly and every reference
# to its types fails with CS0246.
#
# Samples~ is deliberately not checked: Unity ignores ~ folders inside a
# package, and regenerates metas when the sample is imported into Assets.

set -euo pipefail

# Folders whose contents Unity imports from the installed package.
ROOTS=(Runtime Editor Tests)

problems=""

fail() {
  problems="${problems}$1"$'\n'
}

# Tracked files plus new, non-ignored ones, so the check can be run locally
# before committing.
assets_in() {
  git ls-files --cached --others --exclude-standard "$1"
}

echo "🔎 Checking .meta files..."

for root in "${ROOTS[@]}"; do
  while IFS= read -r path; do
    if [ "${path##*.}" = "meta" ]; then
      # Orphans: a meta left behind by a deleted or renamed asset.
      [ -e "${path%.meta}" ] || fail "Orphaned .meta file (no matching asset): $path"
      continue
    fi

    [ -e "$path.meta" ] || fail "Missing .meta file: $path.meta"

    # Unity also needs a meta for every folder in the import path.
    dir=$(dirname "$path")
    while [ "$dir" != "." ]; do
      [ -e "$dir.meta" ] || fail "Missing folder .meta file: $dir.meta"
      dir=$(dirname "$dir")
    done
  done < <(assets_in "$root")
done

# Package-root assets (package.json, README.md, ...). Dot-prefixed paths are
# hidden from Unity's asset database and never need a meta.
while IFS= read -r path; do
  case "$path" in
    .*) continue ;;
  esac
  if [ "${path##*.}" = "meta" ]; then
    [ -e "${path%.meta}" ] || fail "Orphaned .meta file (no matching asset): $path"
  else
    [ -e "$path.meta" ] || fail "Missing .meta file: $path.meta"
  fi
done < <(assets_in . | grep -v '/' || true)

if [ -n "$problems" ]; then
  unique=$(printf '%s' "$problems" | sort -u)
  printf '%s\n' "$unique" | while IFS= read -r problem; do
    echo "❌ $problem"
  done
  echo ""
  echo "$(printf '%s\n' "$unique" | wc -l | tr -d ' ') problem(s) found."
  echo "To generate a missing .meta, open a project that installs the SDK as a"
  echo "local folder package (Unity writes metas for mutable packages) and commit"
  echo "the result. Delete orphaned ones."
  exit 1
fi

echo "✅ All imported assets have .meta files."
