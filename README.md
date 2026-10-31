# StateMachineGenerator

State machine code generator for [LogicBlocks] from [Chickensoft] (compatible with [SaveFileBuilder]).

---

Generates all boilerplate code necessary for a state machine.

Installed tool generates files in the folder where it is run.

Cloned repo will generate files in the folder: `projectRoot/bin/Debug/net10.0`

---

## Capabilities

- generates the root `AutoNode` class, logic class (with Data, Input and Output classes) and state classes
- choose if you want your files to be in a namespace
- ability to generate states that are children of other states
- optional saving logic in the `AutoNode` class for saving using the [SaveFileBuilder]
- optional Settings file
- state file names can optionally begin with `'StateMachineName'Logic.State.`

## Instructions

### Install

Open terminal and enter the following command:

```
dotnet tool install Chickensoft.StateMachineGenerator.Unofficial --version 1.0.0
```

### Run tool
You can use the tool by entering the command `statemachinegenerator` into the terminal. The files will be generated in the location where the terminal is currently positioned.

### Uninstall
To uninstall the tool enter the following commant in the terminal:
```
dotnet tool uninstall Chickensoft.StateMachineGenerator.Unofficial
```

## Generating states

When asked, enter state names (one in each line). When finished leave the state name blank.

If you just enter the name of the state, that state will be a child of `State`.

Additionally, you can use the notation `ParentState.ChildState`

> [!Note]
> Parent state doesn't have to be added beforehand. It will be a child of `State`

If you want a state that is a child of the previously entered state start with a dot: `.ChildState`

If you want a state that has the same parent as the previously entered state start with two dots: `..ChildState`

Example of generating states:
```text
Do you want a particular initial state? (enter the state name or leave blank)
Alive
------------------------------------------------------------------------------------------------------
Start entering possible states. When you are finished with the states submit a blank line to continue.
.Grounded
State Grounded with a parent Alive has been added
..Airborne
State Airborne with a parent Alive has been added
.Jumping
State Jumping with a parent Airborne has been added
Defeated.Dead
State Dead with a parent Defeated has been added

```

## Generated file hierarchy

This is an example of all files generated for a state machine called `Player` (including optional files).

Folders are marked with brackets. Files/folders that are inside a folder are moved one tab to the right of their parent for clarity.
```text
(player)
    - (state)
        - (states)
            - Alive.cs
            - Alive.Airborne.cs
            - Alive.Airborne.Jumping.cs
            - Alive.Grounded.cs
            - Dead.cs
        - PlayerLogic.cs
        - PlayerLogic.Data.cs
        - PlayerLogic.Input.cs
        - PlayerLogic.Output.cs
        - PlayerLogic.Settings.cs
    - Player.cs
    - PlayerData.cs
```

## Examples: 

### Player.cs

```csharp
namespace GameDemo;

using Chickensoft.SaveFileBuilder;
using Chickensoft.LogicBlocks;
using Chickensoft.Introspection;
using Chickensoft.Collections;
using Chickensoft.AutoInject;
using Godot;

public interface IPlayer
{
}

[Meta(typeof(IAutoNode))]
public partial class Player : Node, IPlayer
{
	public override void _Notification(int what) => this.Notify(what);

#region Save
	[Dependency] public EntityTable<uint> EntityTable => this.DependOn<EntityTable<uint>>();
	[Dependency] public ISaveChunk<GameData> GameChunk => this.DependOn<ISaveChunk<GameData>>();
	public ISaveChunk<PlayerData> PlayerChunk { get; set; } = default!;
#endregion Save

#region Dependencies
	[Dependency] public GameData GameData => this.DependOn<GameData>();
#endregion Dependencies

#region Provisions
#endregion

#region  State
	public PlayerLogic PlayerLogic { get; set; } = default!;
	public PlayerLogic.IBinding PlayerBinding { get; set; } = default!;
#endregion State

	public void Setup()
	{
		PlayerLogic = new PlayerLogic();
		PlayerLogic.Save(() => new PlayerLogic.Data() { });

		PlayerChunk = new SaveChunk<PlayerData>(
			onSave: (chunk) => new PlayerData()
			{
				StateMachine = PlayerLogic
			},
			onLoad: (chunk, data) =>
			{
				PlayerLogic.RestoreFrom(data.StateMachine);
				PlayerLogic.Start();
			}
		);
	}

	public void OnResolved()
	{
		GameChunk.AddChunk(PlayerChunk);
		PlayerBinding = PlayerLogic.Bind();
		this.Provide();
		PlayerLogic.Start();
	}

	public void OnExitTree()
	{
		PlayerLogic.Stop();
		PlayerBinding.Dispose();
	}
}
```

### PlayerLogic.State.Alive.Grounded.cs

```csharp
namespace GameDemo;

using Chickensoft.LogicBlocks;
using Chickensoft.Introspection;
using Chickensoft.Collections;
using Godot;

public partial class PlayerLogic
{
	public partial record State
	{
		[Meta, Id("player_logic_state_grounded")]
		public partial record Grounded: Alive
		{
			public Grounded()
			{
			}
		}
	}
}
```

## TODO

Expand this project so that it can easily be installed as a tool and run from the terminal in the folder where you want to generate files for ease of use.

Add an option to generate saving logic for the root node that provides the initial chunk.



[LogicBlocks]: https://github.com/chickensoft-games/LogicBlocks
[Chickensoft]: https://github.com/chickensoft-games
[SaveFileBuilder]: https://github.com/chickensoft-games/SaveFileBuilder