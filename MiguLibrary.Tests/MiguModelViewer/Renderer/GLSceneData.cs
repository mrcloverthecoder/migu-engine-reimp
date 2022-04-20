using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguEngine;
using MiguEngine.Objects;
using MiguLibrary.Scene;
using MiguLibrary.Motions;
using MiguLibrary.Objects;

namespace MiguModelViewer.Renderer
{
    public class GLSceneData
    {
        public SceneObject[] Objects;
        public SceneAnimationCut[] Camera;

        public GLSceneData(B3DScene sceneData)
        {
            Objects = new SceneObject[sceneData.Entries.Length];

            int i = 0;
            foreach(ObjectEntry entry in sceneData.Entries)
            {
                // Set the object name
                Objects[i].Name = entry.FileEntry.Path;

                // Load the object
                Shader shader = new Shader(File.ReadAllText("Resource/Shader/OBJ_01.vert.shp"), File.ReadAllText("Resource/Shader/OBJ_01.frag.shp"));

                // If its a chara object
                if (entry.FileEntry.Filename.ToLower().EndsWith("mig.bmd"))
                {
                    Objects[i].Object = new GLObjectData(ObjectData.Load($"{Config.DataPath}/OBJECTS/OBJ_MIGU_A{State.SelectedGumi.ToString("00")}/OBJ_MIG.BMD"), shader);
                }
                else
                    Objects[i].Object = new GLObjectData(ObjectData.Load($"{Config.DataPath}/{entry.FileEntry.Path}"), shader);

                // Set the bsx animation cuts
                Objects[i].Cuts = entry.Cuts;

                // Load motions
                Objects[i].Motions = new MotionEntry[entry.Motions.Length];
                for(int j = 0; j < entry.Motions.Length; j++)
                {
                    Objects[i].Motions[j].CurrentMorphIndices = new int[Objects[i].Object.Morphs.Length];
                    for (int k = 0; k < Objects[i].Motions[j].CurrentMorphIndices.Length; k++)
                        Objects[i].Motions[j].CurrentMorphIndices[k] = 0;

                    if(entry.Motions[j].Type != MotionType.Physics)
                        Objects[i].Motions[j].Motion = Motion.FromFile($"{Config.DataPath}/{entry.Motions[j].Path}");
                    else
                    {
                        Objects[i].Motions[j].Motion = Motion.FromFile($"{Config.DataPath}/MOTIONS/" + entry.Motions[j].Filename.Split('_')[0] + $"_P{Objects[i].Object.PhysicsId}.BMM");
                    }

                    Objects[i].Motions[j].CurrentBoneIndex = 0;

                    Objects[i].Motions[j].FrameSettings = entry.Motions[j].FrameSettings;
                    Objects[i].Motions[j].Duration = entry.Motions[j].Duration;
                    Objects[i].Motions[j].Type = entry.Motions[j].Type;
                }

                i++;
            }

            Camera = sceneData.CameraCuts;

            /*
            for(int j = 0; j < Objects.Length; j++)
            {
                for(int m = 0; m < Objects[j].Motions.Length; m++)
                {
                    if (Objects[j].Motions[m].FrameSettings.Length < 1)
                        continue;

                    if (Objects[j].Motions[m].Motion.Bones.Count < 1)
                        continue;

                    // Shouldn't be done for character motions
                    if (Objects[j].Motions[m].Type != MotionType.Object)
                        continue;

                    FrameSetting set = Objects[j].Motions[m].FrameSettings[0];

                    // Length of each loop
                    float loopLength = (float)set.EndFrame / (float)set.PlayCount;

                    float frameMultiplier = loopLength / ((float)Objects[j].Motions[m].Motion.Bones[0].Keyframes.Last().Frame * 2.0f);

                    Console.WriteLine(Objects[j].Motions[m].Motion.Name);
                    Console.WriteLine(frameMultiplier);
                    Console.WriteLine(set.PlayCount);

                    for (int b = 0; b < Objects[j].Motions[m].Motion.Bones.Count; b++)
                    {
                        for (int k = 0; k < Objects[j].Motions[m].Motion.Bones[b].Keyframes.Length; k++)
                        {
                            Objects[j].Motions[m].Motion.Bones[b].Keyframes[k].Frame = (int)Math.Ceiling(Objects[j].Motions[m].Motion.Bones[b].Keyframes[k].Frame * frameMultiplier);
                        }
                    }
                }
            }
            */
        }
    }

    public struct SceneObject
    {
        public string Name;
        public GLObjectData Object;
        public MotionEntry[] Motions;
        public SceneAnimationCut[] Cuts;
    }

    public struct MotionEntry
    {
        public int[] CurrentMorphIndices;
        // All bones should have the same frame count
        public int CurrentBoneIndex;

        public int CurrentLoopNumber;

        public Motion Motion;
        public int Duration;
        public MotionType Type;
        public FrameSetting[] FrameSettings;
    }
}
