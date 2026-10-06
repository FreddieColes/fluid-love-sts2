# Fluid Love Band — Slay the Spire 2 character mod

Play as the Fluid Love Band. Every card adds a note (Lead, Rhythm or Keys) to a 3-slot Setlist;
three notes play a Song. Built on BaseLib 3.4.5 for game v0.107.x.

## Codespace workflow
1. `./setup.sh` once (asks for your Steam login + Steam Guard to fetch the game files).
2. `./build.sh` → `dist/FluidLoveBand.zip`. Right-click > Download.
3. On your PC: unzip into `Slay the Spire 2/mods/` (you should end up with `mods/FluidLoveBand/`).
   BaseLib must be installed too (Steam Workshop or `mods/BaseLib/`).
4. Launch with mods, enable Fluid Love Band, restart.
5. Problems: `./report.sh` pushes `logs/` here. Drop a game log into `logs/` first if it crashed.

## Layout
- `FluidLoveBandCode/Music` — the Setlist rules, Songs, on-screen widget
- `FluidLoveBandCode/Cards` — cards (each has a band role)
- `FluidLoveBandCode/Relics`, `Potions`, `Powers`, `Character`
- `FluidLoveBand/images`, `FluidLoveBand/localization/eng` — art and text
