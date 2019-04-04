# Setup

## Linux

Install all mono + monogame

Link the UWP content folder to the CXDesktop one:
`ln -s $(pwd)/Team6.UWP/Content/ $(pwd)/Team6.CXDesktop/Content`

Then, build the project with:
`msbuild Team6.CXDesktop/Team6.CXDesktop.csproj`

Launch with:
`mono ./Team6.CXDesktop/bin/DesktopGL/AnyCPU/Debug/Team6.CXDesktop.exe`

### known problems

If you get
`/usr/bin/chmod: changing permissions of '/usr/lib/mono/xbuild/MonoGame/v3.0/Tools/ffprobe': Operation not permitted`
warnings while building and then the game doesn't start because of
`System.IO.InvalidDataException: Could not determine container type!`
try building the content with the `MonoGame Pipeline Tool` GUI tool instead.

Open the `Team6.CXDesktop/Content/Content.mgcb` file and choose `DesktopGL` as `Platform` under `Properties > Misc` and hit build.
Then, build the game again with msbuild and it should work
