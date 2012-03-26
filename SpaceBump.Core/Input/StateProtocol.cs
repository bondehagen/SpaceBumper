using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;

namespace SpaceBumper
{
    public static class StateProtocol
    {
        public static string Create(Bumpership player, World world, int iteration)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(BeginStateLine(iteration));
            builder.AppendLine(YouIndexLine(world, player));
            
            foreach (Bumpership bumpership in world.Bumperships)
                builder.AppendLine(BumpershipLine(bumpership));

            foreach (Cell cell in world.Map.Grid.Cast<Cell>().Where(c => c.CellType == CellType.Attractor))
                builder.AppendLine("STAR " + ConvertToString(cell.Position));

            builder.AppendLine(EndStateLine());
            return builder.ToString();
        }

        private static string BeginStateLine(int ticks)
        {
            return "BEGIN_STATE {0}".Format(ticks);
        }

        private static string YouIndexLine(World world, Bumpership bumpership)
        {
            return "YOU {0}".Format(world.Bumperships.IndexOf(bumpership));
        }

        private static string EndStateLine()
        {
            return "END_STATE";
        }

        private static string BumpershipLine(Bumpership bumpership)
        {
            string score = ToInvariantString(bumpership.Score);
            return string.Join(" ",
                               "BUMPERSHIP",
                               ConvertToString(bumpership.Position),
                               ConvertToString(bumpership.Velocity),
                               score);
        }

        private static string ConvertToString(Vector vector)
        {
            return string.Join(" ", ToInvariantString(vector.X), ToInvariantString(vector.Y));
        }

        private static string ToInvariantString(double d)
        {
            return ((float) d).ToString(NumberFormatInfo.InvariantInfo);
        }
    }
}