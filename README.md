# Hack Pad

This mod introduces a new item: the Hack Pad. The tool provides a way to open and close Big Doors as well as disable turrets manually without requiring somebody to stay behind in the ship.

## Usage

To use the Hack Pad ensure it is charged, press the "use" key (LMB on keyboard/mouse) to turn it on and off. While the Hack Pad is powered on simply look at nearby turret or big door and hold the "interact" key (E on keyboard/mouse) to begin hacking.

## Configuration

Both the price and hacking speed of the Hack Pad are configurable.
To change these values take the following steps:
- Run the game at least once after installing the mod
- Open r2modman or Thunderstore Mod Manager
- Click "Edit Config" and look for a config entry named `BepInEx/config/com.willis.lc.portablehackpad.cfg`
- Modify `HackPad_Cost`, `HackPad_HackDuration`, and `HackPad_BatteryLife` to your desired values
- Click "Save"
- Run the game

## Features

- The MultiTool is a grabbable object within the game.
- It has a screen that displays the hacking progress.
- It can play different audio clips based on the state of the hack (idle, in progress, success, failure, abort).
- The MultiTool can be used to interact with TerminalAccessibleObjects in the game.
- The hack progress is displayed as a series of dots, representing the percentage of completion.
- The MultiTool uses batteries, which can be used up.

## Dependencies

- atomic.terminalapi
- evaisa.lethallib

## Contributing

Contributions are welcome. Please submit a pull request with your proposed changes.

## License

This project is licensed under the terms of the GPL-3.0 license.
