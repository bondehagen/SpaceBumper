using System;
using System.Threading;
using System.Windows;

namespace SpaceBumper
{
    public class IdiotAi : IInputHandler
    {
        private Random random;

        public void Start(World world)
        {
            Thread.Sleep(20);
            random = new Random();
        }

        public void Update(int iteration, World world, Bumpership bumpership)
        {
            Vector accelerate = new Vector((float)random.NextDouble() * 6 - 3, (float)random.NextDouble() * 6 - 3);
            accelerate.Normalize();
            if (random.Next(0,12) > 1)
            {
                accelerate = bumpership.Velocity;
            }
            bumpership.Move(accelerate);
        }

        public void Dispose()
        {
        }
    }
}