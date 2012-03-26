using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SpaceBumper
{
    public class GameManager : IDisposable
    {
        private readonly GameSettings gameSettings;
        private readonly IGraphicsHandler graphics;
        private readonly GameLoop gameLoop;
        private readonly World world;
        private static int iterations;

        public GameManager(
            GameSettings gameSettings,
            IInputHandler keyboardHandler,
            SocketManager socketManager,
            IGraphicsHandler graphics)
        {
            this.gameSettings = gameSettings;
            this.graphics = graphics;
            const int ticksPerSecond = 30;
            gameLoop = new GameLoop(5, ticksPerSecond);
            gameLoop.Update += Update;
            gameLoop.Render += Render;
 
            Map map = new Map(gameSettings.Map);
            world = new World(map);

            int countShip = 0;
            List<Bumpership> bumperships = gameSettings.Players.Take(world.Map.StartPositions.Count)
                .Select(p => CreateBumpership(p, keyboardHandler, socketManager, ref countShip))
                .ToList();
            world.AddShips(bumperships);
        }

        private Bumpership CreateBumpership(PlayerType player, IInputHandler keyboardHandler, SocketManager socketManager, ref int countShip)
        {
            IInputHandler input;
            string name = "";
            switch (player)
            {
                case PlayerType.Human:
                    input = keyboardHandler;
                    name = "Human";
                    break;
                case PlayerType.AI:
                    input = socketManager.GetNetworkPlayer();
                    break;
                default:
                    input = new IdiotAi(); // Test
                    break;
            }
            Vector startPosition = world.Map.StartPositions[countShip];
            countShip++;
            return new Bumpership(input, world, graphics.CreateShip(), name, startPosition);
        }

        public void Dispose()
        {
            gameLoop.Dispose();
        }

        public void Start()
        {
            foreach (Bumpership bumpership in world.Bumperships)
                bumpership.Start();
            
            gameLoop.Start();
        }

        public bool Update()
        {
            iterations++;
            return world.Update(iterations);
        }

        protected bool Render(float deltaTime)
        {
            graphics.BeforeRender(deltaTime);

            graphics.RenderMap(world.Map);

            foreach (Bumpership bumpership in world.Bumperships)
                graphics.RenderShip(bumpership);

            graphics.AfterRender();
            return true;
        }
    }
}