using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MiguLibrary.IO;

namespace MiguLibrary.Scene
{
    public class SceneAnimationCut
    {
        // unlike bmm (which is 30 fps), this is 60 fps
        public int StartFrame;
        public int Length;
        public TranslationPoint[] TranslationPoints;
        public RotationPoint[] RotationPoints;

        public SceneAnimationCut()
        {
            StartFrame = 0;
            Length = 0;
            TranslationPoints = new TranslationPoint[0];
            RotationPoints = new RotationPoint[0];
        }

        public static SceneAnimationCut Read(EndianBinaryReader reader)
        {
            SceneAnimationCut cut = new SceneAnimationCut();

            cut.StartFrame = reader.ReadInt32();
            cut.Length = reader.ReadInt32();

            // Unknown data
            reader.SeekCurrent(8 + (4 * 16));

            int transPointCount = reader.ReadInt32();
            cut.TranslationPoints = new TranslationPoint[transPointCount];

            for(int i = 0; i < transPointCount; i++)
            {
                TranslationPoint point = new TranslationPoint();

                // Flags; Not sure what they serve for right now
                reader.SeekCurrent(4);

                point.StartPosition = reader.ReadVector3(true);
                point.MidwayPosition = reader.ReadVector3(true);
                point.EndPosition = reader.ReadVector3(true);
                point.FadeIn = reader.ReadSingle();
                point.FadeOut = reader.ReadSingle();

                cut.TranslationPoints[i] = point;
            }

            Console.WriteLine("AA");
            int unkPointCount = reader.ReadInt32();
            reader.SeekCurrent(20 * unkPointCount);

            Console.WriteLine("BB");
            int rotationPointCount = reader.ReadInt32();
            cut.RotationPoints = new RotationPoint[rotationPointCount];

            Console.WriteLine($"CC {reader.Position}");
            for (int i = 0; i < rotationPointCount; i++)
            {
                RotationPoint point = new RotationPoint();

                // Flags; Not sure what they serve for right now
                reader.SeekCurrent(4);

                // Unknown values
                reader.SeekCurrent(16);

                point.Rotation = reader.ReadQuaternion();
                Console.WriteLine(point.Rotation);

                cut.RotationPoints[i] = point;
            }

            Console.WriteLine("DD");
            int unk1PointCount = reader.ReadInt32();
            reader.SeekCurrent(20 * unk1PointCount);

            Console.WriteLine("EE");
            int unk2PointCount = reader.ReadInt32();
            reader.SeekCurrent(20 * unk2PointCount);

            Console.WriteLine($"BSX POS 1: {reader.Position}");

            return cut;
        }
    }

    public struct TranslationPoint
    {
        public Vector3 StartPosition;
        public Vector3 MidwayPosition;
        public Vector3 EndPosition;
        public float FadeIn;
        public float FadeOut;
    }

    public struct RotationPoint
    {
        public Quaternion Rotation;
    }
}
