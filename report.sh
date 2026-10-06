#!/usr/bin/env bash
# Pushes logs/ to GitHub so Claude can read them.
# To include a game log: drag it from your PC into the logs folder in the Explorer first.
cd "$(dirname "$0")"
git config user.email >/dev/null || git config user.email "freddie@codespace"
git config user.name >/dev/null || git config user.name "Freddie"
dotnet --info 2>/dev/null | head -5 > logs/env.txt
ls -la /workspaces/sts2-game 2>/dev/null | head -30 >> logs/env.txt
ls out/mods/FluidLoveBand 2>/dev/null >> logs/env.txt
git add -f logs/
git commit -qm "logs $(date -u +%H:%M)" || true
git pull -q --rebase && git push -q && echo "Report pushed. Tell Claude 'report pushed'."
