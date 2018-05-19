# osuBMParser

This project is something I made as a preparation for making an osu! clone (educational purposes).

The osuBMParser project contains all that is needed for beatmap parsing.

osuBMParserTest is a project I created to test out my code. It shows all the properties of a Beatmap object in a Windows Forms Property Grid

## How to use?

Using this class library is pretty easy.
Build the assembly from the osuBMParser project or download it [here](https://github.com/Razacx/osuBMParser/raw/master/osuBMParser/bin/Debug/osuBMParser.dll).

To parse a beatmap (.osu) file, use the following code:

```Beatmap beatmap = new Beatmap(string path);```

or

```Beatmap beatmap = new Beatmap(string path, OsuFileSection sectionsToParse)```

The Beatmap object will contain all properties found on this webpage (including lists with HitObjects, TimingPoints, etc.):
https://osu.ppy.sh/wiki/Osu_(file_format)

## New Changes
* You can now use bitwise operations for parsing certain sections of a beatmap (use OsuFileParser.OsuFileSection enum). Use the new Beatmap constructor for this behaviour.
* Default values are now set correctly.
* Removed parsing for additions, edgeAdditions and hitsounds (was not working correctly on newer beatmaps).

## To-do List
* Calculate end time for HitSliders.
* Hitsounds, additions and edgeAdditions currently don't work, since I can't find any info on them.
* Add code for writing Beatmap object back to a file?
* Parse the "Event" section of beatmap files?