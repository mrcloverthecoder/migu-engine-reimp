namespace PuyoTools.Core.Textures.Gim.PaletteCodecs
{
    internal static class PaletteCodecFactory
    {
        /// <summary>
        /// Returns a palette codec for the specified format.
        /// </summary>
        /// <param name="format"></param>
        /// <returns>The palette codec, or <see langword="null"/> if one does not exist.</returns>
        public static PaletteCodec Create(GimPaletteFormat format)
        {
            switch(format)
            {
                case GimPaletteFormat.Rgb565:
                    return new Rgb565PaletteCodec();
                case GimPaletteFormat.Argb1555:
                    return new Rgba5551PaletteCodec();
                case GimPaletteFormat.Argb4444:
                    return new Rgba4444PaletteCodec();
                case GimPaletteFormat.Argb8888:
                    return new Rgba8888PaletteCodec();
                default:
                    return null;
            }
        }
    }
}
