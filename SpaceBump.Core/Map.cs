using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace SpaceBumper
{
    public class Map
    {
        public const double CellSize = 1.0;

        public Map(string mapName)
        {
            LoadMap(mapName);
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Cell[,] Grid { get; private set; }
        public IList<Vector> StartPositions { get; private set; }

        private void LoadMap(string mapName)
        {
            string[] lines = File.ReadAllLines(mapName);
            Width = lines.Max(line => line.Length);
            Height = lines.Length;
            Grid = new Cell[Width, Height];
            StartPositions = new List<Vector>();

            for (int row = 0; row < Height; row++)
            {
                string line = lines[row];
                for (int col = 0; col < Width; col++)
                {
                    CellType cellType;
                    if (line.Length > col)
                    {
                        char c = line[col];
                        cellType = GetTile(c);
                        int playerNumber;
                        if (int.TryParse(c.ToString(), out playerNumber))
                        {
                            cellType = CellType.Normal;
                            double r = CellSize / 2;
                            StartPositions.Add(new Vector(col * CellSize + r, row * CellSize + r));
                        }
                    }
                    else
                        cellType = CellType.None;

                    Cell cell = new Cell(cellType, col, row);
                    Grid[col, row] = cell;
                }
            }
        }

        private static CellType GetTile(char c)
        {
            CellType cellType;
            if (new Dictionary<char, CellType>
                {
                    { ' ', CellType.None },
                    { '#', CellType.Blocked },
                    { 'b', CellType.Boost },
                    { 's', CellType.SlowDown },
                    { '.', CellType.Normal },
                    { 'o', CellType.Attractor }
                }.TryGetValue(c, out cellType))
                return cellType;
            return CellType.None;
        }
    }
}