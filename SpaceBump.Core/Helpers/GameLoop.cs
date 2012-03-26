using System;

namespace SpaceBumper
{
    public class GameLoop : IDisposable
    {
        #region Delegates
        public delegate bool RenderHandler(float interpolation);

        public delegate bool UpdateHandler();
        #endregion

        private readonly int maxFrameskip;
        private readonly int ticksPerSecond;
        private bool isDisposed;

        public GameLoop() : this(5, 35) {}

        public GameLoop(int maxFrameskip, int ticksPerSecond)
        {
            this.maxFrameskip = maxFrameskip;
            this.ticksPerSecond = ticksPerSecond;
        }

        #region IDisposable Members
        public void Dispose()
        {
            isDisposed = true;
        }
        #endregion

        public void Start()
        {
            if (Update == null || Render == null)
                return;

            int skipTicks = 1000 / ticksPerSecond;

            int nextGameTick = Environment.TickCount;
            bool gameIsRunning = true;

            while (gameIsRunning && !isDisposed)
            {
                int loops = 0;
                while (Environment.TickCount > nextGameTick && loops < maxFrameskip)
                {
                    gameIsRunning = Update();

                    nextGameTick += skipTicks;
                    loops++;
                }

                float interpolation = (Environment.TickCount + skipTicks - nextGameTick) / (float) skipTicks;
                gameIsRunning = gameIsRunning && Render(interpolation);

                //Thread.Sleep(1);
            }
        }

        public event UpdateHandler Update;
        public event RenderHandler Render;
    }
}