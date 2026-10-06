#!/usr/bin/env bash
# Builds the mod and zips it to dist/FluidLoveBand.zip
set -uo pipefail
cd "$(dirname "$0")"
mkdir -p logs out dist
touch logs/.gdignore out/.gdignore dist/.gdignore
LOG=logs/build.log
GODOT=$(grep -oP '(?<=<GodotPath>).*(?=</GodotPath>)' Directory.Build.props 2>/dev/null || true)
if [ -z "$GODOT" ]; then echo "Run ./setup.sh first."; exit 1; fi
rm -rf out/mods/FluidLoveBand dist/FluidLoveBand.zip

{
  echo "== $(date -u) build"
  echo "== Godot import"
  timeout 600 "$GODOT" --headless --path . --import 2>&1 | grep -v "^$" | tail -40
  echo "== dotnet publish"
  dotnet publish 2>&1
} > "$LOG" 2>&1

M=out/mods/FluidLoveBand
ok=1
for f in FluidLoveBand.dll FluidLoveBand.json FluidLoveBand.pck; do
  if [ ! -f "$M/$f" ]; then echo "missing: $f" | tee -a "$LOG"; ok=0; fi
done

grep -E "error|Error" "$LOG" | grep -v "0 Error" | sort -u | head -25
if [ $ok = 1 ]; then
  (cd out/mods && zip -qr ../../dist/FluidLoveBand.zip FluidLoveBand)
  echo
  echo "BUILD OK -> dist/FluidLoveBand.zip"
  echo "Right-click it in the Explorer (left) > Download, then unzip into your game's mods folder."
else
  echo
  echo "BUILD FAILED. Run ./report.sh so Claude can read the log."
fi
