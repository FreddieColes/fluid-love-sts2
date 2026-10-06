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

# Description and visibility are only sent on the first upload; after that, edit them on the web page.
{
  echo '"workshopitem"'
  echo '{'
  echo '  "appid" "2868840"'
  echo "  \"publishedfileid\" \"$ID\""
  echo "  \"contentfolder\" \"$STAGE/content\""
  echo "  \"previewfile\" \"$STAGE/preview.png\""
  echo '  "title" "Fluid Love Band"'
  echo "  \"changenote\" \"$NOTE\""
  if [ "$ID" = "0" ]; then
    echo '  "visibility" "2"'
    echo "  \"description\" \"$DESC\""
  fi
  echo '}'
} > "$STAGE/item.vdf"

read -rp "Steam username: " STEAM_USER
"$STEAMCMD" +login "$STEAM_USER" +workshop_build_item "$STAGE/item.vdf" +quit | tee logs/publish.log

NEWID=$(grep -oP '"publishedfileid"\s*"\K[0-9]+' "$STAGE/item.vdf")
if grep -qiE "ERROR|Failed" logs/publish.log; then
  echo
  echo "UPLOAD FAILED (see above). Run ./report.sh and tell Claude."
elif [ -n "$NEWID" ] && [ "$NEWID" != "0" ]; then
  echo "$NEWID" > workshop/id.txt
  if [ "$ID" = "0" ]; then git add workshop/id.txt && git commit -qm "workshop id $NEWID" && git pull -q --rebase --autostash && git push -q; fi
  echo
  echo "Uploaded. Page: https://steamcommunity.com/sharedfiles/filedetails/?id=$NEWID"
  [ "$ID" = "0" ] && echo "It's PRIVATE. On that page: add BaseLib under Required items, then set visibility when ready."
else
  echo "Upload didn't report an ID. Run ./report.sh and tell Claude."
fi
