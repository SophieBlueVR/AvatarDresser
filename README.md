Sophie's Avatar Dresser
=======================

A simple Unity editor tool for adding accessories to a VR Chat avatar.

[![Generic badge](https://img.shields.io/badge/Unity-2019.4.31f1-informational.svg)](https://unity.com/releases/editor/whats-new/2019.4.31)
[![Generic badge](https://img.shields.io/badge/Unity-2022.3.6f1-informational.svg)](https://unity.com/releases/editor/whats-new/2022.3.6)
[![Generic badge](https://img.shields.io/badge/SDK-AvatarSDK3-informational.svg)](https://vrchat.com/home/download)

## Usage

Drag your clothing prefab into the Unity hierarchy at the base of your scene.

Open `Tools` > `SophieBlue` > `AvatarDresser` from the menu bar.  In the
AvatarDresser window, drag your avatar and the clothing prefab into the
appropriate places, then simply click `Get Dressed!`

You can also check the box for `Create Animations` - this will create
appropriate enable/disable animations for your clothing item, add a layer to
your FX animator, and add a toggle to the selected menu.

### Scaling

A note about scaling - If the asset you are adding is not at the same scale as
your avatar, you should adjust scaling *before* running the dresser, to be sure
all armature components match properly.

For example, your avatar is at 1.25 scale but your asset is the standard 1 - in
the inspector window of the asset's root, set the x, y, and z scaling factors to
match the avatar's 1.25.  Then you should be able to run the dresser and
everything will match up fine.

## Installation

There are several methods, pick **only one**:

### VCC

Go to my [VPM Repository](https://sophiebluevr.github.io/vpm/) and simply click
`Add to VCC` next to the AvatarDresser package!

### VPM

You can also use [VRChat's VPM tool](https://vcc.docs.vrchat.com/vpm/cli/)!
First add my [VPM Repository](https://sophiebluevr.github.io/vpm/index.json)

```
vpm add repo https://sophiebluevr.github.io/vpm/index.json
```

Then you can simply go to your project directory and type:

```
vpm add package io.github.sophiebluevr.avatardresser
```

### UnityPackage

While using VCC or VPM is the preferred method, you can also download the
unitypackage from the **releases** section in this repository (on the right over
there --> ) and then install the unitypackage the usual way, from the menu bar
in Unity, going to `Assets` then `Import Package` then `Custom Package...` and
selecting the file.

## License

AvatarDresser is available as-is under MIT. For more information see
[LICENSE](/LICENSE.txt).

## Thanks

This project was initially inspired by https://github.com/artieficial/ApplyAccessories .
Lots of code reference thanks to https://github.com/VRLabs .
