using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SpaceBumper
{
    public static class GridExtensions
    {
        public static Cell GetCell(this Cell[,] grid, int col, int row)
        {
            int gridWidth = grid.GetUpperBound(0);
            int gridHeight = grid.GetUpperBound(1);
            return (col > gridWidth || col < 0 || row > gridHeight || row < 0)
                       ? new Cell(CellType.None)
                       : grid[col, row];
        }


        public static Cell GetCell(this Cell[,] grid, Vector position)
        {
            return grid.GetCell(position.X, position.Y);
        }


        public static Cell GetCell(this Cell[,] grid, double positionX, double positionY)
        {
            int gridColumn = (int) Math.Floor(positionX / Map.CellSize);
            int gridRow = (int) Math.Floor(positionY / Map.CellSize);
            return grid.GetCell(gridColumn, gridRow);
        }


        public static IEnumerable<Cell> GetTouchingCells(this Cell[,] grid, Vector position, double radius)
        {
            return new[]
                {
                    grid.GetCell(position.X + radius, position.Y - radius),
                    grid.GetCell(position.X - radius, position.Y + radius),
                    grid.GetCell(position.X + radius, position.Y + radius),
                    grid.GetCell(position.X - radius, position.Y - radius),
                    grid.GetCell(position.X + radius, position.Y),
                    grid.GetCell(position.X - radius, position.Y),
                    grid.GetCell(position.X, position.Y + radius),
                    grid.GetCell(position.X, position.Y - radius)
                }.Distinct();
        }
    }
}