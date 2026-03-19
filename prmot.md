I need help in working with my MMORPG turn based trading card game. The game works via a integrated server (similar to how minecaft does it). This makes it so that the game can easily be from singleplayer and LAN. So when we open a save, we are basically entering into a server-client game session. 

Then there is modding. Right now mods have a structure, which are the dll and the content. The content can include stuff for cards or even UI and client side assets too. But none of the content is actually loaded into the game and isn't implemented yet. Moreover, we should be loading the mod from the save file of that game session (typically stored in appdata or godot's `user://`).

My end goal is that whenever we enter into a game session/server, we will grab and load only the client sided contents while the server sided cards and dll are on the server side.

As a professional programmer, advice and help me.

---

