using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FortsLobbyManager
{
    public class FortsLogFileWatcher
    {
        private long lastFileLength = 0;
        private readonly string logFilePath;
        private FileSystemWatcher watcher;

        public delegate void LinesAddedHandler(List<string> newLines);
        public event LinesAddedHandler LinesAdded;

        public FortsLogFileWatcher(string path)
        {
            logFilePath = path;
            watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(logFilePath),
                Filter = Path.GetFileName(logFilePath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            };

            watcher.Changed += async (sender, e) => await OnChanged();
            watcher.EnableRaisingEvents = true;
            lastFileLength = new FileInfo(logFilePath).Length;
        }

        private async Task OnChanged()
        {
            List<string> newLines = await ReadNewLinesAsync(logFilePath);
            if (newLines.Count > 0)
            {
                LinesAdded?.Invoke(newLines);
            }
        }

        private async Task<List<string>> ReadNewLinesAsync(string filePath)
        {
            List<string> newLines = new List<string>();

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (fs.Length > lastFileLength)
                {
                    fs.Seek(lastFileLength, SeekOrigin.Begin);
                    lastFileLength = fs.Length;
                    using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            newLines.Add(line);
                        }
                    }

                    
                }
            }

            return newLines;
        }
    }
}
