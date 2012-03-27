using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace SpaceBumper
{
    public class Bumpership
    {
        private readonly double factor1;
        private readonly double factor2;
        private readonly IInputHandler input;
        private readonly Vector spawnPosition;
        private readonly World world;
        private Bumpership lastcollider;
        private Vector oldPosition;
        private Vector acceleration;


        public Bumpership(IInputHandler input, World world, object shape, string name, Vector spawnPosition)
        {
            this.input = input;
            this.world = world;
            Shape = shape;
            Name = name;
            this.spawnPosition = spawnPosition;

            Radius = 0.5;
            factor1 = 0.05;
            factor2 = 0.1;

            Spawn();
        }

        public object Shape { get; private set; }
        public double Radius { get; private set; }
        public Vector Velocity { get; set; }
        public Vector Position { get; private set; }
        public int Score { get; internal set; }
        public string Name { get; set; }

        public double Angle
        {
            get { return Velocity.GetAngle() * (180 / Math.PI); }
        }


        /// <summary>
        /// Moves this instanse.
        /// </summary>
        /// <param name="acceleration">The acceleration.</param>
        public void Move(Vector acceleration)
        {
            // The strength is calculated as sqrt(x*x+y*y)
            double strength = acceleration.Length;

            if (strength == 0)
                return;

            if (strength > 1.0)
                acceleration.Normalize();

            this.acceleration = acceleration;
        }

        public void Update(int iterations)
        {
            input.Update(iterations, world, this);

            if (!acceleration.IsValid())
                acceleration = new Vector(0, 0);

            Velocity += acceleration * factor1;

            Velocity *= 0.97;

            oldPosition = Position.Clone();
            Position += Velocity * factor2;

            Cell currentCell = GetCurrentCell();

            if (currentCell.CellType == CellType.Attractor && Collide(currentCell))
            {
                Score += 50;
                currentCell.CellType = CellType.Normal;
            }

            foreach (Bumpership b in world.Bumperships.Where(b => b != this && Collide(b)))
            {
                Position = oldPosition;

                Vector oldVelocityA = Velocity.Clone();
                Vector oldVelocityB = b.Velocity.Clone();

                Velocity = Reflect(Velocity, Normalize(Position - b.Position));
                b.Velocity = Reflect(b.Velocity, Normalize(b.Position - Position));

                lastcollider = b;
                b.lastcollider = this;

                if (Velocity.Length > b.Velocity.Length)
                {
                    b.Velocity += oldVelocityA * 0.8;
                    Velocity *= 0.2;
                    Score += 5;
                    b.Score -= 5;
                }
                else
                {
                    b.Velocity += oldVelocityB * 0.8;
                    Velocity *= 0.2;
                    b.Score += 5;
                    Score -= 5;
                }
            }

            if (currentCell.CellType == CellType.None)
            {
                if (lastcollider != null)
                {
                    lastcollider.Score += 50;
                    lastcollider = null;
                }
                if (IsSpawnable())
                {
                    Score -= 50;
                    Spawn();
                }
            }

            foreach (Cell cell in GetTouchingCells().Where(Collide))
            {
                if (cell.CellType == CellType.Blocked && Collide(cell.Position, 0.5))
                {
                    Position = oldPosition;
                    Velocity = Reflect(Velocity, Normalize(Position - cell.Position));
                }
                else if (cell.CellType == CellType.Boost)
                    Velocity *= 1.1;
                else if (cell.CellType == CellType.SlowDown)
                    Velocity *= 0.9;
            }
        }


        /// <summary>
        ///   Determines whether this instance is spawnable.
        ///   A position is spawnable if no other bumpercars have their position wihtin
        ///   a distance of 1.0 units from the position.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is spawnable; otherwise, <c>false</c>.
        /// </returns>
        private bool IsSpawnable()
        {
            return world.Bumperships.All(b => (b.Position - spawnPosition).Length > 1);
        }

        private static Vector Normalize(Vector vector)
        {
            vector.Normalize();
            return vector;
        }

        private static Vector Reflect(Vector velocity, Vector normal)
        {
            return velocity - (2.0 * normal * velocity.Dot(normal));
        }

        private void Spawn()
        {
            Velocity = new Vector(0, 0);
            Position = spawnPosition;
        }

        public IEnumerable<Cell> GetTouchingCells()
        {
            return world.Map.Grid.GetTouchingCells(Position, Radius);
        }

        /// <summary>
        ///   Check for collision with the specified Bumpership
        ///   A bumpercar collides with another bumpership if the distance between the
        ///   centers of the two bumperships is lower than the sum of the radiuses of the bumperships.
        /// </summary>
        /// <param name = "b">The Bumpership.</param>
        /// <returns></returns>
        private bool Collide(Bumpership b)
        {
            return Collide(b.Position, b.Radius);
        }

        /// <summary>
        ///   Check for collision with the specified object position and radius.
        ///   A bumpercar collides with another bumpercar if the distance between the
        ///   centers of the two bumpercars is lower than the sum of the radiuses of the bumpercars.
        /// </summary>
        /// <param name = "objectPosition">The object position.</param>
        /// <param name = "objectRadius">The object radius.</param>
        /// <returns></returns>
        private bool Collide(Vector objectPosition, double objectRadius)
        {
            return (objectPosition - Position).Length < objectRadius + Radius;
        }


        /// <summary>
        ///   Check for collision with the specified cell.
        ///   A bumpership collides with a cell if the circle defined by
        ///   the bumperships position and radius overlaps the interior of the cell.
        /// </summary>
        /// <param name = "cell">The cell.</param>
        /// <returns></returns>
        private bool Collide(Cell cell)
        {
            Vector closestPoint = new Vector(Position.X, Position.Y);

            if (Position.X < cell.Min.X)
                closestPoint.X = cell.Min.X;
            else if (Position.X > cell.Max.X)
                closestPoint.X = cell.Max.X;

            if (Position.Y < cell.Min.Y)
                closestPoint.Y = cell.Min.Y;
            else if (Position.Y > cell.Max.Y)
                closestPoint.Y = cell.Max.Y;

            Vector diff = closestPoint - Position;
            return diff.X * diff.X + diff.Y * diff.Y <= Radius * Radius;
        }

        public Cell GetCurrentCell()
        {
            return world.Map.Grid.GetCell(Position);
        }


        public Vector GetDeltaPosition(double delta)
        {
            return Position + Velocity * factor2 * delta;
        }


        public void Start()
        {
            input.Start(world);
        }
    }
}