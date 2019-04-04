Vacamole is a local multiplayer game for up to 4 people.

All the cattle escaped into the jungle and the sun is setting down. Make sure to bring them all back in time but beware of the other farmers that might want to claim them for themselves!

So hop on your couch with your friends and show them who's the best farmer.

# Setup

## Linux

Install mono + monogame.

open `Team6.CXDesktop/Content/Content.mgcb` and change

```
/platform:WindowsStoreApp
```

to

```
/platform:DesktopGL
```

Then, build the project with:
`msbuild Team6.CXDesktop/Team6.CXDesktop.csproj`

Launch with:
`mono ./Team6.CXDesktop/bin/DesktopGL/AnyCPU/Debug/Team6.CXDesktop.exe`

#### Build release

`msbuild Team6.CXDesktop/Team6.CXDesktop.csproj /p:Configuration=Release`
