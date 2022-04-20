using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MiguModelViewer
{
    public struct CostumeTable
    {
        public string Name;
        public int Id;
        public string Info;
        public string BasePath;
        public string ObjectFilename;
        // Subcostumes (like TexVars)
        public SubCostumeTable[] SubCostumes;
    }

    public struct SubCostumeTable
    {
        public string Name;
        public int Id;
        public int ShopPrice;
        // For texture override
        public string BasePath;
        public string ThumbnailName;
    }

    public class TableLoader
    {
        public static CostumeTable[] LoadCosTable(string path)
        {
            string[] lines = File.ReadAllLines(path);

            CostumeTable[] costumes = new CostumeTable[100];
            int idx = 0;
            int subIdx = 0;

            for (int i = 0; i < 100; i++)
                costumes[i] = new CostumeTable() { Info = "__SKIP_NULL" }; // hacky trick to skip null elements when returning the array

            foreach(string line in lines)
            {
                if(line.StartsWith("#") || String.IsNullOrEmpty(line))
                    continue;

                string[] values = line.Split(',');

                int id = int.Parse(values[0]);
                int subId = int.Parse(values[1]);
                int subCount = int.Parse(values[2]);

                if (subCount == 0 || (subCount != 0 && subId == -1))
                {
                    // Reset index for subcostumes
                    subIdx = 0;

                    // Proccess table entries
                    CostumeTable tbl = new CostumeTable();
                    tbl.Name = values[3];
                    tbl.SubCostumes = new SubCostumeTable[subCount == 0 ? 1 : subCount];
                    tbl.Id = id;
                    tbl.Info = values[4];
                    tbl.BasePath = values[5];
                    tbl.ObjectFilename = values[6];

                    SubCostumeTable subTbl = new SubCostumeTable();
                    subTbl.Name = values[3];
                    subTbl.BasePath = values[5];
                    subTbl.Id = 0;
                    subTbl.ThumbnailName = values[7];
                    subTbl.ShopPrice = int.Parse(values[8]);

                    tbl.SubCostumes[0] = subTbl;

                    costumes[idx] = tbl;
                    idx++;
                }
                else if(subId != -1) // It's a subcostume
                {
                    SubCostumeTable subTbl = new SubCostumeTable();
                    subTbl.Id = subId;
                    subTbl.Name = values[3];
                    subTbl.BasePath = values[5];
                    subTbl.ThumbnailName = values[7];
                    subTbl.ShopPrice = int.Parse(values[8]);

                    // (idx - 1) because when it last proccessed the table it added 1 to idx,
                    // but since subcostumes modify the last main table and not create a new one
                    // you need to subtract that 1 again to access the last table
                    costumes[idx - 1].SubCostumes[subIdx] = subTbl;
                    subIdx++;
                }
            }

            return costumes.Where(p => p.Info != "__SKIP_NULL").ToArray();
        }
    }
}
