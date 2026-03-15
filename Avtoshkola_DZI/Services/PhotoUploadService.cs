using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Avtoshkola_DZI.Services
{
    /// <summary>
    /// Преобразува качени снимки в двоичен формат за запис в БД (без запис на файлове).
    /// </summary>
    public class PhotoUploadService
    {
        // Максимален размер на снимката в БД
        private const int MaxSizeBytes = 10 * 1024 * 1024; // 10 MB

        /// <summary>
        /// Чете файла като byte[] за запис в БД.
        /// Ако е по-голям от MaxSizeBytes, опитва да го компресира (преоразмери и запише като JPEG).
        /// Връща null при липса на файл или при невъзможност за четене/компресия.
        /// </summary>
        public async Task<byte[]?> GetPhotoBytesAsync(IFormFile? file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0) return null;

            try
            {
                await using var ms = new System.IO.MemoryStream((int)file.Length);
                await file.CopyToAsync(ms, cancellationToken);
                ms.Position = 0;
                var bytes = ms.ToArray();
                if (bytes.Length == 0) return null;
                if (bytes.Length <= MaxSizeBytes)
                    return bytes;

                // Пробваме да компресираме/преоразмерим, ако е твърде голяма
                var compressed = CompressImage(bytes);
                if (compressed != null && compressed.Length <= MaxSizeBytes)
                    return compressed;

                // Ако и след компресията е прекалено голямо, не записваме
                return null;
            }
            catch
            {
                return null;
            }
        }

        private static byte[]? CompressImage(byte[] originalBytes)
        {
            try
            {
                using var inputStream = new System.IO.MemoryStream(originalBytes);
                using var image = System.Drawing.Image.FromStream(inputStream);

                // Ограничаваме до разумен размер (макс 1600px по дългата страна)
                const int maxDimension = 1600;
                var ratio = Math.Min(
                    (double)maxDimension / image.Width,
                    (double)maxDimension / image.Height);

                if (ratio > 1.0) ratio = 1.0; // не увеличаваме малки снимки

                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                using var resized = new System.Drawing.Bitmap(newWidth, newHeight);
                using (var g = System.Drawing.Graphics.FromImage(resized))
                {
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(image, 0, 0, newWidth, newHeight);
                }

                // Записваме като JPEG с постепенно намаляващо качество
                var jpegCodec = System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders()
                    .FirstOrDefault(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
                if (jpegCodec == null)
                {
                    using var fallbackMs = new System.IO.MemoryStream();
                    resized.Save(fallbackMs, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return fallbackMs.ToArray();
                }

                long quality = 85L;
                while (quality >= 40L)
                {
                    using var ms = new System.IO.MemoryStream();
                    using var ep = new System.Drawing.Imaging.EncoderParameters(1);
                    ep.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                    resized.Save(ms, jpegCodec, ep);
                    var result = ms.ToArray();
                    if (result.Length <= MaxSizeBytes)
                        return result;

                    quality -= 15L;
                }

                // Връщаме най-ниското качество, дори да е по-голямо, за да може контролерът да прецени
                using var finalMs = new System.IO.MemoryStream();
                resized.Save(finalMs, System.Drawing.Imaging.ImageFormat.Jpeg);
                return finalMs.ToArray();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Определя MIME тип по разширение на файла.
        /// </summary>
        public static string GetContentType(string? fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return ext switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };
        }
    }
}
