# tetoco

This project is a BepInEx 5 plugin designed to restore functionality to **Tetote x Connect**, making it playable again.

## Features
- Restores game functionality to allow offline/local play with persistant save data.
- Unlocks test mode (press T in game)
- Removes menu timeouts
- Unlocks all songs

## Installation
- Download [BepInEx](https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.2/BepInEx_win_x64_5.4.23.2.zip) and extract the contents into the game root.
- Do a first run to generate configuration files. This should generate a BepInEx configuration file into BepInEx/config folder and an initial log file BepInEx/LogOutput.log.
- Download the [latest release](https://github.com/Redcrafter/tetoco/releases) of the plugin and place the dll in the `BepInEx/plugins` folder.
- Launch Tetote x Connect as usual. BepInEx should automatically load the plugin.

## Configuration
After running the game for the first time, a configuration file will be generated in:
```
BepInEx/config/tetoco.cfg
```
Modify this file to tweak various settings (e.g., enabling remote server connection).
