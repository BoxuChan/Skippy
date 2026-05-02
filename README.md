# Skippy

The plugin has last been updated to function with **[Dalamud 15.0.0](https://github.com/goatcorp/Dalamud)** under FFXIV's **[7.5](https://na.finalfantasyxiv.com/lodestone/topics/detail/07320affa7e0fcd9685afcbe54fbf55405b6d822)** Patch.

## _"So that you never have to hear Gaius yap again."_

Are you tired of having to go through **Main Scenario Roulettes** while just sitting there, having to read all of that yap from NPCs that don't especially matter for the 1,000,000,000th time? You've come to the right place.

**Skippy** is a plugin that allows you to do just that. Say goodbye to those _"unskippable"_ cutscenes, by, that's right, __skipping them__.
However, if you queue with the plugin on while even one member of your party doesn't have it installed, you will end up just sitting there waiting in the cage until they are done with their cutscene, just like every instance's starting cutscene making the party unable to start early, ensuring that everyone is ready.

[![Welcome to The Praetorium](https://boxu.fr/ffxiv/praetorium.gif)](https://boxu.fr/ffxiv/praetorium.gif)

## Custom Repository Link

You may find the full list of available plugins on the **[Hako | 箱](https://github.com/BoxuChan/Hako)** GitHub page.
```
https://raw.githubusercontent.com/BoxuChan/Hako/main/repo.json
```

Or you can also just use the [Puni.sh Repository](https://puni.sh/directory/boxu):
```
https://puni.sh/api/repository/boxu
```

## Commands

- **/skippy**: Opens the settings window.
- **/skippy [on/off]**: Enables or disables Skippy.
- **/skippy [log/export]**: Exports your Dalamud log file to your Desktop.
- **/skippy [territory/zone]**: Prints your current territory ID and IntendedUse to chat.
- **/sc**: Good old sanity check with a dice roll.

## How to Use

- Import the above repository into Dalamud Settings by going into **Experimental** > **Custom Plugin Repositories**.
- Look for **Skippy** in the Dalamud Plugin Installer.
- Install the plugin and give it a little while to load.
- _(Optional)_ Queue for any kind of **Main Scenario Roulette** Dungeon/Trial in **Unsynched Party**.
- Queue for your **Main Scenario Roulette**.
- Enjoy! No more cutscenes, anywhere throughout the dungeon/trial!

[![Tell me... For whom do you fight?](https://boxu.fr/ffxiv/gaius.png)](https://boxu.fr/ffxiv/gaius.png)

## Skip Categories

Skippy covers a fairly wide range of cutscenes now, which all belong to independently toggleable categories:

| Category                    | Description                                                                                                                                                                                                                                           |
|-----------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **MSQ Roulette**            | Cutscenes that belong to the Main Scenario Roulette Dungeons & Trial. When Skip All is on, per-instance Exemptions are available. When off, you can instead pick specific instances to skip.                                                          |
| **Large-Scale Content**     | Cutscenes that may belong to large-scale content like Variant Dungeons perhaps? We have yet to map out the contents list, feel free to report what you managed to skip with it!                                                                       |
| **Gold Saucer**             | Cutscenes that are within Gold Saucer game modes. When Skip All is on, per-mode Exemptions are available (Chocobo Racing, Lord of Verminion, Triple Triad, Fall Guys, Air Force One, Mahjong). When off, you can pick specific modes to skip instead. |
| **NPC Dialogue Cutscenes**  | Cutscenes that are triggered through NPC Custom Talk dialogue. Submarine Voyage cutscenes can be toggled independently.                                                                                                                               |
| **World & Quest Cutscenes** | Cutscenes that are usually ran throughout the Overworld and Quests. We have yet to map out the contents list, feel free to report what you managed to skip with it!                                                                                   |
| **Feed Buddy Scene**        | Cutscene that belongs to the Companion Feeding Animation.                                                                                                                                                                                             |

## Risky Skips

Some people like taking risks when playing the game just for the sake of going faster in their automation or whatever, so, while the following skips are experimental, they carry a ban risk. Please only enable them if you accept full responsibility, this remains your fault in case of ban, not mine:

| Category                 | Description                                                                                        |
|--------------------------|----------------------------------------------------------------------------------------------------|
| **Ocean Fishing**        | Disabled by default. It skips any cutscenes related to Ocean Fishing.                              |
| **Crystalline Conflict** | Disabled by default. It skips the cutscene that pans around the map and displays the players list. |
| **Inn Skip**             | Disabled by default. It bypasses the Inn Login Sequence entirely.                                  |

## Auto-Party Mode

I added an extra mode to Skippy, that lets you **Auto-Enable MSQ Roulette Skip while in a 4-man Party**! When enabled, Skippy will automatically enable the MSQ Roulette skip when your party has exactly 4 players, and disable itself when the party drops below 4. It does make you unable to change the settings in MSQ Roulette Skips though, to ensure it runs fine!

## IPC

As requested by some fellow plugin developers, Skippy has 3 IPC channels that other plugins may use to figure out information about its state and configurations.

### `Skippy.IsEnabled` → `bool`
Returns whether Skippy is currently enabled or not.

```csharp
var isEnabled = pluginInterface.GetIpcSubscriber<bool>("Skippy.IsEnabled").InvokeFunc();
```

### `Skippy.GetSkippedCategories` → `string[]`
Returns the names of all the categories of skips that are currently active.

```csharp
var categories = pluginInterface.GetIpcSubscriber<string[]>("Skippy.GetSkippedCategories").InvokeFunc();
```

Possible keys: `IsEnabled`, `AutoEnable4Man`, `SkipMSQRoulette`, `ExemptPrae`, `ExemptCastrum`, `ExemptPorta`, `SkipMassivePC`, `SkipGoldSaucer`, `ExemptChocoboRacing`, `ExemptVerminion`, `ExemptTripleTriad`, `ExemptFallGuys`, `ExemptAirForceOne`, `ExemptMahjong`, `SkipCustomTalk`, `SkipNormalCutscenes`, `SkipFeedBuddy`, `SkipOceanFishing`, `SkipCrystallineConflict`, `SkipInn`.

### `Skippy.GetConfig` → `Dictionary<string, bool>`
Returns the full configuration information of Skippy as a key/value map.

```csharp
var config = pluginInterface.GetIpcSubscriber<Dictionary<string, bool>>("Skippy.GetConfig").InvokeFunc();
```
