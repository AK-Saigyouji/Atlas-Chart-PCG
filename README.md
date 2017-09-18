# Atlas-Chart Procedural Generation

This is a Unity framework I'm experimenting with for a type of procedural generation commonly found in ARPGs (Action Role Playing Games), such as Diablo 2 and Path of Exile. It is based on the abstract notions of charts, which map out a local section of a level; atlases, which are a complete collection of charts; and markers, which represent content in a chart. A concrete example of this is the room and maze style of maps found in Diablo 2. A simple maze algorithm generates a number of connected cells, and hand-crafted rooms are slotted into those cells to flesh out the entire level. 

The heart of this framework is a set of editor tools for building these charts. It is usable in its current state but there are still some features I wish to flesh out, and adding a few sample projects will be useful in helping to understand how to put everything together. In the meantime, I've put together an overview to illustrate what the project is about. 

## Overview

Here's a brief overview of the intended workflow for creating a level generator.

First, simple black and white tile maps are drawn in any paint program. Black pixels are interpreted as unwalkable areas, white pixels are interpreted as walkable areas. This represents the walkable/unwalkable areas of a single chart. Here's an example, amplified 8x (the original is 25 by 25 pixels, representing 25 by 25 tiles). Those familiar with Diablo 2 may recognize it as an actual level chunk from the Den of Evil. 

![Chart Map](https://i.imgur.com/LpOf7VY.png)

Next, a "Marker Palette" is constructed to define types of content that can be placed manually onto each chart. There's a lot to be said about this object, but the idea is to create a layer of indirection in front of the placement of content onto charts. A trivial marker palette could define a marker preset for each instance of content: one for each enemy, each environmental object, etc. So we might have markers for Goblin, Skeleton, Skeleton Archer, Destructible Barrel, etc. A less trivial marker palette would instead define abstract markers. In the example below we see Minor Encounter, Major Encounter, Start, End, etc. By using such abstractions we can define a scheme for the content found in multiple levels, so that the same palette and even charts can be re-used for very different levels with very different content. We'd simply need to associate those abstractions with different sets of content for each level. 

![Marker Palette](https://i.imgur.com/WhmM0yX.png)

Next, content markers are applied to charts using the Chart Editor. The Chart Editor is a custom window I wrote specifically for this purpose. By attaching the marker palette from above, the context menu shown below automatically gets populated with all the marker presets from the palette. Once we've populated a chart with markers, we have a complete abstract representation of a level chunk. 

Note that there is a tradeoff between placing markers by hand, versus writing algorithms to randomly place content automatically. The former gives you more control, but is less randomized. I think the best approach is to use a mixture of both. Diablo 2 appears to rely primarily on algorithms to place enemy encounters for most of its charts (rooms), but it also has special "theme" charts. These charts have hand-placed content, and I'm guessing the content algorithms skip over these charts entirely. These theme charts allow you to construct more interesting encounters than can be provided simply by randomly placing groups of enemies around the map, including tactical placement of obstacles, interesting enemy compositions, etc. 

![Chart Editor](https://i.imgur.com/ITpXIsR.png)

The next step is to assemble these chunks to form a complete level. In principle, there are many ways to do this. An implementation I've provided is the previously mentioned maze and room style. You can write your own maze algorithm and plug it in, but here's a fairly flexible maze generator I've provided (each cell in the maze corresponds to one chunk):

![Maze Generator](https://i.imgur.com/kkBeFiA.png)

At this point we have a complete abstract level generator (it's abstract since we still just have markers and white/black tiles, not actual content), so we need a presentation layer to interpret everything. I'm working on providing better tools for this part, but in the following example I've simply created a component that associates marker presets to a list of prefabs to be chosen and instantiated at random at the marker's location (e.g. minor encounter -> fallen, gargantuan beast, or zombie, and major encounter -> fallen shaman + 5 fallen, 3 gargantuan beasts, or 4 zombies, etc.). A more flexible approach would be to associate presets to "factories", which I will likely implement (and explain more clearly) in the future. 

That handles the markers. For the level itself, I've used my Procedural Cave Generator project to build this level. 

![Final Result](https://i.imgur.com/gSYQn0n.png)

![Final Result Close-up](https://i.imgur.com/MS9xWET.png)

## An integrated example

See my Unity Roguelike repo for a more complete example of a game making use of this framework. It uses a completely different presentation (as it is a 2D tile-based game) and also integrates the map generation with the rest of the systems. Note that the version of the framework in this repo will always be the most up to date one.

![Roguelike example](https://i.imgur.com/PTK31iB.png)

A more complete readme will be written in the (most likely near) future.
