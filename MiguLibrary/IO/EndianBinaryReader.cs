/*                                                              */
/* Taken and modified from https://github.com/TGEnigma/Amicitia */
/*                                                              */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Numerics;

namespace MiguLibrary.IO
{
    public class EndianBinaryReader : BinaryReader
    {
        private Endianness mEndianness;
        private Encoding mEncoding;
        private Stack<long> mBaseOffset;
        private List<byte> mStringBuilder;

        public Endianness Endianness
        {
            get { return mEndianness; }
            set
            {
                if (value != EndiannessHelper.SystemEndianness)
                    SwapBytes = true;
                else
                    SwapBytes = false;

                mEndianness = value;
            }
        }

        public bool SwapBytes { get; private set; }

        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public long Length => BaseStream.Length;

        public EndianBinaryReader(Stream input, Endianness endianness)
            : base(input)
        {
            Init(Encoding.Default, endianness);
        }

        public EndianBinaryReader(Stream input, Encoding encoding, Endianness endianness)
            : base(input, encoding)
        {
            Init(encoding, endianness);
        }

        public EndianBinaryReader(Stream input, Encoding encoding, bool leaveOpen, Endianness endianness)
            : base(input, encoding, leaveOpen)
        {
            Init(encoding, endianness);
        }

        private void Init(Encoding encoding, Endianness endianness)
        {
            mStringBuilder = new List<byte>();
            mEncoding = encoding;
            Endianness = endianness;
            mBaseOffset = new Stack<long>();
            mBaseOffset.Push( 0 );
        }

        public void Align(int alignment)
        {
            while (Position % alignment != 0)
                SeekCurrent(1);
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            BaseStream.Seek(offset, origin);
        }

        public void SeekBegin(long offset)
        {
            BaseStream.Seek(offset, SeekOrigin.Begin);
        }

        public void SeekCurrent(long offset)
        {
            BaseStream.Seek(offset, SeekOrigin.Current);
        }

        public void SeekEnd(long offset)
        {
            BaseStream.Seek(offset, SeekOrigin.End);
        }

        public void PushBaseOffset( long baseOffset ) => mBaseOffset.Push( baseOffset );

        public void PopBaseOffset() => mBaseOffset.Pop();

        public void ReadOffset( Action action )
        {
            var offset = ReadInt32() + mBaseOffset.Peek();
            if ( offset != 0 )
                ReadAtOffset( offset, action );
        }

        public void ReadOffset( int count, Action action )
        {
            ReadOffset( () =>
            {
                for ( int i = 0; i < count; ++i )
                    action();
            } );
        }

        public void ReadAtOffset( long offset, Action action )
        {
            long current = Position;
            SeekBegin( offset );
            action();
            SeekBegin( current );
        }

        public Vector2 ReadVector2()
        {
            Vector2 vector = new Vector2();
            vector.X = ReadSingle();
            vector.Y = ReadSingle();

            return vector;
        }

        public Vector3 ReadVector3()
        {
            Vector3 vector = new Vector3();
            vector.X = ReadSingle();
            vector.Y = ReadSingle();
            vector.Z = ReadSingle();

            return vector;
        }
        public Vector4 ReadVector4()
        {
            Vector4 vector = new Vector4();
            vector.X = ReadSingle();
            vector.Y = ReadSingle();
            vector.Z = ReadSingle();
            vector.W = ReadSingle();

            return vector;
        }

        public Matrix4x4 ReadMatrix4x4()
        {
            float m11 = ReadSingle();
            float m21 = ReadSingle();
            float m31 = ReadSingle();
            float m41 = ReadSingle();
            float m12 = ReadSingle();
            float m22 = ReadSingle();
            float m32 = ReadSingle();
            float m42 = ReadSingle();
            float m13 = ReadSingle();
            float m23 = ReadSingle();
            float m33 = ReadSingle();
            float m43 = ReadSingle();
            float m14 = ReadSingle();
            float m24 = ReadSingle();
            float m34 = ReadSingle();
            float m44 = ReadSingle();

            return new Matrix4x4(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);
        }

        public bool ReadSignature(string signature, int size = 4)
        {
            return ReadString(StringBinaryFormat.FixedLength, size) == signature;
        }

        public Color ReadColor(ColorFormat fmt = ColorFormat.RGBA)
        {
            Color color = new Color();

            if (fmt == ColorFormat.RGBA)
            {
                color.R = ReadByte();
                color.G = ReadByte();
                color.B = ReadByte();
                color.A = ReadByte();
            }
            else if(fmt == ColorFormat.BGRA)
            {
                color.B = ReadByte();
                color.G = ReadByte();
                color.R = ReadByte();
                color.A = ReadByte();
            }

            return color;
        }

        public sbyte[] ReadSBytes(int count)
        {
            sbyte[] array = new sbyte[count];
            for (int i = 0; i < array.Length; i++)
                array[i] = ReadSByte();

            return array;
        }

        public bool[] ReadBooleans(int count)
        {
            bool[] array = new bool[count];
            for (int i = 0; i < array.Length; i++)
                array[i] = ReadBoolean();

            return array;
        }

        public override short ReadInt16()
        {
            if (SwapBytes)
                return EndiannessHelper.Swap(base.ReadInt16());
            return base.ReadInt16();
        }

        public short[] ReadInt16s(int count)
        {
            short[] array = new short[count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ReadInt16();
            }

            return array;
        }

        public override ushort ReadUInt16()
        {
            if (SwapBytes)
                return EndiannessHelper.Swap(base.ReadUInt16());
            return base.ReadUInt16();
        }

        public ushort[] ReadUInt16s(int count)
        {
            ushort[] array = new ushort[count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ReadUInt16();
            }

            return array;
        }

        public override decimal ReadDecimal()
        {
            if (SwapBytes)
                return EndiannessHelper.Swap(base.ReadDecimal());
            return base.ReadDecimal();
        }

        public decimal[] ReadDecimals(int count)
        {
            decimal[] array = new decimal[count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ReadDecimal();
            }

            return array;
        }

        public override double ReadDouble()
        {
            if (SwapBytes)
                return EndiannessHelper.Swap(base.ReadDouble());
            return base.ReadDouble();
        }

        public double[] ReadDoubles(int count)
        {
            double[] array = new double[count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ReadDouble();
            }

            return array;
        }

        public override int ReadInt32()
        {
            if (SwapBytes)
                return EndiannessHelper.Swap(base.ReadInt32());
            return base.ReadInt32();
        }

        public int[] ReadInt32s(int count)
        {
            int[] array = new int[count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ReadInt32();
            }

            return array;
        }

        public override long ReadInt64()
        {
            if (SwapBytes)
                return EndiannessHelper.Swap(base.ReadInt64());
            return base.ReadInt64();
        }

        public long[] ReadInt64s(int count)
        {
            long[] array = new long[count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ReadInt64();
            }

            return array;
        }

        public override float ReadSingle()
        {
            if (SwapBytes)
                return EndiannessHelper.Swap(base.ReadSingle());
            return base.ReadSingle();
        }

        public float[] ReadSingles(int count)
        {
            float[] array = new float[count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ReadSingle();
            }

            return array;
        }

        public override uint ReadUInt32()
        {
            if (SwapBytes)
                return EndiannessHelper.Swap(base.ReadUInt32());
            return base.ReadUInt32();
        }

        public uint[] ReadUInt32s(int count)
        {
            uint[] array = new uint[count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ReadUInt32();
            }

            return array;
        }

        public override ulong ReadUInt64()
        {
            if (SwapBytes)
                return EndiannessHelper.Swap(base.ReadUInt64());
            return base.ReadUInt64();
        }

        public ulong[] ReadUInt64s(int count)
        {
            ulong[] array = new ulong[count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ReadUInt64();
            }

            return array;
        }

        public string ReadString(StringBinaryFormat format, int fixedLength = -1)
        {
            mStringBuilder.Clear();

            // For some reason StringBuilder is breaking japanese characters so
            // I replaced it with List<byte> and I'm using mEncoding now
            switch (format)
            {
                case StringBinaryFormat.NullTerminated:
                    {
                        byte b;
                        while ((b = ReadByte()) != 0)
                            mStringBuilder.Add(b);
                    }
                    break;

                case StringBinaryFormat.FixedLength:
                    {
                        if (fixedLength == -1)
                            throw new ArgumentException("Invalid fixed length specified");

                        byte b;
                        bool terminated = false;
                        for (int i = 0; i < fixedLength; i++)
                        {
                            b = ReadByte();
                            if ( b == 0 )
                                terminated = true;

                            if ( !terminated )
                                mStringBuilder.Add(b);
                        }
                    }
                    break;

                case StringBinaryFormat.Mbits:
                    {
                        int length = ReadInt32();

                        // Read the string
                        byte b;
                        bool terminated = false;
                        for (int i = 0; i < length; i++)
                        {
                            b = ReadByte();
                            if (b == 0)
                                terminated = true;

                            if (!terminated)
                                mStringBuilder.Add(b);
                        }

                        while (Position % 4 != 0)
                            SeekCurrent(0x01);

                        if (length % 4 == 0)
                            SeekCurrent(0x04);
                    }
                    break;

                default:
                    throw new ArgumentException("Unknown string format", nameof(format));
            }

            if(format == StringBinaryFormat.Mbits)
            {
                try
                {
                    return Encoding.GetEncoding(932).GetString(mStringBuilder.ToArray());
                }
                catch
                {
                    return Encoding.UTF8.GetString(mStringBuilder.ToArray());
                }
            }

            return mEncoding.GetString(mStringBuilder.ToArray());
        }

        public string ReadString( long offset, StringBinaryFormat format, int fixedLength = -1 )
        {
            string str = null;
            ReadAtOffset( offset, () => str = ReadString( format, fixedLength ) );
            return str;
        }

        public string[] ReadStrings(int count, StringBinaryFormat format, int fixedLength = -1)
        {
            string[] value = new string[count];
            for (int i = 0; i < value.Length; i++)
                value[i] = ReadString(format, fixedLength);

            return value;
        }

        public string[] ReadStrings( long offset, int count, StringBinaryFormat format, int fixedLength = -1 )
        {
            string[] str = null;
            ReadAtOffset( offset, () => str = ReadStrings( count, format, fixedLength ) );
            return str;
        }
    }
}
