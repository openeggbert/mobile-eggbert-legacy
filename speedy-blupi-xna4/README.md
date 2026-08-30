# speedy-blupi-xna4

Speedy Blupi built against the **original Microsoft XNA Game Studio 4.0** for Windows and run on
Linux through Wine.

`WindowsPhoneSpeedyBlupi/` holds the C# decompiled by ILSpy from `speedblu.xap`, taken from commit
`dc4dfaf` (*Added the C# code decompiled by ILSpy using the file speedblu.xap*) rather than from the
later modernised branches. Only what the raw decompiler output needed to compile and run off the
phone was changed — see *Changes to the decompiled source* below.

Content and levels are relative symlinks to the shared sibling directories, so nothing is
duplicated:

| Link | Target |
|---|---|
| `Content` | `../mobile-eggbert-content` |
| `worlds` | `../mobile-eggbert-worlds` |
| `Microsoft.Devices.Sensors` | `../mobile-eggbert-fna-desktop/mobile-eggbert-fna/Microsoft.Devices.Sensors` |

## Building

```bash
./scripts/build.sh                    # content + executable
CNA_BUILD_CONTENT=0 ./scripts/build.sh  # relink only
```

The script links the game with the stock .NET 4.0 `csc.exe` under Wine and runs the 140 textures
and sounds through the stock XNA Content Pipeline importers, driven by
`scripts/XnaPipelineRunner.cs`. Everything lands in `build/` (gitignored):

```
build/bin/SpeedyBlupi.exe   build/bin/Content/*.xnb   build/bin/worlds/
```

The Windows Phone original is a Reach-profile game, so the Windows build stays Reach. The profile
is embedded as the `Microsoft.Xna.Framework.RuntimeProfile` resource and checked again at load
time — a mismatch kills the game in `LoadContent`.

Prerequisites, all already present on this host: the `~/.wine-cna-xna40` prefix, the XNA Game
Studio 4.0 reference assemblies under `/rv/tmp/samples/_tools/` (override with `CNA_XNA40_REFS`),
`mcs`, `wine`, `Xvfb`. The pipeline needs a display of its own because it creates a real D3D9
device; the script starts one.

## Running

```bash
./scripts/run.sh
```

or, with the host's `wx` alias, from `build/bin`:

```bash
wx SpeedyBlupi.exe
```

### Controls

The phone build only ever read the touch panel. On the desktop the mouse and the keyboard work as
well, and all three stay live at once.

| Input | Action |
|---|---|
| Left mouse button | Everything a finger did: menus, the virtual pad, the sensitivity slider |
| Arrows / WASD | Move; down also crouches, the way the on-screen down button does |
| Ctrl | Jump |
| Space or Enter | Action — pick up, switch, board a vehicle |
| Esc | Pause, and resume from the pause screen |

Up climbs (`speedY = -1`, what ladders, swimming and the helicopter read); it is not a second jump
key.

`Game1` hardcodes `IsFullScreen = true` and never sets a preferred back buffer, so XNA picks the
fullscreen mode itself. The art is drawn 1:1 for the phone's 800x480 screen and is cropped, not
scaled, on a larger mode — an 800x480 display reproduces the original framing exactly. A Wine
virtual desktop does not pin it, because the game changes the display mode from inside.

## Changes to the decompiled source

- `Program.cs` — rewritten as a classic `Main`; the committed version used top-level statements and
  named a namespace that does not exist in this assembly.
- `Resource.cs` — ILSpy emitted the class attributes above the `namespace`, which is not legal C#.
- `InputPad.cs` — one `((AccelerometerReading)(ref …))` decompiler artifact.
- `Game1.cs`, `Pixmap.cs`, `Decor.cs` — dropped a vestigial `using static System.Net.Mime.MediaTypeNames;`.
- `Worlds.cs` — `IsolatedStorageFile.GetUserStoreForAssembly()` in place of
  `GetUserStoreForApplication()`. A plain desktop executable has no application identity, so the
  original call threw on the first save-data read.

`Microsoft.Devices.Sensors` does not exist off the phone; the accelerometer stubs the FNA port
already carries stand in for it, so the sensor code compiles unchanged and simply never reports.

### Mouse and keyboard

`InputPad.Update` used to walk the `TouchPanel` state directly. It now walks a list of *pointers*
that merges the touch state with the mouse while its left button is down, so the menus, the virtual
pad and the slider all work with a mouse through the code that was already there.

Pointer positions go through `Pixmap.ScreenToDraw` first. The buttons, the pad and the slider are
hit-tested in the `DrawBounds` space that `GetDstRectangle` scales by `zoom` on its way to the
screen, and only at 800x480 is that zoom 1 — which is all the phone ever ran at, so the original
code could hit-test the raw touch position. At 1280x1024 the zoom is 2 and every press landed at
twice its true position, far outside the window; that is what made the cursor look dead. The same
conversion fixes touch on any display that is not 800x480.

Keys arrive on two paths. Movement is applied last, after the accelerometer leg — `Accelerometer.Start()`
cannot report that a desktop has no sensor, so that leg would otherwise zero the pad and leave the
game unsteerable if the sensor option is on. Keys that stand in for an on-screen button go through
the same `buttonGlygh` variable as a touch, so the press/release edge detection and `ButtonPressed`
behave identically.
