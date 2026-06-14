# RiftMarks

This project is a mod for Rift of the NecroDancer which allows charters to add "riftmarks" to their custom levels to improve the quality of life in practice mode. A riftmark functions as a checkpoint in the song that can be used to mark important segments or chapters. Players with the mod installed can use the practice mode slider to quickly start the song from a riftmark without having to search for the exact beat. This makes it easier to quickly practice a particular segment or jump to a late point in a long song.

> [!WARNING]
> BepInEx mods are <ins>**not officially supported**</ins> by Rift of the NecroDancer. If you encounter any issues with this mod, please open an issue on this GitHub repository, and do not submit reports to Brace Yourself Games!

The current version is <ins>**v1.0.1**</ins>. Downloads for the latest version can be found [here](https://github.com/96-LB/RiftMarks/releases/latest). The changelog can be found [here](Changelog.md).


## Installation

1. Install the latest version of BepInEx 5 and Rift of the NecroManager. You can find detailed directions on the [Rift of the NecroManager](https://github.com/96-LB/RiftOfTheNecroManager) GitHub page!

2. Navigate to the latest release of RiftMarks [here](https://github.com/96-LB/RiftMarks/releases/latest).

> [!CAUTION]
> Do NOT download the source code using the button at the top of this page. If you're downloading a `.zip` file, you are at the wrong place.

3. Expand the "Assets" tab at the bottom and download `RiftMarks.dll`.

4. Place `RiftMarks.dll` in the `BepInEx/plugins` directory inside the Rift of the NecroDancer game folder.

> [!TIP]
> You can find this folder by right clicking on the game in your Steam library and clicking 'Properties'. Then navigate to 'Installed Files' and click 'Browse'.


## Usage

### As a player

When a chart supports riftmarks, the practice mode slider will automatically be set to 'mark mode' and appear blue instead of the usual green. When in mark mode, the slider selects the start and endpoints of the practice range using riftmarks instead of beats. The name of the currently selected riftmark will be displayed next to the slider, allowing you to easily scroll through in search of a particular segment or chapter. To toggle 'mark mode', press the same button you would use to toggle the sort mode in the custom menu (by default this is TAB on keyboards).

The mod also makes a couple other minor adjustments:
- General performance optimizations, especially when starting the level in practice mode thousands of beats into a song with many traps.
- If you move the beat slider in the practice mode pause menu, returning to the game will only reload the level if the beats have changed.
- Minor bugfixes with the practice mode slider on charts with fractional beat length.
- The score display is no longer capped at 7 digits (this bugfix will likely be moved to a different mod in the future).


### As a charter

You do not need the mod installed to add riftmarks to your custom levels. In fact, all you need to do is add a file called `RiftMarks.json` to the folder with your chart files (this is the folder with `info.json`). In this file, create a JSON object with some or all of the following keys:
- Default
- Easy
- Medium
- Hard
- Impossible

Each difficulty will use the corresponding set of riftmarks if it exists, and fall back to the default riftmarks otherwise. For each key, the value should be a list of JSON objects, each representing one riftmark. A riftmark has two keys:
- **Beat** *(int)*: The beat number at which to place the riftmark.
- **Name** *(string?)*: The name associated with the mark. If omitted, the mark will be displayed as 'Beat *X*' in-game

If no riftmark is placed at beat 0, a nameless one will automatically be created.

Here is an example:
```json
{
    "Default": [
        {"Beat": 0},
        {"Beat": 100},
        {"Beat": 200},
    ],
    "Hard": [
        {"Beat": 0, "Name": "Once Upon a Time"},
        {"Beat": 174, "Name": "Start Menu"},
        {"Beat": 270, "Name": "Your Best Friend"},
        {"Beat": 322,"Name": "Fallen Down"},
        {"Beat": 430,"Name": "Ruins"}
    ]
}
```
