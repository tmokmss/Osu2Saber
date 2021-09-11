# Osu2Saber
This software converts osu beatmap files (*.osu) to Beat Saber beatmap files.

## Feature
* Automatic Beat Saber beatmaps generation from osu! beatmaps
* Generated beatmaps currently support:
    * Notes placement and their cut direction
    * Light control 
    * and being updated continuously!
* Batch process for multiple beatmaps
    * You can make hundreds of beatmaps just in 5 minutes!
* You can play osu! beatmaps in modern Beat Saber version from Steam

## How to Use
1. Download the executable from [GitHub Release](https://github.com/Ivan_Alone/Osu2Saber/releases). We recommend to download FFmpeg version to everybody.
2. Extract all the files in zip and run Osu2Saber.exe.
3. Press "Open Files" button and open valid osu archive files (*.osz or *.zip)
    * You can download them [here (official)](https://osu.ppy.sh/beatmapsets) or [here (unofficial mirror, faster download)](https://bloodcat.com/osu/?q=&c=b&s=&m=&g=&l=)
    * For osu players, you can just compress `osu!\Songs\[ur-favorite-beatmap]` folders respectively into zip files
        * **Caution**: make sure each zip file contains **only one** .mp3 and several .osu for the song
4. The file list you selected should show up, then press "Process" button.
![list](img/List.png)
5. Wait until the progress bar is filled
6. When completed, the output folder should be opened. The generated beatmaps are in it.

## About ths version
- Updated to support modern Beat Saber beatmaps format. Now you can use this converter again to play osu! beatmaps in Steam version of game.
- Modified export from .json to .dat 2.0.0 format.
- Now you can do not open output directory after conversion, if it annoys you.
- Now conversion do not includes osu!catch and osu!mania beatmaps versions by default.
- Added conversion via FFmpeg instead of embedded converter (that has bug with conversion some mp3 files to ogg). FFmpeg conversion using by default and is not disableable. ```FFmpeg.exe``` must exists in PATH, or in same forder with ```Osu2Saber.exe```. Release has 2 types of builds, with and without FFmpeg (useful for peoples that has FFmpeg installed). If FFmpeg is not found - converter will process music with embedded module.

## Dependency
This software depends on the following open source libraries:
* [osuBMParser](https://github.com/Razacx/osuBMParser)
* [.NET Ogg Vorbis Encoder](https://github.com/SteveLillis/.NET-Ogg-Vorbis-Encoder)
* [FFmpeg](https://github.com/ffmpeg/ffmpeg) (soft-dependency)

## Credits
- [Beat Saber](https://beatsaber.com/) Developers for amazing game.
- [tmokmss](https://github.com/tmokmss) for this amazing converter.
- [MCJack123](https://github.com/MCJack123) for his [1.5.0 to 2.0.0 beatmaps converter](https://gist.github.com/MCJack123/892b936ef0d18da43ab2764ba97402be) and convertion algorhitm.
- [FFmpeg Team](https://github.com/ffmpeg) for FFmpeg.
- [Ivan_Alone](https://github.com/Ivan-Alone) (myself, hehe) for this update and fix.
