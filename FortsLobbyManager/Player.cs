using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Windows;
namespace FortsLobbyManager
{
    internal class Player
    {
        public ulong SteamID { get; set; }
        public string Name { get; set; }
        public Team Team;

        public Player(ulong steamID, string name, Team team)
        {
            SteamID = steamID;
            Name = name;
            Team = team;

            // Check if the folder playerData/SteamID exists
            // If it doesn't, create it
            // If it does, load the player's settings from the file
            string playerDataFolder = "playerData/" + steamID;
            if (!Directory.Exists(playerDataFolder))
            {
                Directory.CreateDirectory(playerDataFolder);
            }
            //GenerateFileInDirectory(playerDataFolder, "settings.json");
            GenerateFileInDirectory(playerDataFolder, "stats.json");
            //GenerateFileInDirectory(playerDataFolder, "achievements.json");
            //GenerateFileInDirectory(playerDataFolder, "inventory.json");
            //GenerateFileInDirectory(playerDataFolder, "friends.json");
            GenerateFileInDirectory(playerDataFolder, "missionData.json");


            Dictionary<ulong, MissionData> missionData = new Dictionary<ulong, MissionData>();
            for (int i = 0; i < FortsLobbyManagerData.maps.Count; i++)
            {
                ulong map = ulong.Parse(FortsLobbyManagerData.maps[i]);
                MissionData data = new MissionData
                {
                    hasPassed = false,
                    difficultyData = new List<CompletionData>()
                };
                for (int j = 0; j < FortsLobbyManagerData.difficultyCount; j++)
                {
                    data.difficultyData.Add(new CompletionData
                    {
                        timesCompleted = 0,
                        timesFailed = 0,
                        highScore = 0
                    });
                }
                missionData.Add(map, data);
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(missionData, options);

            File.WriteAllText(playerDataFolder + "/missionData.json", jsonString);
        }




        private void GenerateFileInDirectory(string directory, string fileName)
        {
            if (!File.Exists(directory + "/" + fileName))
            {
                File.Create(directory + "/" + fileName).Close();
            }
        }

    }

    public class MissionData
    {
        public bool hasPassed { get; set; }
        public List<CompletionData> difficultyData { get; set; }
    }

    public class CompletionData
    {
        public int timesCompleted { get; set; }
        public int timesFailed { get; set; }
        public double highScore { get; set; }

    }

    public class Stats
    {
        public int timesConnected { get; set; }
        public int timesKicked { get; set; }
        public int timesLeftLobby { get; set; }
        public int timesLeftGame { get; set; }
        public double timePlayed { get; set; }
        public double timeInLobby { get; set; }

        public TimesUsedCommands timesUsedCommands { get; set; }
    }
    public class TimesUsedCommands
    {
        public int command1 { get; set; }
        public int command2 { get; set; }
        public int command3 { get; set; }
    }
}
