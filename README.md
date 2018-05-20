# Osu2Saber
This software converts osu beatmap files (*.osu) to Beat Saber beatmap files.

## How to Use
1. Download the executable from [GitHub Release](https://github.com/tmokmss/Osu2Saber/releases).
2. Run Osu2Saber.exe
3. Press "Open Files" button and open valid osu archive files (*.osz or *.zip)
    * You can download them [here](https://osu.ppy.sh/beatmapsets)
    * For osu players, just compress your `osu!\Songs\[ur-favorite-beatmap]` folders respectively into zip files
4. The file list you selected should show up, then press "Process" button.
![list](img/List.png)
5. Wait until the progress bar is filled.
6. When completed, the output folder should be opened. The generated beatmaps are in it.

## ToDo
* The conversion (osu2BS) algorithm is still poor. Needs improvement for more fun.
    * If you can handle C#, you can fully customize it by editing [Osu2Saber.Model.Algorithm.ConvertAlgorithm.Convert](https://github.com/tmokmss/Osu2Saber/blob/master/Osu2Saber/Model/Algorithm/ConvertAlgorithm.cs) method :)
* The ogg library seems a bit slow and the output quality is bad. We may find faster conversion (mp3 to ogg) method for heavy users.
* The difficulty levels are messy, as osu! mappers use variety of words for describing their difficulty.
* Fade out a song and quit after all the notes are delivered.
* Some beatmap aren't loaded in game. The reason is still unknown.
* And your feedback are totally welcome!

## Dependency
This software depends on the following open source libraries:
* [osuBMParser](https://github.com/Razacx/osuBMParser)
* [.NET Ogg Vorbis Encoder](https://github.com/SteveLillis/.NET-Ogg-Vorbis-Encoder)