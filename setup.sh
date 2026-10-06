#!/usr/bin/env bash
# One-time Codespace setup: SteamCMD + game files, Godot 4.5.1 .NET, build paths.
# Safe to re-run; finished steps are skipped.
set -euo pipefail
cd "$(dirname "$0")"
GAME=/workspaces/sts2-game
TOOLS=/workspaces/tools
GODOT_DIR=$TOOLS/Godot_v4.5.1-stable_mono_linux_x86_64
GODOT=$GODOT_DIR/Godot_v4.5.1-stable_mono_linux.x86_64
mkdir -p "$TOOLS" logs out dist
touch logs/.gdignore out/.gdignore dist/.gdignore

echo "== 1/4 System packages"
sudo dpkg --add-architecture i386 >/dev/null 2>&1 || true
# The base image ships a Yarn apt source with an expired key; it breaks apt-get update. We don't need Yarn.
sudo rm -f /etc/apt/sources.list.d/yarn*.list
sudo grep -rl "dl.yarnpkg.com" /etc/apt/sources.list.d/ 2>/dev/null | xargs -r sudo rm -f
sudo apt-get update -qq || echo "   (apt update had warnings, carrying on)"
sudo apt-get install -y -qq lib32gcc-s1 unzip zip curl libfontconfig1 >/dev/null

echo "== 2/4 Slay the Spire 2 files (Windows build, via SteamCMD)"
if ls "$GAME"/data_sts2_*/sts2.dll >/dev/null 2>&1; then
  echo "   already downloaded, skipping"
else
  mkdir -p "$TOOLS/steamcmd"
  if [ ! -x "$TOOLS/steamcmd/steamcmd.sh" ]; then
    curl -sSL https://steamcdn-a.akamaihd.net/client/installer/steamcmd_linux.tar.gz | tar -xz -C "$TOOLS/steamcmd"
  fi
  read -rp "   Steam username: " STEAM_USER
  echo "   SteamCMD will ask for your password and Steam Guard code."
  "$TOOLS/steamcmd/steamcmd.sh" +@sSteamCmdForcePlatformType windows +force_install_dir "$GAME" \
    +login "$STEAM_USER" +app_update 2868840 validate +quit
  if ! ls "$GAME"/data_sts2_*/sts2.dll >/dev/null 2>&1; then
    echo "!! sts2.dll not found after download. Run ./report.sh and tell Claude."; exit 1
  fi
fi

echo "== 3/4 Godot 4.5.1 .NET (headless)"
if [ -x "$GODOT" ]; then
  echo "   already installed, skipping"
else
  curl -sSL -o /tmp/godot.zip https://github.com/godotengine/godot/releases/download/4.5.1-stable/Godot_v4.5.1-stable_mono_linux_x86_64.zip
  unzip -q -o /tmp/godot.zip -d "$TOOLS" && rm /tmp/godot.zip
  chmod +x "$GODOT"
fi

echo "== 4/4 Build paths + restore"
cat > Directory.Build.props <<P
<Project>
    <PropertyGroup>
        <GodotPath>$GODOT</GodotPath>
        <Sts2Path>$GAME</Sts2Path>
    </PropertyGroup>
</Project>
P
dotnet restore 2>&1 | tail -3
GAMEVER=$(cat "$GAME"/release_info.json 2>/dev/null | head -c 300 || true)
echo
echo "Setup done. Game info: ${GAMEVER:-unknown}"
echo "Next: ./build.sh"
