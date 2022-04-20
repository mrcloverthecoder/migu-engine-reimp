using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiguModelViewer.Structs
{
    public class SpecialStructReader
    {
        public static LightSetting[] ReadLightSetting(string path)
        {
            string[] lines = File.ReadAllLines(path);

            int length = 0;
            // First pass to get the length
            foreach (string line in lines)
                if (line != String.Empty)
                    length++;

            // Second pass to get the data
            LightSetting[] settings = new LightSetting[length];

            int idx = 0;
            for(int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == String.Empty)
                    continue;

                string[] values = lines[i].Split('\t');

                LightSetting set = new LightSetting();
                set.Id = idx;
                set.Type = (LightType)int.Parse(values[0]);
                set.Color = Utils.StringVec3Ex(values[1]);
                set.Intensity = Utils.ParseFloatEx(values[2]);

                settings[idx] = set;
                idx++;
            }

            return settings;
        }

        public static FileListInfo[] ReadFileList(string path)
        {
            string[] lines = File.ReadAllLines(path);

            List<FileListInfo> fileList = new List<FileListInfo>(256);
            foreach (string line in lines)
            {
                if (line.StartsWith("#") || String.IsNullOrEmpty(line))
                    continue;

                string[] splitLine = line.Split('\t');

                FileListInfo info = new FileListInfo();
                info.Name = splitLine[0];

                string[] slashSplit = splitLine[1].Split('/');
                string filePath = "";
                int i = 1;
                foreach (string part in slashSplit)
                {
                    if (part == "<DATA>")
                        filePath += Config.DataPath;
                    else if (part == "<OBJ>")
                        filePath += $"{Config.DataPath}/OBJECTS";
                    else
                        filePath += part;

                    if (i < slashSplit.Length)
                        filePath += "/";
                    i++;
                }

                info.Path = filePath;
                Console.WriteLine($"{info.Name} {info.Path}");

                fileList.Add(info);
            }

            return fileList.ToArray();
        }
    }
}
