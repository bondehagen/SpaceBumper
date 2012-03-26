using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SpaceBumper.Input;

namespace SpaceBumper
{
    internal class Game : IDisposable
    {
        private readonly GameSettings settings;
        private readonly GameManager gameManager;
        private readonly FormGraphics graphics;
        private readonly SocketManager socketManager;

        public Game()
        {
            settings = GetSettings();

            // Connect players
            if (settings.Players.Count(p => p == PlayerType.AI) > 0)
            {
                SocketForm socketForm = new SocketForm(settings);
                socketForm.Show();
                socketManager = socketForm.CreateSocketManager();
                socketForm.Close();
                if (socketManager == null)
                    return;
            }

            // Setup graphics
            graphics = new FormGraphics();
            graphics.Show();
            graphics.Closed += (sender, args) => Dispose();

            // Get keyboard control
            KeyboardHandler keyboardHandler = new KeyboardHandler(graphics);
            gameManager = new GameManager(settings, keyboardHandler, socketManager, graphics);

            // Start game
            gameManager.Start();
        }

        private static GameSettings GetSettings()
        {
            GameSettings gameSettings = new GameSettings();
            string[] lines = new string[] { };
            if (File.Exists("settings.txt"))
                lines = File.ReadAllLines("settings.txt");

            foreach (string line in lines.Where(l => !l.StartsWith("#")))
            {
                string[] words = line.Split(' ');
                string key = words.FirstOrDefault().ToLower();
                switch (key)
                {
                    case "port":
                        int port;
                        if (int.TryParse(words[1], out port))
                            gameSettings.Port = port;
                        break;
                    case "map":
                        string map = line.Substring(key.Length+1).Replace('"', ' ').Trim();
                        if (File.Exists(map))
                            gameSettings.Map = map;
                        break;
                    case "human":
                        gameSettings.Players.Add(PlayerType.Human);
                        break;
                    case "ai":
                        gameSettings.Players.Add(PlayerType.AI);
                        break;
                    case "test":
                        gameSettings.Players.Add(PlayerType.Test);
                        break;
                }
            }
            if(gameSettings.Port == 0 && !string.IsNullOrEmpty(gameSettings.Map))
                Application.Exit();

            return gameSettings;
        }

        public void Dispose()
        {
            if (socketManager != null)
                socketManager.Dispose();
            if (gameManager != null)
                gameManager.Dispose();
            if (graphics != null)
                graphics.Dispose();
        }
    

        /// <summary>
        ///   The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new Game();
            Application.Exit();
        }
    }
}