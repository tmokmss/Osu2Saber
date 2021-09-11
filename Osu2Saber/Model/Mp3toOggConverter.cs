using NAudio.Wave;
using OggVorbisEncoder;
using System;
using System.IO;
using System.Diagnostics;

namespace Osu2Saber.Model
{
	class Mp3toOggConverter
	{
		private static bool run(string prog, string args = "")
		{
			Process proc = new Process();
			try {
				proc.StartInfo.FileName = prog;
				proc.StartInfo.Arguments = args;

				#if DEBUG
				#else
					proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					proc.StartInfo.CreateNoWindow = true;
				#endif

				proc.Start();
				proc.WaitForExit();
				return true;
			} catch (Exception e) {
				return false;
			}
		}
		public static string ConvertToOgg(string srcPath, string outputDir)
		{
			var ext = Path.GetExtension(srcPath);
            switch (ext)
            {
                case ".mp3":
					// Tested, some mp3 files is broken after encoding (f.e. beatmap 1287 Hatsune Miku - Ievan Polkka; song is faster x2? after encoding)
					// I do not want to search reason for it - just added this fix, native correct transcoding via FFmpeg
					var oggName = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(srcPath) + ".ogg");
					// Trying to run FFmpeg encoding 
					if (run("ffmpeg", "-i \"" + srcPath + "\" -y -vn -c:a libvorbis \"" + oggName + "\"") && File.Exists(oggName) && new System.IO.FileInfo(oggName).Length > 0)
					{
						return oggName;
					}
					return ConvertMp3toOgg(srcPath, outputDir);
                default:
                    // we give up to convert any other format for now
                    // wav and ogg are just ok for BS though
                    return JustCopy(srcPath, outputDir);
            }
        }

        public static string JustCopy(string srcPath, string outputDir)
        {
            var fname = Path.GetFileName(srcPath);
            var dstPath = Path.Combine(outputDir, fname);
            File.Copy(srcPath, dstPath, true);
            return fname;
        }

        public static string ConvertMp3toOgg(string mp3Path, string outputDir)
        {
            var oggName = Path.GetFileNameWithoutExtension(mp3Path) + ".ogg";
            try
            {
                using (var mp3 = new Mp3FileReader(mp3Path))
                using (var wav = WaveFormatConversionStream.CreatePcmStream(mp3))
                using (var ogg = new FileStream(Path.Combine(outputDir, oggName), FileMode.Create, FileAccess.Write))
                {
                    const int SampleSize = 4096;

                    // todo: if !isValid, we cannot use ogg converter
                    var isValid = wav.WaveFormat.SampleRate == 44100;
                    if (!isValid) return ConvertMp3toWav(mp3Path, outputDir);

                    // Here's a reference for the code below: 
                    //   https://github.com/SteveLillis/.NET-Ogg-Vorbis-Encoder/blob/master/OggVorbisEncoder.Example/Encoder.cs
                    var info = VorbisInfo.InitVariableBitRate(2, 44100, 0.1f);

                    // set up our packet->stream encoder
                    var serial = new Random().Next();
                    var oggStream = new OggStream(serial);

                    // =========================================================
                    // HEADER
                    // =========================================================
                    // Vorbis streams begin with three headers; the initial header (with
                    // most of the codec setup parameters) which is mandated by the Ogg
                    // bitstream spec.  The second header holds any comment fields.  The
                    // third header holds the bitstream codebook.
                    var headerBuilder = new HeaderPacketBuilder();

                    var comments = new Comments();
                    // comments.AddTag("ARTIST", "TEST");

                    var infoPacket = headerBuilder.BuildInfoPacket(info);
                    var commentsPacket = headerBuilder.BuildCommentsPacket(comments);
                    var booksPacket = headerBuilder.BuildBooksPacket(info);

                    oggStream.PacketIn(infoPacket);
                    oggStream.PacketIn(commentsPacket);
                    oggStream.PacketIn(booksPacket);

                    // Flush to force audio data onto its own page per the spec
                    OggPage page;
                    while (oggStream.PageOut(out page, true))
                    {
                        ogg.Write(page.Header, 0, page.Header.Length);
                        ogg.Write(page.Body, 0, page.Body.Length);
                    }

                    // =========================================================
                    // BODY (Audio Data)
                    // =========================================================
                    var processingState = ProcessingState.Create(info);

                    var buffer = new float[info.Channels][];
                    buffer[0] = new float[SampleSize];
                    buffer[1] = new float[SampleSize];

                    var readbuffer = new byte[SampleSize * 4];
                    while (!oggStream.Finished)
                    {
                        var bytes = wav.Read(readbuffer, 0, readbuffer.Length);

                        if (bytes == 0)
                        {
                            processingState.WriteEndOfStream();
                        }
                        else
                        {
                            var samples = bytes / 4;

                            for (var i = 0; i < samples; i++)
                            {
                                // uninterleave samples
                                buffer[0][i] = (short)((readbuffer[i * 4 + 1] << 8) | (0x00ff & readbuffer[i * 4])) / 32768f;
                                buffer[1][i] = (short)((readbuffer[i * 4 + 3] << 8) | (0x00ff & readbuffer[i * 4 + 2])) / 32768f;
                            }

                            processingState.WriteData(buffer, samples);
                        }

                        while (!oggStream.Finished
                               && processingState.PacketOut(out OggPacket packet))
                        {
                            oggStream.PacketIn(packet);

                            while (!oggStream.Finished
                                   && oggStream.PageOut(out page, false))
                            {
                                ogg.Write(page.Header, 0, page.Header.Length);
                                ogg.Write(page.Body, 0, page.Body.Length);
                            }
                        }
                    }
                }
            } catch (NAudio.MmException e)
            {
                Console.WriteLine(e);
                // Just retry
                return ConvertMp3toOgg(mp3Path, outputDir);
            } catch(InvalidOperationException e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error while converting to " + oggName);
                return oggName;
            }
            return oggName;
        }

        public static string ConvertMp3toWav(string mp3Path, string outputDir)
        {
            var wavName = Path.GetFileNameWithoutExtension(mp3Path) + ".wav";

            try
            {
                using (var mp3 = new Mp3FileReader(mp3Path))
                using (var wav = WaveFormatConversionStream.CreatePcmStream(mp3))
                {
                    WaveFileWriter.CreateWaveFile(Path.Combine(outputDir, wavName), wav);
                }
            } catch (NAudio.MmException e)
            {
                Console.WriteLine(e);
                return ConvertMp3toWav(mp3Path, outputDir);
            }
            return wavName;
        }
    }
}
