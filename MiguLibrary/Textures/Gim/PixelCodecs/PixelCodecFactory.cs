using System;
using System.Collections.Generic;
using System.Text;

namespace PuyoTools.Core.Textures.Gim.PixelCodecs
{
    internal static class PixelCodecFactory
    {
        /// <summary>
        /// Returns a pixel codec for the specified format.
        /// </summary>
        /// <param name="format"></param>
        /// <returns>The pixel codec, or <see langword="null"/> if one does not exist.</returns>
        public static PixelCodec Create(GimPixelFormat format)
        {
            switch (format)
            {
                case GimPixelFormat.Rgb565:
                    return new Rgb565PixelCodec();
                case GimPixelFormat.Argb1555:
                    return new Rgba5551PixelCodec();
                case GimPixelFormat.Argb4444:
                    return new Rgba4444PixelCodec();
                case GimPixelFormat.Argb8888:
                    return new Rgba8888PixelCodec();
                case GimPixelFormat.Index4:
                    return new Index4PixelCodec();
                case GimPixelFormat.Index8:
                    return new Index8PixelCodec();
                case GimPixelFormat.Index16:
                    return new Index16PixelCodec();
                case GimPixelFormat.Index32:
                    return new Index32PixelCodec();
                case GimPixelFormat.Dxt1:
                case GimPixelFormat.Dxt1Ext:
                    return new Dxt1PixelCodec();
                case GimPixelFormat.Dxt3:
                case GimPixelFormat.Dxt3Ext:
                    return new Dxt3PixelCodec();
                case GimPixelFormat.Dxt5:
                case GimPixelFormat.Dxt5Ext:
                    return new Dxt5PixelCodec();
                default:
                    return null;
            }
        }
    }
}
