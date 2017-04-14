# VSBlockJumper

VSBlockJumper is an extension for Visual Studio that allows you to jump over blocks of code. Available in the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=NightroAR.VSBlockJumper).

![Demo](./media/demo.gif?raw=true "Demo")

# Usage

Jumping takes you outside of the nearest block edge (i.e. the whitespace line adjacent to a block). If the cursor reaches BOF or EOF, we jump there instead.

## Commands

Due to existing keybindings within Visual Studio (and common extensions like ReSharper or Visual Assist), I was unable to provide intuitive default keybindings. I've listed my preferences here, and encourage you to update your keybindings under Tools>Options>Environment>Keyboard

| Command               | Description                                     | Keybinding (Default) | Keybinding (Ideal) |
|:--------------------- |:----------------------------------------------- |:-------------------- |:------------------ |
| `Edit.JumpUp`         | Jump to the closest block edge above the cursor | `Ctrl+Num -`         | `Ctrl+Up`          |
| `Edit.JumpDown`       | Jump to the closest block edge below the cursor | `Ctrl+Num +`         | `Ctrl+Down`        |
| `Edit.JumpSelectUp`   | Jump Up and add to the active selection         | `Ctrl+Shift+Num -`   | `Ctrl+Shift+Up`    |
| `Edit.JumpSelectDown` | Jump Down and add to the active selection       | `Ctrl+Shift+Num +`   | `Ctrl+Shift+Down`  |

**NOTE**: When assigning shortcuts to these commands, be sure to select `Text Editor` in the drop down labelled `Use new shortcut in:` to override existing keybindings within the scope of the Text Editor (Global will not cut it).

## Settings

Settings can be found under Tools>Options>VSBlockJumper

| Setting           | Description                                                                                                                   | Default |
|:----------------- |:----------------------------------------------------------------------------------------------------------------------------- |:------- |
| `JumpOutsideEdge` | If enabled, the cursor will jump outside of the block edge (blank line), otherwise it jumps inside the block edge (text line) | `true`  |
| `SkipClosestEdge` | If enabled, the cursor will only jump to the far edge of a block, otherwise it visits every edge of a block                   | `false` |

# Credits and Thanks

* [Casey Muratory](https://twitter.com/cmuratori) - I first saw this method for navigating code in his Handmade Hero video series
* [Space Block Jumper](https://marketplace.visualstudio.com/items?itemName=jmfirth.vsc-space-block-jumper) - I use this extension for the same purpose as VSBlockJumper in VSCode
* [Anthony Reddan](https://twitter.com/AnthonyReddan) - That's me!

![VSBlockJumper](./code/Resources/VSBlockJumperPackage.png?raw=true "VSBlockJumper")
