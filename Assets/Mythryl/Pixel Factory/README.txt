2D Pixel Factory
Free edition, version 1.0
by Mythryl Studio

2D isometric factory sprites, animated: belts, machines, power, oil, ore.

DOCUMENTATION

This file is the pack's documentation: its sizes, how it works and how it's set up in Unity.

SIZES

32px/ and 64px/: the same pack at each size, each with its own example scene.

HOW THIS PACK WORKS

Isometric factory sprites at 32px, for 32x32 isometric tiles (32 x 16 diamonds). Every piece takes whole tiles: a belt one, an assembler three by three, and sits on the middle of the tiles it covers.

UNITY

Every sheet is sliced into sprites named piece_turn_frame, one unit to a tile's width, with the pivot on the middle of the footprint. Every animated piece has an Animation Clip per turn (switching its Body and Shadow together) and an Animator Controller with a state per turn; every piece has a prefab with a Shadow and a Body, in each palette. The example scene lays out a small working line, stacked by sorting order.

All images are PNG with transparency. Scale by whole numbers
(2x, 3x, 4x) with nearest-neighbour filtering to keep pixels crisp.

This is the free edition. The full pack is available at
https://mythrylstudio.itch.io/pixel-factory

SUPPORT

hello@mythryl.com
https://mythryl.com

LICENCE

Unity Asset Store EULA.
