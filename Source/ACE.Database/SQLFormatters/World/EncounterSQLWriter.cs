using System;
using System.Collections.Generic;
using System.IO;

using ACE.Database.Models.World;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;

namespace ACE.Database.SQLFormatters.World
{
    public class EncounterSQLWriter : SQLWriter
    {
        /// <summary>
        /// Default is formed from: input.Landblock.ToString("X4")
        /// </summary>
        public string GetDefaultFileName(Encounter input)
        {
            string fileName = input.Landblock.ToString("X4");
            fileName = IllegalInFileName.Replace(fileName, "_");
            fileName += ".sql";

            return fileName;
        }

        public void CreateSQLDELETEStatement(IList<Encounter> input, StreamWriter writer)
        {
            writer.WriteLine($"DELETE FROM `encounter` WHERE `landblock` = 0x{input[0].Landblock:X4};");
        }

        public void CreateSQLINSERTStatement(IList<Encounter> input, StreamWriter writer)
        {
            writer.WriteLine("INSERT INTO `encounter` (`landblock`, `weenie_Class_Id`, `cell_X`, `cell_Y`, `last_Modified`)");

            var lineGenerator = new Func<int, string>(i =>
            {
                string label = null;
                var level = 0;

                if (WeenieNames != null)
                    WeenieNames.TryGetValue(input[i].WeenieClassId, out label);

                if (WeenieLevels != null)
                    WeenieLevels.TryGetValue(input[i].WeenieClassId, out level);

                if (level > 0)
                    label += $" - Level: {level}";

                if (TreasureDeath != null)
                {
                    var weenie = DatabaseManager.World.GetCachedWeenie(input[i].WeenieClassId);
                    var deathTreasureType = weenie.GetProperty(PropertyDataId.DeathTreasureType) ?? 0;
                    if (deathTreasureType != 0 && TreasureDeath.TryGetValue(deathTreasureType, out var treasureDeath))
                        label += $" - {GetValueForTreasureData(treasureDeath.TreasureType)}";
                }

                return $"0x{input[i].Landblock:X4}, {input[i].WeenieClassId}, {input[i].CellX}, {input[i].CellY}, '{input[i].LastModified:yyyy-MM-dd HH:mm:ss}') /* {label} */";
            });

            ValuesWriter(input.Count, lineGenerator, writer);
        }
    }
}
