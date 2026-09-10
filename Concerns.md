# A list of concerns about the project state right now


# High Concern
Error and logging system. I don't where could the game crash. I don't have a baseline on when I should detect a crash and stop vs detect a crash and continue. Or when I should silently ignore `if null`. There is a lot of `if value is null` and similar that I don't know when to choose or log. On top of this, the entire logging message & system is 1. not standardize and 2. messy

# Medium Concern
Folder structure and file structure of some of the handlers (like packet for example). A lot dumps everything into one file which could get messy

Possible difference and usage of the `SharedIds`. We tend to store a lot of the constant data under `SharedIds`, but does all the system throughout the game uses these constants?


# Low Concern
`CardWars.BattleEngine.Shared` project references `CardWars.Vanilla.Shared`. Idk it feels strange having it there

The name GameState just sounds so wrong and generic for the battle engine.

The name `BattleRegistry` for client and possible confusion against `BattleEngineRegistry`
