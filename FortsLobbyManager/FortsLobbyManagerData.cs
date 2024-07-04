using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortsLobbyManager
{
    public static class FortsLobbyManagerData
    {
        static string t1ColourCode = "[HL=C0C0FFFF]";
        static string t2ColourCode = "[HL=FFC0C0FF]";
        static string obsColourCode = "[HL=C0C0C0FF]";

        public static Dictionary<string, Team> colourCodeToTeam = new Dictionary<string, Team>
        {
            { t1ColourCode, Team.Team1 },
            { t2ColourCode, Team.Team2 },
            { obsColourCode, Team.Observer }
        };

        public static List<string> maps = new List<string>
        {
            "1269562662",
            "1546096712",
            "1230779210",
            "1132518580",
            "2791034275",
            "2135662035",
            "1241206902",
            "1130327085",
            "1641363178",
            "1364744940",
        };
        public static int difficultyCount = 5;
    }
    public enum Team
    {
        Observer,
        Team1,
        Team2
    }

}
