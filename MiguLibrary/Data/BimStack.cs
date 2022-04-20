using System;
using System.Collections.Generic;
using System.IO;
using MiguLibrary.IO;

namespace MiguLibrary.Data
{
    public class BimStack
    {
        public List<object> Values;

        public void Read(EndianBinaryReader reader)
        {
            if (!reader.ReadSignature("\"\"\"\"BIMV01", 0x0C))
                throw new Exception("Not a MBITS file!");

            bool end = false;
            List<string> strings = new List<string>();
            List<int> stringIndices = new List<int>();
            Values = new List<object>();

            while(!end)
            {
                string sectionName = reader.ReadString(StringBinaryFormat.FixedLength, 4);

                if (sectionName == "BIMI")
                {
                    int length = reader.ReadInt32();

                    int valueIdx = 0;

                    int i = 0;
                    long stPos = reader.Position;
                    while (i < length)
                    {
                        short valueLength = reader.ReadInt16();
                        reader.SeekCurrent(0x01);
                        ValueType type = (ValueType)reader.ReadByte();

                        switch(type)
                        {
                            case ValueType.SignedInt:
                                Values.Add(reader.ReadInt32());
                                break;
                            case ValueType.UnsignedInt:
                                Values.Add(reader.ReadUInt32());
                                break;
                            case ValueType.String:
                                {
                                    Values.Add("");
                                    stringIndices.Add(valueIdx);
                                    reader.SeekCurrent(0x04);
                                }
                                break;
                            case ValueType.List:
                                {
                                    reader.SeekCurrent(0x08);
                                }
                                break;
                        }

                        i = (int)(reader.Position - stPos);

                        if(type != ValueType.List)
                            valueIdx++;
                    }
                }
                else if (sectionName == "BIMH")
                {
                    int length = reader.ReadInt32();
                    reader.SeekCurrent(length);
                }
                else if (sectionName == "BIMB")
                {
                    int length = reader.ReadInt32();

                    int i = 0;
                    long stPos = reader.Position;
                    while(i < length)
                    {
                        strings.Add(reader.ReadString(StringBinaryFormat.Mbits));
                        i = (int)(reader.Position - stPos);
                    }
                }
                else if (sectionName == "\"\"\"\"")
                    end = true;
            }

            // Fixing strings
            for(int i = 0; i < stringIndices.Count; i++)
                Values[stringIndices[i]] = strings[i];
        }

        public static BimStack FromFile(string path)
        {
            BimStack stack = new BimStack();

            using(var reader = new EndianBinaryReader(File.Open(path, FileMode.Open), Endianness.LittleEndian))
            {
                stack.Read(reader);
            }

            return stack;
        }
    }

    public enum ValueType : byte
    {
        SignedInt = 0x01,
        UnsignedInt = 0x02,
        String = 0x05,
        List = 0x09
    }
}
