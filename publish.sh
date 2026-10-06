#!/usr/bin/env bash
# Uploads the last successful build to the Steam Workshop with SteamCMD.
# First run creates the item (private). Later runs update the same item.
# Usage: ./publish.sh "what changed"
set -uo pipefail
cd "$(dirname "$0")"
STEAMCMD=/workspaces/tools/steamcmd/steamcmd.sh
M=out/mods/FluidLoveBand
NOTE="${1:-Update}"
for f in FluidLoveBand.dll FluidLoveBand.json FluidLoveBand.pck; do
  [ -f "$M/$f" ] || { echo "No build found ($f missing). Run ./build.sh first."; exit 1; }
done
[ -x "$STEAMCMD" ] || { echo "Run ./setup.sh first."; exit 1; }

STAGE=/workspaces/fluid-love-workshop
rm -rf "$STAGE" && mkdir -p "$STAGE/content"
cp "$M"/* "$STAGE/content/"
cp workshop/preview.png "$STAGE/preview.png" 2>/dev/null || cp FluidLoveBand/mod_image.png "$STAGE/preview.png"
ID=$(cat workshop/id.txt 2>/dev/null || echo 0)
DESC=$(sed 's/"/\\"/g' workshop/description.txt)

cat > "$STAGE/item.vdf" <<V
"workshopitem"
{
  "appid" "2868840"
  "publishedfileid" "$ID"
  "contentfolder" "$STAGE/content"
  "previewfile" "$STAGE/preview.png"
  "visibility" "2"
  "title" "Fluid Love Band"
  "description" "$DESC"
  "changenote" "$NOTE"
}
V
[ "$ID" != "0" ] && sed -i '/"visibility"/d; /"description"/d' "$STAGE/item.vdf"

read -rp "Steam username: " STEAM_USER
"$STEAMCMD" +login "$STEAM_USER" +workshop_build_item "$STAGE/item.vdf" +quit | tee logs/publish.log

NEWID=$(grep -oP '"publishedfileid"\s*"\K[0-9]+' "$STAGE/item.vdf")
if [ -n "$NEWID" ] && [ "$NEWID" != "0" ]; then
  echo "$NEWID" > workshop/id.txt
  git add workshop/id.txt && git commit -qm "workshop id $NEWID" && git pull -q --rebase && git push -q
  echo
  echo "Uploaded. Page: https://steamcommunity.com/sharedfiles/filedetails/?id=$NEWID"
  echo "It's PRIVATE. On that page: add BaseLib under Required items, then set visibility when ready."
else
  echo "Upload didn't report an ID. Run ./report.sh and tell Claude."
fi
