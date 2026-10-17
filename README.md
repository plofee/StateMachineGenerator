# StateMachineGenerator

State machine code generator for [LogicBlocks] from [Chickensoft] (compatible with [SaveFileBuilder]).

---

Generates all boilerplate code necessary for a state machine.

---

## Capabilities

- generates the root `AutoNode` class, logic class (with Data, Input and Output classes) and state classes
- choose if you want your files to be in a namespace
- ability to generate states that are children of other states
- optional saving logic in the `AutoNode` class for saving using the [SaveFileBuilder]
- optional Settings file
- state file names can optionally begin with `'StateMachineName'Logic.State.`

## Generating states

When asked, enter state names one by one separated by a newline (hit Enter). When finished leave the state name blank.

If you just enter the name of the state, that state will be a child of `State`.

Additionally, you can use the notation `ParentState.ChildState`

> [!Note]
> Parent state doesn't have to be added beforehand. It will be a child of `State`

If you want a state that is a child of the previously entered state start with a dot: `.ChildState`

If you want a state that has the same parent as the previously entered state start with two dots: `..ChildState`

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
            - Alive.Dead.cs
        - PlayerLogic.cs
        - PlayerLogic.Data.cs
        - PlayerLogic.Input.cs
        - PlayerLogic.Output.cs
        - PlayerLogic.Settings.cs
    - Player.cs
    - PlayerData.cs
```


[LogicBlocks]: https://github.com/chickensoft-games/LogicBlocks
[Chickensoft]: https://github.com/chickensoft-games
[SaveFileBuilder]: https://github.com/chickensoft-games/SaveFileBuilder