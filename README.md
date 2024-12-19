# Treasure Hunt: A Multiplayer Game of Risk and Reward
![Treasure Hunt Spawn Area](https://github.com/user-attachments/assets/6549e448-c2c0-49c5-956d-9458e7e88ec7)

Treasure Hunt is a game mainly focused on risk and reward. Players compete against each other to pick up the coins in a maze and make the most money in the span of five minutes.
This repository contains an uncompiled source of the game. If you are looking for an already-built project that does not need to be compiled, please head to [this link](https://github.com/Bubseatbubs/CSS432_FinalProjectBuild).

## Description
Players spawn into a randomly generated maze, which has 100 gold coins scattered throughout the maze. Players need to pick up the coins and return them to the center of the maze (back where they spawned) to stash their coins into their balance. However, blue floating skulls will wander around the map, and if a player’s spotted, they’ll begin to chase after them! If a skull touches a player,  they drop all of their currently held items and respawn back at the spawn area. The player who has stashed the most money by 5 minutes wins the game. 


### Controls
| Control  | Action  |
| -------- | ------- |
| WASD or arrow keys (↑, ↓, ←, →)  | Move your character    |
| Space                            | Pick up an item        |
| E                                | Drop an item           |

## To Compile:
1. Open the Unity Hub.
2. Click `New Project`.
3. Select the Project Template `2D (Built-In Render Pipeline)`
4. Extract and unzip the files located within `TreasureHuntGame-<branch_name>` into your project's root folder.
5. After Unity finishes compiling all of the assets, restart Unity.
6. In the Assets folder, open the scene located in `Assets/Scenes/MainScene`.
7. The project should run in the Unity Editor now. If it still doesn't run, try restarting the project again.

## Testing Multiple Connections
Hosting or joining a lobby through the Unity Editor works the same as it does in the Build. 
1. First, create a Build of the game through the Unity Editor.
2. Host on either the Unity Editor or on the Build version. Ensure that the port number is `6077` or else you will need to direct connect to the host.
3. The host's lobby should appear on the list of lobbies on the client's end. If it does not, use the direct connect option and type in the IP address `127.0.0.1` and the port number as the number you selected.

## Project Specifications
This game was developed using Unity, but the network API was developed manually. The game uses UDP for constant updates like player/enemy movement, and TCP for things like joining a game, getting the game's seed, and picking up/dropping items. The game is fully P2P so there is no server that the clients connect to, one client is the host and has the "correct" state of the game.

If you want a more detailed look into how exactly I structured the networking code, please take a look at [this link](https://docs.google.com/document/d/1tOlMuKUA6JnFKkPXRA4OJvJRSxx45xVe2ZRsxFfbotc), though note that some aspects of the code may be out of date.
This project moved repositories. The older repository of this project can be found [here](https://github.com/Bubseatbubs/TreasureHunt).
