using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguLibrary.Scene;
using MiguLibrary.Motions;
using MiguLibrary.Objects;

namespace MiguModelViewer.Renderer
{
    public class GLSceneData
    {
        public SceneObject[] Objects;

        public GLSceneData(B3DScene sceneData)
        {
            Objects = new SceneObject[sceneData.Entries.Length];

            int i = 0;
            foreach(ObjectEntry entry in sceneData.Entries)
            {
                Console.WriteLine(entry.FileEntry.Path);
                // Load the object
                Objects[i].Object = new GLObjectData(ObjectData.FromFile($"{Config.DataPath}/{entry.FileEntry.Path}"), new GLShader(File.ReadAllText("Resource/Shader/OBJ_01.vert.shp"), File.ReadAllText("Resource/Shader/OBJ_01.frag.shp")), Path.GetDirectoryName(entry.FileEntry.Path));
                Console.WriteLine("11111");

                // Set the bsx animation cuts
                Objects[i].Cuts = entry.Cuts;

                // Load motions
                Objects[i].Motions = new MotionEntry[entry.Motions.Length];
                for(int j = 0; j < entry.Motions.Length; j++)
                {
                    Objects[i].Motions[j].Motion = Motion.FromFile($"{Config.DataPath}/{entry.Motions[j].Path}");

                    if (entry.Motions[j].FrameSettings.Length > 0)
                        Objects[i].Motions[j].FrameSetting = entry.Motions[j].FrameSettings[0];
                    else
                        Objects[i].Motions[j].FrameSetting = new FrameSetting() { Duration = 65536, PlayCount = 1 };
                }

                i++;
            }
        }
    }

    public struct SceneObject
    {
        public GLObjectData Object;
        public MotionEntry[] Motions;
        public SceneAnimationCut[] Cuts;
    }

    public struct MotionEntry
    {
        public Motion Motion;
        public FrameSetting FrameSetting;
    }
}
