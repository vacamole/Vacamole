# Vacamole

Vacamole is a local multiplayer game for up to 4 people.

All the cattle escaped into the jungle and the sun is setting down. Make sure to bring them all back in time but beware of the other farmers that might want to claim them for themselves!

So hop on your couch with your friends and show them who's the best farmer!

# Setup

## Windows

Install [MonoGame][monogame] and open the project in Visual Studio. Then start the `Team6.UWP` project.

## Linux

Install [mono][mono] + [MonoGame][monogame].

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

# Credits

All the code was written by [Alexander Kayed][alex], Moritz Zilian and [Florian Zinggeler][flo].  
All art assets were created by [Sonja Böckler][sonja].

# Licence

All the code is licensed under the GNU GPL.  
The graphical assets are licensed under CC BY 4.0. E.g. you can use them in your own projects as long as you give proper credits to [Sonja Böckler][sonja].  
Everything else (mostly the music and sound effects) was taken from various sources and has been licensed under CC-0.

[monogame]: http://www.monogame.net/
[mono]: https://www.mono-project.com/
[sonja]: http://sonjaboeckler.de/
[alex]: https://github.com/akade
[flo]: https://github.com/Zinggi
