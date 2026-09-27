# FrizzQOL Area Repair

Repair one piece with the hammer and every damaged piece inside a sphere around it is repaired too.

Those extra pieces cost no materials and do not check for a crafting station. The default radius is 5 meters.

## Config

`BepInEx/config/com.frizzqol.arearepair.cfg`

| Setting | Default | What it does |
| --- | --- | --- |
| RepairRadius | 5 | Sphere radius in meters. 0 or less turns area repair off. |

The file is created the first time the mod loads.

Remove any older Area Repair plugin, including aruberuto-AreaRepair, before you use this one.

## Multiplayer

Install this on the dedicated server and on every client.

## Install

Install with r2modman or the Thunderstore Mod Manager.

To install by hand, copy `FrizzQOL.AreaRepair.dll` into `BepInEx/plugins`.

## Requirements

- Valheim
- [BepInExPack for Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)

## License

[MIT](LICENSE). You can use, copy, change, and share this mod. Keep the copyright notice with any copy.

## Building

1. Copy `Environment.props.example` to `Environment.props`.
2. Set your Valheim and BepInEx folders in that file.
3. From this folder, run:

```
dotnet build AreaRepair.sln -c Release
```

The plugin file is `FrizzQOL.AreaRepair.dll`, under the project `bin\Release\net48` folder.

`Environment.props` stays on your machine. It is listed in `.gitignore`.

## Source

https://github.com/frizzlebeard/FrizzQOL.AreaRepair
