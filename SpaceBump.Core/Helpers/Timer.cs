using System.Diagnostics;

namespace SpaceBumper
{
    public class Timer
    {
        private readonly decimal pollFrequency;
        private readonly Stopwatch stopWatch;
        private long countsPerSecond;
        private int frameCount;

        public Timer() : this(1000m) {}

        public Timer(decimal pollFrequency)
        {
            this.stopWatch = new Stopwatch();
            this.pollFrequency = Stopwatch.IsHighResolution ? pollFrequency : 1000m;
            FPS = 0;
        }

        public double FPS { get; private set; }

        public void Start()
        {
            frameCount = 0;
            stopWatch.Restart();
            countsPerSecond = Stopwatch.Frequency;
        }

        public void Update()
        {
            frameCount++;
            decimal time = GetElapsedTimeInMilliseconds();
            if (stopWatch.IsRunning && time >= pollFrequency)
            {
                FPS = (double) (frameCount / time * 1000m);
                Start();
            }
        }


        private decimal GetElapsedTimeInMilliseconds()
        {
            return (stopWatch.ElapsedTicks * 1000m) / countsPerSecond;
        }


        public void Stop()
        {
            stopWatch.Stop();
        }
    }
}