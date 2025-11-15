using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerStats
{
    public ushort SlotID { get; set; }
    public string PlayerIGN { get; set; }
    public byte LeaderboardPos { get; set; }
    public ushort Kills { get; set; }
    public ushort Deaths { get; set; }
    public int Score { get; set; }
    public string NNNNNNNNNNN { get; } = "NNNNNNNNNN"; // 10 N's
    public int EXP { get; set; }
    public int Carats { get; set; }

    public AdditionalStats Stats { get; set; }
    // Add other properties as needed
}

public class AdditionalStats
{
    public int STD { get; set; }
    public int RUN { get; set; }
    public int ATK { get; set; }
    public int DMG { get; set; }
    public int GD { get; set; }
    public int TF { get; set; }
    public int SKL { get; set; }
    public int JMP { get; set; }
    public int EAT { get; set; }
    public int DIE { get; set; }

    public static AdditionalStats Deserialize(string stats)
    {
        var dict = stats.Split(',')
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Chunk(2) // pairs: KEY, VALUE
                        .ToDictionary(
                            x => x[0],
                            x => int.TryParse(x[1], out var val) ? val : 0
                        );

        return new AdditionalStats
        {
            STD = dict.GetValueOrDefault("STD", 0),
            RUN = dict.GetValueOrDefault("RUN", 0),
            ATK = dict.GetValueOrDefault("ATK", 0),
            DMG = dict.GetValueOrDefault("DMG", 0),
            GD = dict.GetValueOrDefault("GD", 0),
            TF = dict.GetValueOrDefault("TF", 0),
            SKL = dict.GetValueOrDefault("SKL", 0),
            JMP = dict.GetValueOrDefault("JMP", 0),
            EAT = dict.GetValueOrDefault("EAT", 0),
            DIE = dict.GetValueOrDefault("DIE", 0)
        };
    }

    public string Serialize()
    {
        // keep order same as input format
        var parts = new List<string>
        {
            $"STD,{STD}",
            $"RUN,{RUN}",
            $"ATK,{ATK}",
            $"DMG,{DMG}",
            $"GD,{GD}",
            $"TF,{TF}",
            $"SKL,{SKL}",
            $"JMP,{JMP}",
            $"EAT,{EAT}",
            $"DIE,{DIE}"
        };
        return string.Join(",", parts);
    }
}
