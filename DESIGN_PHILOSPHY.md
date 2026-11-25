# What is Card Wars
Card Wars is a:
1. large scale
2. integrated server-client
3. turn based card game MMORPG
4. Data-driven content and modding capability

The game follows the player to explore different areas/locations just like a normal rpg, which we then turn into a combat turn based card game which is extensively customizable to fit the game's MMORPG story need. It has mmorpg elements of story, quests, game modes and events like roguelikes.

# Design Philosphy
## No Global, Singleton or Static
We wanted to keep the project clean and uncluttered. Almost to the point that in the same instance, we could run 2 game sessions at once and it will not interfere with each other.

## Data-driven and Mod Compatibility
The entire game is designed as a core + vanilla split. Core exposes the modding APIs, and vanilla is built on top of those same APIs. Vanilla does not get a "special-expetion" path that other mods cannot use. As such, other mods can create whatever the vanilla base game makes. This means the game can be built, extended and tested through the same abstractions.

Modding is divided into 2 parts, content mods & DLL code injection modding.

### 1. Data driven Content Modding
Similar to minecraft's datapacks, we load json, data and files dynamically. These should be able to load at runtime. As such, we can load these data-driven content on the fly.

Data-driven content is the primary path for server-side modding. Servers (or hosting players) can add and modify content, and other players can join without installing anything, or with minimal installs for visual-only assets. This lets servers push content mods to clients without much trouble.

The same system lets us as developers add and modify content easily. We can host our own "official" server with our own content and roll out event content updates through the same pipeline.

### 2. DLL Modding
Mods (such as the server and client DLL mods) are compiled code from DLL and laoded into the game. These modding are more advance and use the APIs exposed by core. Because everything is abstracted, DLL modding allows mods to create whatever it vanilla could do.

### Modding Intents
Because everything is abstracted, the intent is to support loading and unloading on the fly, including DLLs. Hot DLL loading/unloading may not be feasible in practice, so we treat it as a goal rather than a guarantee. Data-driven content should be feasible to load/unload on the fly. That means we can load into a server not hosted by others and play with their own mods.

## Raw Design
We did not want to use any external libraries (Such as messagepack). As such, everything was to be built ground up. Such as how we created the DataTag system. This is to keep the project lean and simple.

We keep certain parts abstracted (e.g. File path systems). Instead of using Path in the core file sytems, we only use `System.IO.Path` in the actual implementation.

## Simple Design
The entire project is meant to be simple in nature. For example, there shouldn't be multiple fallbacks, gaurdrails just to keep a single feature in check.

## Split design
To keep everything modular and no violations between server, client and modding capabailiteis. We keep each project such as server core, server vanilla etc. So that we don't reference or use 

That means, all the client needs to connect to the server is just the client dll (+common dll). Similarly, for the server, if its a only server system, only server dll is needed (+common dlls). Unless we are using integrated client-server, which requires both.

## Concistency, Consistency, Consistency!
When using a certian design style choice, we need to question 1. is this a standard and optimal approach? 2. Is this approach style we used previously in other parts of the code. If we encounter whereby 1 & 2 clashes, we need to find out why we use differing approaches. Then we standardize the format to use that styling/approach.

# Code Styling
A list of patterns we use and NOT use
- Never use Null-Forgiving Operator: `!`
- Never use Casting `(T)value` unless it is really needed
