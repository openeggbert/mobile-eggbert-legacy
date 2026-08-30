#!/usr/bin/env bash
# Builds Speedy Blupi against the original Microsoft XNA Game Studio 4.0 for Windows: the
# decompiled Windows Phone sources are linked by the stock .NET 4.0 csc.exe under Wine, and the
# textures and sounds go through the stock XNA Content Pipeline importers.
#
# The Windows Phone original is a Reach-profile game, so the Windows build stays Reach: the
# RuntimeProfile resource embedded in the executable is checked again at load time, and a
# mismatch kills the game in LoadContent.
#
# Set CNA_BUILD_CONTENT=0 to relink the executable without rebuilding the 140 .xnb files.
set -euo pipefail

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
build_dir="$project_root/build"
runner_dir="$build_dir/pipeline-runner"
bin_dir="$build_dir/bin"
obj_dir="$build_dir/obj"

wine_prefix="${CNA_XNA40_WINEPREFIX:-$HOME/.wine-cna-xna40}"
refs="${CNA_XNA40_REFS:-/rv/tmp/samples/_tools/xna-game-studio-4-refresh/admin/Program Files/Microsoft XNA/XNA Game Studio/v4.0/References/Windows/x86}"
xna_native="$wine_prefix/drive_c/Program Files/Common Files/Microsoft Shared/XNA/Framework/v4.0/XnaNative.dll"
build_content="${CNA_BUILD_CONTENT:-1}"

win() { printf '%s' "Z:$1" | sed 's#/#\\#g'; }

# The content pipeline creates a real D3D9 device, so it gets a display of its own rather than the
# owner's desktop.
display="${CNA_BUILD_DISPLAY:-:118}"
if ! DISPLAY="$display" xdpyinfo >/dev/null 2>&1; then
    Xvfb "$display" -screen 0 1280x1024x24 +extension GLX >/dev/null 2>&1 &
    xvfb_pid=$!
    trap 'kill "${xvfb_pid:-}" 2>/dev/null || true' EXIT
    for _ in $(seq 1 100); do
        DISPLAY="$display" xdpyinfo >/dev/null 2>&1 && break
        sleep 0.1
    done
fi
export DISPLAY="$display"

mkdir -p "$runner_dir" "$bin_dir/Content" "$build_dir/Content" "$obj_dir/content"

for dll in Microsoft.Xna.Framework.dll Microsoft.Xna.Framework.Game.dll \
           Microsoft.Xna.Framework.Graphics.dll Microsoft.Xna.Framework.Content.Pipeline.dll \
           Microsoft.Xna.Framework.Content.Pipeline.TextureImporter.dll \
           Microsoft.Xna.Framework.Content.Pipeline.AudioImporters.dll; do
    cp -u "$refs/$dll" "$runner_dir/"
done
cp -u "$xna_native" "$runner_dir/"
# WavImporter P/Invokes this one, and it only ships in the Game Studio Bin directory.
cp -u "$refs/../../../Bin/XnaMediaHelper_1.dll" "$runner_dir/"
cp -u /usr/lib/mono/4.5/Microsoft.Build.Framework.dll "$runner_dir/"
cp -u /usr/lib/mono/4.5/Microsoft.Build.Utilities.v4.0.dll "$runner_dir/"

if [[ "$build_content" == "1" ]]; then
    mcs -platform:x86 -target:exe -out:"$runner_dir/XnaPipelineRunner.exe" \
        -r:"$runner_dir/Microsoft.Build.Framework.dll" \
        -r:"$runner_dir/Microsoft.Build.Utilities.v4.0.dll" \
        -r:"$runner_dir/Microsoft.Xna.Framework.Content.Pipeline.dll" \
        "$project_root/scripts/XnaPipelineRunner.cs"

    env WINEPREFIX="$wine_prefix" WINEDEBUG=-all wine "$runner_dir/XnaPipelineRunner.exe" \
        "$(win "$project_root/Content")" \
        "$(win "$build_dir/Content")" \
        "$(win "$obj_dir/content")" \
        "$(win "$runner_dir")"
fi

cp "$project_root/scripts/Microsoft.Xna.Framework.RuntimeProfile.txt" "$obj_dir/"

gac32='C:\windows\Microsoft.NET\assembly\GAC_32'
gacmsil='C:\windows\Microsoft.NET\assembly\GAC_MSIL'
ver='v4.0_4.0.0.0__842cf8be1de50553'

sources=()
while IFS= read -r -d '' f; do sources+=("$(win "$f")"); done < <(
    find "$project_root/WindowsPhoneSpeedyBlupi" "$project_root/Microsoft.Devices.Sensors/" \
         -name '*.cs' -print0 | sort -z)

env WINEPREFIX="$wine_prefix" WINEDEBUG=-all \
    wine 'C:\windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' \
    /nologo /target:winexe /platform:x86 /debug+ '/define:TRACE;WINDOWS' \
    "/out:$(win "$bin_dir/SpeedyBlupi.exe")" \
    "/resource:$(win "$obj_dir/Microsoft.Xna.Framework.RuntimeProfile.txt"),Microsoft.Xna.Framework.RuntimeProfile" \
    "/reference:$gac32\\Microsoft.Xna.Framework\\$ver\\Microsoft.Xna.Framework.dll" \
    "/reference:$gac32\\Microsoft.Xna.Framework.Game\\$ver\\Microsoft.Xna.Framework.Game.dll" \
    "/reference:$gac32\\Microsoft.Xna.Framework.Graphics\\$ver\\Microsoft.Xna.Framework.Graphics.dll" \
    "/reference:$gacmsil\\Microsoft.Xna.Framework.GamerServices\\$ver\\Microsoft.Xna.Framework.GamerServices.dll" \
    "/reference:$gacmsil\\Microsoft.Xna.Framework.Input.Touch\\$ver\\Microsoft.Xna.Framework.Input.Touch.dll" \
    "/reference:$gacmsil\\Microsoft.Xna.Framework.Storage\\$ver\\Microsoft.Xna.Framework.Storage.dll" \
    /reference:System.dll /reference:System.Core.dll /reference:System.Xml.dll \
    "${sources[@]}"

for dll in Microsoft.Xna.Framework.dll Microsoft.Xna.Framework.Game.dll \
           Microsoft.Xna.Framework.Graphics.dll Microsoft.Xna.Framework.GamerServices.dll \
           Microsoft.Xna.Framework.Input.Touch.dll Microsoft.Xna.Framework.Storage.dll; do
    cp -u "$refs/$dll" "$bin_dir/"
done
cp -a "$build_dir/Content/." "$bin_dir/Content/"
mkdir -p "$bin_dir/worlds"
cp -a "$project_root/worlds/." "$bin_dir/worlds/"

file "$bin_dir/SpeedyBlupi.exe"
echo "--- content ---"
(cd "$bin_dir/Content" && find . -name '*.xnb' | wc -l) | sed 's/^/xnb files: /'
