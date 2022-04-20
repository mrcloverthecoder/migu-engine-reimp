using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MiguLibrary.IO;

namespace MiguLibrary.Sprites
{
    public class SpriteSet
    {
        public ImageEntry[] ImageEntries;
        public SpriteEntry[] SpriteEntries;

        public void Read(EndianBinaryReader reader)
        {
            if (reader.ReadString(StringBinaryFormat.FixedLength, 0x0C) != "B2D_SPRT_SET")
                throw new Exception("Not a sprite set (.BSD) file");

            // Data starts at 0x40
            reader.SeekBegin(0x40);

            int imageCount = reader.ReadInt32();

            ImageEntries = new ImageEntry[imageCount];

            for(int i = 0; i < imageCount; i++)
            {
                ImageEntry entry = new ImageEntry();

                entry.Filename = reader.ReadString(StringBinaryFormat.FixedLength, 0x20);
                entry.Id = reader.ReadInt32();

                reader.SeekCurrent(7 * 0x04);

                ImageEntries[i] = entry;
            }

            int spriteCount = reader.ReadInt32();

            SpriteEntries = new SpriteEntry[spriteCount];

            for(int i = 0; i < spriteCount; i++)
            {
                SpriteEntry entry = new SpriteEntry();

                entry.Name = reader.ReadString(StringBinaryFormat.FixedLength, 0x20);

                reader.SeekCurrent(0x0C);

                entry.Size = reader.ReadVector2();
                entry.Position = reader.ReadVector2();

                reader.SeekCurrent(0x0C);

                entry.ImageId = reader.ReadInt32();
                entry.Id = reader.ReadInt32();

                reader.SeekCurrent(0x40);

                SpriteEntries[i] = entry;
            }
        }

        public static SpriteSet FromFile(string path)
        {
            SpriteSet set = new SpriteSet();

            using(EndianBinaryReader reader = new EndianBinaryReader(File.Open(path, FileMode.Open), Encoding.GetEncoding(932), Endianness.LittleEndian))
            {
                set.Read(reader);
            }

            return set;
        }
    }
}
