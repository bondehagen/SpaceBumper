using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace SpaceBumper.Input
{
    internal class KeyboardHandler : IInputHandler
    {
        private readonly ISet<Keys> keyQueue;


        public KeyboardHandler(Form form)
        {
            keyQueue = new HashSet<Keys>();
            form.KeyDown += OnKeyDown;
            form.KeyPreview = true;
            form.KeyUp += OnKeyUp;
        }

        public void Start(World world) {}

        public void Update(int iteration, World world, Bumpership bumpership)
        {
            foreach (Keys key in keyQueue.Distinct())
            {
                const double increaseSpeedBy = 0.05;
                const int rotateAngleBy = 5;

                double currentSpeed = bumpership.Velocity.Length; // Get current speed
                double currentAngle = bumpership.Angle; // Get current angle in degrees

                switch (key)
                {
                    case Keys.Up:
                    case Keys.W:
                        currentSpeed += increaseSpeedBy;
                        break;
                    case Keys.Down:
                    case Keys.S:
                        currentSpeed -= increaseSpeedBy;
                        break;
                    case Keys.Left:
                    case Keys.A:
                        currentAngle -= rotateAngleBy;
                        break;
                    case Keys.Right:
                    case Keys.D:
                        currentAngle += rotateAngleBy;
                        break;
                }
                double radians = currentAngle * Math.PI / 180;
                Vector rotation = new Vector(Math.Cos(radians), Math.Sin(radians));
                bumpership.Velocity = rotation * currentSpeed;
            }
        }

        public void Dispose() {}

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            keyQueue.Add(e.KeyCode);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            keyQueue.Remove(e.KeyCode);
        }
    }
}