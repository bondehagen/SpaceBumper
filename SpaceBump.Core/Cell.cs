using System.Windows;

namespace SpaceBumper
{
    public class Cell
    {
        public Cell(CellType cellType)
        {
            CellType = cellType;
        }

        public Cell(CellType cellType, int col, int row) : this(cellType)
        {
            Col = col;
            Row = row;

            Min = new Vector(col * Map.CellSize, row * Map.CellSize);
            Max = new Vector(col * Map.CellSize + Map.CellSize, row * Map.CellSize + Map.CellSize);
            const double r = Map.CellSize / 2;
            Position = new Vector(Min.X + r, Min.Y + r);
        }

        public int Col { get; private set; }
        public int Row { get; private set; }
        public CellType CellType { get; set; }
        public Vector Max { get; private set; }
        public Vector Min { get; private set; }
        public Vector Position { get; private set; }
    }
}