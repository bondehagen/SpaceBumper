using System.Collections.Generic;

namespace SpaceBumper
{
    public class GameSettings
    {
        public GameSettings()
        {
            Players = new List<PlayerType>();
            Map = "";
            Port = 2012;
        }

        public IList<PlayerType> Players { get; set; }

        public string Map { get; set; }

        public int Port { get; set; }
    }

    public enum PlayerType
    {
        Human,
        AI,
        Test
    }
}