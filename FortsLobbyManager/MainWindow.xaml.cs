using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Text.Json;

namespace FortsLobbyManager
{
    



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string logFilePath = "G:\\SteamLibrary\\steamapps\\common\\Forts\\users\\76561198126608185\\log.txt";


        Dictionary<string, Player> players;

        FortsLogFileWatcher logFileWatcher;


        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            //dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100); // Set the interval
            //dispatcherTimer.Tick += dispatcherTimer_Tick; // Subscribe to the Tick event
            //dispatcherTimer.Start();


            logFileWatcher = new FortsLogFileWatcher(logFilePath);
            logFileWatcher.LinesAdded += OnLinesAdded;
            players = new Dictionary<string, Player>();
            Player player = new Player(76561198126608185, "Linn", Team.Team1);
        }
        private List<string> RemoveNullChars(List<string> input)
        {
            List<string> output = new List<string>();
            foreach (var line in input)
            {
                output.Add(RemoveNullChars(line));
            }
            return output;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
            this.Activate();
        }

        private string RemoveNullChars(string input)
        {
            return input.Replace("\0", "");
        }

        private void OnLinesAdded(List<string> newLines)
        {
            Dispatcher.Invoke(() =>
            {
                TestLines(newLines);
            });
        }

        PlayerGenerateState playerGenerateState = PlayerGenerateState.None;
        string lastSteamID = "";
        string lastUserName = "";
        string lastColourCode = "";





        private void TestLines(List<string> lines)
        {
            lines = RemoveNullChars(lines);

            PlayerGeneratorStatus.Text = playerGenerateState.ToString();
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.Length < 2) continue;


                bool isPlayerMessage = line.Contains("[/HL]: ");
                if (isPlayerMessage)
                {
                    ProcessPlayerMessage(line);
                }
                else
                {
                    ProcessSystemMessage(line);
                }

            }


            TryCreateNewPlayer();
        }


        private void TryCreateNewPlayer()
        {
            if (playerGenerateState == PlayerGenerateState.TeamIDAndUsername)
            {
                Team team = FortsLobbyManagerData.colourCodeToTeam[lastColourCode];
                Player player = new Player(ulong.Parse(lastSteamID), lastUserName, team);
                if (players.ContainsKey(lastUserName))
                {
                    // should kick both players
                    LogFileTextBlock.Text += $"Player {lastUserName} already exists. Kicking second player\n";
                    playerGenerateState = PlayerGenerateState.None;
                }
                else
                {
                    players.Add(lastUserName, player);
                    LogFileTextBlock.Text += $"New player connected. STEAMID: {lastSteamID}, USERNAME: {lastUserName}, TEAM: {team}\n";
                    playerGenerateState = PlayerGenerateState.None;
                }

            }
        }
        private void ProcessPlayerMessage(string line)
        {
            string username = line.Substring(13, line.IndexOf("[/HL]: " ) - 13);
            string message = line.Substring(line.IndexOf("[/HL]: ") + 7);
            LogFileTextBlock.Text += "Message sent by player " + username + ": " + message + "\n";

        }

        private void ProcessSystemMessage(string line)
        {

            ProcessPlayerConnect(line);
            ProcessPlayerDisconnect(line);
            
        }

        private void ProcessPlayerConnect(string line)
        {
            switch (playerGenerateState)
            {
                case PlayerGenerateState.None:
                    if (line.Contains("Message 134") && line.Contains("steamID"))
                    {
                        //LogFileTextBlock.Text += "Getting SteamID\n";
                        int steamIdStart = line.IndexOf("steamID") + 8;
                        int steamIdEnd = steamIdStart + 17;
                        lastSteamID = line.Substring(steamIdStart, 17);

                        playerGenerateState = PlayerGenerateState.SteamID;
                    }
                    else
                    {
                        //LogFileTextBlock.Text += $"Not getting SteamID({line}){line.Contains("Message 134")} {line.Contains("steamID")}\n";
                    }
                    break;
                case PlayerGenerateState.SteamID:
                    int nameIndexEnd = line.IndexOf("[/HL]");
                    if (line.StartsWith("[HL=") && !line.Substring(nameIndexEnd).Contains(":"))
                    {
                        //LogFileTextBlock.Text += "Getting TeamID and Username\n";
                        lastColourCode = line.Substring(0, 13);
                        int nameIndexStart = 13;

                        lastUserName = line.Substring(nameIndexStart, nameIndexEnd - nameIndexStart);
                        playerGenerateState = PlayerGenerateState.TeamIDAndUsername;
                    }
                    else
                    {
                        if (
                                !(
                                line.StartsWith("SendClientStatus(")
                                || line.StartsWith("Multiplayer(Playing")
                                || line.StartsWith("CommandInterpreter testing client status")
                                || line.StartsWith("  Handled in World")
                                )
                            )
                        {
                            playerGenerateState = PlayerGenerateState.None;
                        }

                    }
                    break;
            }
        }
        private void ProcessPlayerDisconnect(string line)
        {
            if (line.Contains("has disconnected, index "))
            {
                int endUsernameIndex = line.IndexOf(" has disconnected");
                int startSteamIDIndex = line.IndexOf("SteamID ") + 8;


                string username = line.Substring(0, endUsernameIndex);
                string steamID = line.Substring(startSteamIDIndex, 17);

                if (!players.ContainsKey(username)) return;
                Player player = players[username];
                LogFileTextBlock.Text += $"Player disconnected. STEAMID: {steamID}, USERNAME: {username}, TEAM: {player.Team}\n";
                players.Remove(username);
            }
            if (line.Contains("Multiplayer(") && line.Contains("): disconnect "))
            {
                int startUsernameIndex = line.IndexOf("): disconnect ") + 14;
                int endUsernameIndex = line.IndexOf(", PlayerChatting -> PlayerChatting, ");
                string username = line.Substring(startUsernameIndex, endUsernameIndex - startUsernameIndex);

                if (!players.ContainsKey(username)) return;
                Player player = players[username];
                LogFileTextBlock.Text += $"Player disconnected. STEAMID: {player.SteamID}, USERNAME: {username}, TEAM: {player.Team}\n";
                players.Remove(username);

            }
        }
    }

    




    enum PlayerGenerateState
    {
        None,
        SteamID,
        TeamIDAndUsername
    }

}