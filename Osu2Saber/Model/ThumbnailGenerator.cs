using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Osu2Saber.Model
{
    // This class converts any image file into the specified square jpeg
    class ThumbnailGenerator
    {
        public static int TargetHeight { set; get; } = 512;
        public readonly static string DefaultExtension = "jpg";

        public static void GenerateThumbnail(string imgPath, string outputDir)
        {
            // Reference:
            // http://note-sharp.blogspot.jp/2014/09/var-dir-cimages-parallel.html

            // ファイルを開いて Stream オブジェクトを作成
            using (var sourceStream = File.OpenRead(imgPath))
            {
                // 画像をデコードするための BitmapDecoder オブジェクトを作成する
                // (ファイルの種類に応じて適切なデコーダーが作成される)
                var decoder = BitmapDecoder.Create(sourceStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                // 画像ファイル内の1フレーム目を取り出す (通常1フレームしかない)
                var bitmapSource = decoder.Frames[0];

                double minEdgeLen = Math.Min(bitmapSource.PixelHeight, bitmapSource.PixelWidth);
                var scale = TargetHeight / minEdgeLen; // 拡大率

                // 拡大・縮小されたビットマップを作成する
                var scaledBitmapSource = new TransformedBitmap(bitmapSource, new ScaleTransform(scale, scale));

                var extension = "." + DefaultExtension;
                var encoder =
                    extension == ".png" ? new PngBitmapEncoder() :
                    extension == ".jpg" ? new JpegBitmapEncoder() :
                    extension == ".gif" ? new GifBitmapEncoder() :
                    extension == ".bmp" ? new BmpBitmapEncoder() :
                    (BitmapEncoder)(new PngBitmapEncoder());

                // エンコーダーにフレームを追加する
                encoder.Frames.Add(BitmapFrame.Create(scaledBitmapSource));

                // 出力ディレクトリが存在しない場合は、新しく作成する
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                var dest = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(imgPath) + extension); // 出力ファイル

                // 出力ファイルのストリームを開く
                using (var destStream = File.OpenWrite(dest))
                {
                    encoder.Save(destStream); // 保存
                }
            }
        }
    }
}
