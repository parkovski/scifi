# SciFighters

A mobile fighting game based on historical scientists

![Gameplay](/Design/promo/game.png "Da Vinci catches Newton with the Flying Machine")

## Table of Contents

> [License](#License)\
> [Overview](#Overview)\
> [How to Play](#How-to-Play)\
> [Characters and Attacks](#Characters-and-Attacks)\
> [Design Overview](#Design-Overview)\
> [In Progress and Future Work](#In-Progress-and-Future-Work)
>> [AI](#AI)\
>> [Item System](#Item-System)\
>> [Code Quality](#Code-Quality)\
>> [General](#General)

-----

## License

*WIP* - My intention is to open source this project, but I haven't chosen a license yet, so everything is copyright me (github.com/parkovski) for now.

-----

## Overview

The gameplay is similar to other fighting games, although more item/magic based than physical. The objective is to knock your opponent off the stage. Each character has three attacks based on their inventions/discoveries and a shield. There are also several items that can spawn randomly on the stage, although this system is off by default as I intended to replace it with collectable, upgradable items.

Some attacks have special properties that enable combos:

- Fire + Electricity
- Fire + Gun/Dynamite
- Gravity well + Projectile

-----

## How to Play

On a touch screen the controls appear on screen. Testing controls on a keyboard are:

- Move: Left/right. Double tap to run.
- Jump: Up. Tap again to double jump.
- Shield: Down. The shield is mostly unimplemented at the moment.
- Attacks: z x y or , . /
- Item: v or enter. A new item system is in progress that uses a custom drawn picker in the corner of the screen.

Some items and attacks are immediate and constant strength; some can charge up by holding the button down.

-----

## Characters and Attacks

### Isaac Newton

![Isaac Newton](Design/promo/newton.png "Isaac Newton")

Attacks: Apple, Calculus book, Gravity well

-----

### Lord Kelvin

![Lord Kelvin](Design/promo/kelvin.png "Lord Kelvin")

Attacks: Ice ball, Fireball, Broken telegraph

-----

### Alfred Nobel

![Alfred Nobel](Design/promo/nobel.png "Alfred Nobel")

Attacks: Gun, Dynamite, Gelignite

-----

### Leonardo da Vinci

![Leonardo da Vinci](Design/promo/davinci.png "Leonardo da Vinci")

Attacks: Bone arm, Flying machine, Paintbrush

-----

## Design Overview

The game is split into several systems. Core game code is in `Assets/SciFi`.

- GameController (`Game/GameController.cs`) - sort of a monstrosity; refactoring in progress. This manages shared state during the actual game.
- Player (`Players/Player.cs`) - the other monster file. This manages most of the state of a player and its response to events and user input.
- Input (`Game/Input/*`) - abstracts user input. Implementations include `InputManager` for user input, `AIInputManager` for computer players, and `NullInputManager` to ignore input.
- Attacks (`Players/Attacks/*` and `Items/*`) - manages attack behavior.
- Hooks (`Players/Hooks/*`) - player behavior stack. Events that call a hook have overlappable properties where certain game conditions or stages can change how the player behaves for a certain amount of time.
- Network (`Game/Network/*`) - mainly handles multiplayer setup. Other components handle their own network state.
  - Web (`Game/Network/Web/*`) - leaderboard, logins, and other long-term state.
- UI (`UI/*`) - touch inputs, camera, other UI elements, layout and screen size/resolution management.
- AI (`AI/*`)
  - v0/placeholder - `DumbAI.cs` - picks actions randomly.
  - v1/Strategy1 - `StrategyAI.cs`, `Strategies`, `StrategyInfra` - attribute based simple strategy picker.
  - v2/Strategy2 - `S2/*` - multithreaded configuration-based strategy picker, in progress.

## In Progress and Future Work

### AI

A new AI design is in progress using a multithreaded strategy picker. The basic idea is that each strategy evaluates itself with a percent advantage and then is compared to other strategies.

Strategies need to be tuned via feedback. To enable this, events are being moved to a generic state change listener system to record and graph events.

### Item System

Items originally spawned randomly on the stage. The system is being changed in these ways:

- Items and attacks are upgradable via collectable treasure chests and cards.
- In-game, items are spawned in a random order chosen from a set of the player's choice. An item picker control is being created for the touch screen (`Shaders/ItemPicker.shader`).
- To support this system, player profiles, leaderboards, and timed collectables are needed.

### Code Quality

There are several places in need of refactoring:

- The current system for networked code uses prefixed variables and `isServer`/`isClient` checks. This is not ideal and it should be refactored into separate server/client/common components.
- Most code does not have tests; redesigns should keep in mind ease of testing.
- `GameController.cs` and `Player.cs` are too monolithic and need to be split into smaller components.
- UI code is confusing at times and needs better abstractions.

### General

These are mostly user friendliness and polishing tasks.

- Graphics, animation, visual and sound effects are very primitive.
- Most of the pre-game UI is not yet built.
- Control flow in multiplayer setup is very finnicky.
- Single player mode uses a dummy/loopback network component; abstractions should be created to allow the game to run without networking.
- Tutorial callouts
