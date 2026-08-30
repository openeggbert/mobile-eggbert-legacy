#!/usr/bin/env bash
# Runs the built game under Wine in the XNA 4.0 prefix. This is the same environment as the `wx`
# alias: d3d9=b selects WineD3D, which is the path the XNA 4.0 work on this host is verified
# against; the default Wine/DXVK route can fail even on a valid build.
#
# Game1 hardcodes IsFullScreen = true and never sets a preferred back buffer, so XNA chooses the
# fullscreen mode itself. The art is drawn 1:1 for the Windows Phone's 800x480 screen, so on a
# larger mode the picture is cropped rather than scaled -- a Wine virtual desktop does not pin it,
# because the game changes the display mode from inside. Running on an 800x480 display gives the
# original framing exactly.
set -euo pipefail

bin_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/../build/bin" && pwd)"

cd "$bin_dir"
exec env WINEPREFIX="${CNA_XNA40_WINEPREFIX:-$HOME/.wine-cna-xna40}" \
    WINEDLLOVERRIDES=d3d9=b WINEDEBUG=-all \
    wine SpeedyBlupi.exe "$@"
