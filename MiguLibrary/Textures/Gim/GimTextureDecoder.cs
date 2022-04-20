using PuyoTools.Core.Textures.Gim.PaletteCodecs;
using PuyoTools.Core.Textures.Gim.PixelCodecs;
using System;
using System.IO;
using System.Linq;
using System.Text;
using MiguLibrary;
using MiguLibrary.IO;

namespace PuyoTools.Core.Textures.Gim
{
    public class GimTextureDecoder
    {
        private PaletteCodec paletteCodec; // Palette codec
        private PixelCodec pixelCodec;    // Pixel codec

        private ushort paletteEntries; // Number of entries in the palette

        private Endianness endianness;

        private bool isSwizzled; // Is the texture data swizzled?

        private static readonly byte[] magicCodeLittleEndian =
        {
            (byte)'M', (byte)'I', (byte)'G', (byte)'.',
            (byte)'0', (byte)'0', (byte)'.', (byte)'1',
            // The remaining 4 bytes present in the encoder are intentionally omitted.
        };

        private static readonly byte[] magicCodeBigEndian =
        {
            (byte)'.', (byte)'G', (byte)'I', (byte)'M',
            (byte)'1', (byte)'.', (byte)'0', (byte)'0',
            // The remaining 4 bytes present in the encoder are intentionally omitted.
        };

        private byte[] paletteData;
        private byte[] textureData;

        private byte[] decodedData;

        private int stride;
        private int pixelsPerRow;
        private int pixelsPerColumn;

        /// <summary>
        /// Gets the width.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the palette format, or null if a palette is not used.
        /// </summary>
        public GimPaletteFormat? PaletteFormat { get; private set; }

        /// <summary>
        /// Gets the pixel format.
        /// </summary>
        public GimPixelFormat PixelFormat { get; private set; }

        /// <summary>
        /// Gets the metadata, or <see langword="null"/> if there is no metadata.
        /// </summary>
        public GimMetadata Metadata { get; private set; }

        /// <summary>
        /// Open a GIM texture from a file.
        /// </summary>
        /// <param name="path">Filename of the file that contains the texture data.</param>
        public GimTextureDecoder(string path)
        {
            var stream = File.Open(path, FileMode.Open);
            Initialize(stream);
            stream.Close();
        }

        /// <summary>
        /// Open a GIM texture from a stream.
        /// </summary>
        /// <param name="source">Stream that contains the texture data.</param>
        public GimTextureDecoder(Stream source)
        {
            Initialize(source);
        }

        private void Initialize(Stream source)
        {
            // Check to see if what we are dealing with is a GIM texture
            /*if (!Is(source))
            {
                throw new Exception("Not a valid GIM texture.");
            }*/

            var startPosition = source.Position;
            var reader = new EndianBinaryReader(source, Endianness.LittleEndian);

            reader.Position = startPosition;

            // Check to see what endianness we're dealing with.
            if (reader.ReadBytes(magicCodeLittleEndian.Length).SequenceEqual(magicCodeLittleEndian))
            {
                endianness = Endianness.LittleEndian;
            }
            else
            {
                endianness = Endianness.BigEndian;
            }

            reader.Endianness = endianness;

            paletteEntries = 0;

            // A GIM is constructed of different chunks. They do not necessarily have to be in order.
            int eofOffset = -1;

            reader.Position += 0x08;
            while (source.Position < source.Length)
            {
                var chunkPosition = source.Position;
                int chunkLength;

                var chunkType = reader.ReadUInt16();
                source.Position += 2; // 0x04

                switch (chunkType)
                {
                    case 0x02: // EOF offset chunk

                        eofOffset = reader.ReadInt32() + 16;

                        // Get the length of this chunk
                        chunkLength = reader.ReadInt32();

                        break;

                    case 0x03: // Metadata offset chunk

                        // Skip this chunk. It's not necessary for decoding this texture.
                        source.Position += 4; // 0x08
                        chunkLength = reader.ReadInt32();

                        break;

                    case 0x04: // Texture data chunk

                        // Get the length of this chunk
                        source.Position += 4; // 0x08
                        chunkLength = reader.ReadInt32();

                        // Get the pixel format & codec
                        source.Position += 8; // 0x14
                        PixelFormat = (GimPixelFormat)reader.ReadUInt16();
                        pixelCodec = PixelCodecFactory.Create(PixelFormat);

                        // Get whether this texture is swizzled
                        isSwizzled = reader.ReadUInt16() == 1;

                        // Get the texture dimensions
                        Width = pixelsPerRow = reader.ReadUInt16();
                        Height = pixelsPerColumn = reader.ReadUInt16();

                        // If we don't have a known pixel codec for this format, that's ok.
                        // This will allow the properties to be read if the user doesn't want to decode this texture.
                        // The exception will be thrown when the texture is being decoded.
                        if (pixelCodec is null)
                        {
                            break;
                        }

                        // Get the pixels per row and pixels per column.
                        source.Position += 2; // 0x1E
                        var strideAlignment = reader.ReadUInt16();
                        var heightAlignment = reader.ReadUInt16();

                        stride = (int)Math.Ceiling((double)Width * pixelCodec.BitsPerPixel / 8);
                        if (stride % strideAlignment != 0)
                        {
                            stride = MathHelper.RoundUp(stride, strideAlignment);
                            pixelsPerRow = stride * 8 / pixelCodec.BitsPerPixel;
                        }

                        if (pixelsPerColumn % heightAlignment != 0)
                        {
                            pixelsPerColumn = MathHelper.RoundUp(pixelsPerColumn, heightAlignment);
                        }

                        // Read the texture data
                        reader.ReadAtOffset(chunkPosition + 0x50, () => textureData = reader.ReadBytes(stride * pixelsPerColumn));

                        break;

                    case 0x05: // Palette data chunk

                        // Get the length of this chunk
                        source.Position += 4; // 0x08
                        chunkLength = reader.ReadInt32();

                        // Get the palette format & codec
                        source.Position += 8; // 0x14
                        PaletteFormat = (GimPaletteFormat)reader.ReadUInt16();
                        paletteCodec = PaletteCodecFactory.Create(PaletteFormat.Value);

                        // Get the number of entries in the palette
                        source.Position += 2; // 0x18
                        paletteEntries = reader.ReadUInt16();

                        // If we don't have a known palette codec for this format, that's ok.
                        // This will allow the properties to be read if the user doesn't want to decode this texture.
                        // The exception will be thrown when the texture is being decoded.
                        if (paletteCodec is null)
                        {
                            break;
                        }

                        // Read the palette data
                        reader.ReadAtOffset(chunkPosition + 0x50, () => paletteData = reader.ReadBytes(paletteEntries * paletteCodec.BitsPerPixel / 8));

                        break;

                    case 0xFF: // Metadata chunk

                        // Get the length of this chunk
                        source.Position += 4; // 0x08
                        chunkLength = reader.ReadInt32();

                        // Read the metadata
                        source.Position += 4; // 0x10

                        Metadata = new GimMetadata
                        {
                            OriginalFilename = reader.ReadString(StringBinaryFormat.NullTerminated),
                            User = reader.ReadString(StringBinaryFormat.NullTerminated),
                            Timestamp = reader.ReadString(StringBinaryFormat.NullTerminated),
                            Program = reader.ReadString(StringBinaryFormat.NullTerminated)
                        };

                        break;

                    default: // Unknown chunk
                        throw new Exception($"Unknown chunk type {chunkType:X}");
                }

                // Verify that the chunk length will allow the stream to progress
                if (chunkLength <= 0)
                {
                    throw new Exception("Chunk length cannot be zero or negative.");
                }

                // Go to the next chunk
                source.Position = chunkPosition + chunkLength;

                // Stop reading if all of the chunks have been read
                if (source.Position - startPosition == eofOffset)
                {
                    break;
                }
            }

            // Verify that the stream's position is as the expected position
            if (source.Position - startPosition != eofOffset)
            {
                throw new Exception("Stream position does not match expected end-of-file position.");
            }
        }

        // Decodes a texture
        private byte[] DecodeTexture()
        {
            // Verify that a palette codec (if required) and pixel codec have been set.
            if (pixelCodec == null)
            {
                throw new NotSupportedException($"Pixel format {PixelFormat:X} is not supported for decoding.");
            }
            if (paletteCodec == null && pixelCodec.PaletteEntries != 0)
            {
                throw new NotSupportedException($"Palette format {PaletteFormat:X} is not supported for decoding.");
            }

            if (paletteData != null) // The texture contains an embedded palette
            {
                pixelCodec.Palette = paletteCodec.Decode(paletteData);
            }

            if (isSwizzled)
            {
                return pixelCodec.Decode(Unswizzle(textureData, stride, pixelsPerColumn), Width, Height, pixelsPerRow, pixelsPerColumn);
            }

            return pixelCodec.Decode(textureData, Width, Height, pixelsPerRow, pixelsPerColumn);
        }

        /// <summary>
        /// Gets unswizzled pixel data with no decoding.
        /// </summary>
        /// <returns>The pixel data as a byte array.</returns>
        public byte[] GetRawPixelData()
        {
            if(isSwizzled)
                return Unswizzle(textureData, stride, pixelsPerColumn);

            return textureData;
        }

        /// <summary>
        /// Decodes the texture and returns the pixel data.
        /// </summary>
        /// <returns>The pixel data as a byte array.</returns>
        public byte[] GetPixelData()
        {
            if (decodedData == null)
            {
                decodedData = DecodeTexture();
            }

            return decodedData;
        }

        private static byte[] Unswizzle(byte[] source, int stride, int pixelsPerColumn)
        {
            int destinationIndex = 0;

            byte[] destination = new byte[stride * pixelsPerColumn];

            int rowblocks = stride / 16;

            for (int y = 0; y < pixelsPerColumn; y++)
            {
                for (int x = 0; x < stride; x++)
                {
                    int blockX = x / 16;
                    int blockY = y / 8;

                    int blockIndex = blockX + (blockY * rowblocks);
                    int blockAddress = blockIndex * 16 * 8;

                    destination[destinationIndex] = source[blockAddress + (x - blockX * 16) + ((y - blockY * 8) * 16)];
                    destinationIndex++;
                }
            }

            return destination;
        }

        /// <summary>
        /// Determines if this is a GIM texture.
        /// </summary>
        /// <param name="source">The stream to read.</param>
        /// <returns>True if this is a GIM texture, false otherwise.</returns>
        /// 
        // This originally was much more complex but I didn't really know why so I just simplified it
        public static bool Is(Stream source)
        {
            long startPosition = source.Position;

            using(var reader = new EndianBinaryReader(source, Endianness.LittleEndian))
            {
                byte[] magic = new byte[8];

                reader.ReadAtOffset(startPosition, () =>
                {
                    magic = reader.ReadBytes(8);
                });

                if (magic.SequenceEqual(magicCodeLittleEndian) || magic.SequenceEqual(magicCodeBigEndian))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if this is a GIM texture.
        /// </summary>
        /// <param name="file">Filename of the file that contains the data.</param>
        /// <returns>True if this is a GIM texture, false otherwise.</returns>
        public static bool Is(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                return Is(stream);
            }
        }
    }
}