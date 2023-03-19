# Stacklands Alt Click

This adds two convenience features:

- Alt + click to pick up a card from a resource container or chest.
- Shift + alt + click to pick up five cards from a resource container or chest.

You can click multiple times while holding a stack to customize how many cards to draw.

## Manual Installation

This mod requires BepInEx to work. BepInEx is a modding framework which allows multiple mods to be loaded.

1. Download and install BepInEx from the [Thunderstore](https://stacklands.thunderstore.io/package/BepInEx/BepInExPack_Stacklands/).
2. Download this mod and extract it into `BepInEx/plugins/`
3. Launch the game

## Development

1. Install BepInEx
2. This mod uses publicized game DLLs to get private members without reflection
   - Use https://github.com/CabbageCrow/AssemblyPublicizer for example to publicize `Stacklands/Stacklands_Data/Managed/GameScripts.dll` (just drag the DLL onto the publicizer exe)
   - This outputs to `Stacklands_Data\Managed\publicized_assemblies\GameScripts_publicized.dll` (if you use another publicizer, place the result there)
3. Compile the project. This copies the resulting DLL into `<GAME_PATH>/BepInEx/plugins/`.
   - Your `GAME_PATH` should automatically be detected. If it isn't, you can manually set it in the `.csproj` file.
   - If you're using VSCode, the `.vscode/tasks.json` file should make it so that you can just do `Run Build`/`Ctrl+Shift+B` to build.

## Links

- Github: https://github.com/a-marquez/stacklands-plugin-boilerplate
  -->

## Changelog

- v0.1: Update for game version 1.2.6
