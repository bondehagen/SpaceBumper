using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace SpaceBumper
{
    public sealed class FormGraphics : Form, IGraphicsHandler
    {
        private const int windowWidth = 1280;
        private const int windowHeight = 720;
        private static Bitmap backbuffer;
        private readonly Font font;
        private readonly SolidBrush fontColor;
        private readonly Queue<RectangleF> paintRegion;
        private readonly Dictionary<string, Bitmap> textures;
        private readonly Timer timer;
        private float currentDeltaTime;
        private Win32.NativeMessage msg;
        private int redrawCounter;
        private Graphics renderSurface;
        private int shapeCounter;
        private Map map;
        public static float CellSize = 40;
        private readonly SolidBrush greyBrush;
        private Bitmap mapAsImage;


        public FormGraphics()
        {
            ClientSize = new Size(windowWidth, windowHeight);

            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.UserPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.Opaque,
                     true);

            SuspendLayout();

            paintRegion = new Queue<RectangleF>();

            // Load images
            textures = LoadTextures();

            fontColor = new SolidBrush(Color.DarkBlue);
            font = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Bold);
            greyBrush = new SolidBrush(Color.LightSkyBlue);
            
            shapeCounter = 1;

            // Create the backbuffer
            backbuffer = new Bitmap(windowWidth, windowHeight);

            mapAsImage = null;

            msg = new Win32.NativeMessage();
            timer = new Timer(300);
            timer.Start();
        }

        private static Dictionary<string, Bitmap> LoadTextures()
        {
            return new Dictionary<string, Bitmap>
                {
                    { CellType.Attractor.ToString(), (Bitmap) Image.FromFile("Textures/Attractor.png") },
                    { CellType.Blocked.ToString(), (Bitmap) Image.FromFile("Textures/Blocked.png") },
                    { CellType.Boost.ToString(), (Bitmap) Image.FromFile("Textures/Boost.png") },
                    { CellType.SlowDown.ToString(), (Bitmap) Image.FromFile("Textures/SlowDown.png") },
                    { CellType.None.ToString(), (Bitmap) Image.FromFile("Textures/Tile.png") },
                    { "startCell", (Bitmap) Image.FromFile("Textures/Spawn.png") },
                    { "bg", (Bitmap) Image.FromFile("Textures/nebula.jpg") },
                    { "player1", (Bitmap) Image.FromFile("Textures/car1.png") },
                    { "player2", (Bitmap) Image.FromFile("Textures/car2.png") },
                    { "player3", (Bitmap) Image.FromFile("Textures/car3.png") },
                    { "player4", (Bitmap) Image.FromFile("Textures/car4.png") }
                };
        }

        #region IGraphicsHandler Members
        public object CreateShip()
        {
            IncrementShapeCounter();
            return textures["player" + shapeCounter];
        }

        public void RenderShip(Bumpership bumpership)
        {
            Vector viewPosition = bumpership.GetDeltaPosition(currentDeltaTime);
            Image rotateImage = RotateImage((Bitmap) bumpership.Shape, (float) bumpership.Angle + 90);
            GraphicsUnit graphicsUnit = GraphicsUnit.Pixel;
            RectangleF bounds = rotateImage.GetBounds(ref graphicsUnit);

            float ix = (float)(viewPosition.X - bumpership.Radius) * CellSize - (bounds.Width / 2) + CellSize / 2;
            float iy = (float)(viewPosition.Y - bumpership.Radius) * CellSize - (bounds.Height / 2) + CellSize / 2;

            renderSurface.DrawImage(rotateImage,
                                    ix,
                                    iy,
                                    bounds,
                                    graphicsUnit);

            string text = bumpership.Name + " (" + bumpership.Score + ")";
            SizeF textSize = renderSurface.MeasureString(text, font);
            
            renderSurface.DrawString(text,
                                     font,
                                     fontColor,
                                     (float)(viewPosition.X * CellSize) - textSize.Width / 2,
                                     (float)(viewPosition.Y * CellSize) + 25);

            IEnumerable<Cell> neighborTiles = bumpership.GetTouchingCells();
#if DEBUG
            double radians = bumpership.Angle * Math.PI / 180;
            float x = (float) (Math.Cos(radians) * bumpership.Radius + bumpership.Position.X);
            float y = (float) ((float) Math.Sin(radians) * bumpership.Radius + bumpership.Position.Y);

            renderSurface.DrawLine(new Pen(Color.White),
                                   (float) bumpership.Position.X * CellSize,
                                   (float) bumpership.Position.Y * CellSize,
                                   x * CellSize,
                                   y * CellSize);


            foreach (Cell tile in neighborTiles)
            {
                if (tile.CellType == CellType.None)
                    continue;

                renderSurface.DrawRectangle(new Pen(new SolidBrush(Color.Green)),
                                            (float) tile.Min.X * CellSize,
                                            (float) tile.Min.Y * CellSize,
                                            CellSize,
                                            CellSize);
            }
            Cell positionCell = bumpership.GetCurrentCell();
            renderSurface.DrawRectangle(new Pen(new SolidBrush(Color.Red)),
                                        (float) positionCell.Min.X * CellSize,
                                        (float) positionCell.Min.Y * CellSize,
                                        CellSize,
                                        CellSize);

#else
            RectangleF region = RectangleF.FromLTRB(
                ix+ bounds.Left,
                iy-bounds.Top,
                ix+bounds.Right,
                iy+bounds.Bottom
                );

            paintRegion.Enqueue(region);
#endif
        }

        public void RenderMap(Map map)
        {
            this.map = map;
            if (mapAsImage == null)
                mapAsImage = GetMapAsImage();

            renderSurface.DrawImage(mapAsImage, 0, 0);

            foreach (Cell cell in this.map.Grid)
            {
                if (cell.CellType != CellType.Attractor)
                    continue;

                Rectangle region = new Rectangle((int)(cell.Min.X * CellSize),
                                 (int)(cell.Min.Y * CellSize),
                                 (int)CellSize,
                                 (int)CellSize);

                renderSurface.DrawImage(textures[cell.CellType.ToString()], region);
                paintRegion.Enqueue(region);
            }

            if (redrawCounter < 0)
            {
                paintRegion.Enqueue(new Rectangle(0, 0, 1280, 720));
                redrawCounter = 30;
            }
            else
                redrawCounter--;
        }

        private Bitmap GetMapAsImage()
        {
            Bitmap map = new Bitmap(backbuffer.Width, backbuffer.Height);
            map.SetResolution(backbuffer.HorizontalResolution, backbuffer.VerticalResolution);
            using(Graphics g = Graphics.FromImage(map))
            {
                g.DrawImage(textures["bg"], new Rectangle(0, 0, 1280, 720));

                foreach (Cell cell in this.map.Grid)
                {
                    if (cell.CellType == CellType.None)
                        continue;

                    Rectangle region = new Rectangle((int) (cell.Min.X * CellSize),
                                                     (int) (cell.Min.Y * CellSize),
                                                     (int) CellSize,
                                                     (int) CellSize);

                    g.DrawImage(textures[CellType.None.ToString()], region);

                    if (cell.CellType == CellType.Blocked
                        || cell.CellType == CellType.Boost
                        || cell.CellType == CellType.SlowDown)
                        g.DrawImage(textures[cell.CellType.ToString()], region);
                }

                IEnumerable<Cell> startCells = this.map.StartPositions.Select<Vector, Cell>(this.map.Grid.GetCell);
                foreach (Cell startCell in startCells)
                {
                    g.DrawImage(textures["startCell"], (int)(startCell.Min.X * CellSize),
                                 (int)(startCell.Min.Y * CellSize),
                                 (int)CellSize,
                                 (int)CellSize);
                }

                g.DrawImage(map, 0, 0);
                return map;
            }
        }

        public bool BeforeRender(float deltaTime)
        {
            currentDeltaTime = deltaTime;

            if (!Created)
                return false;

            if (Win32.PeekMessage(out msg, Handle, 0, 0, (uint) Win32.PM.REMOVE))
            {
                if (msg.message == (uint) Win32.WindowsMessage.WM_QUIT)
                    return false;

                Win32.TranslateMessage(ref msg);
                Win32.DispatchMessage(ref msg);
            }
            timer.Update();

            renderSurface = Graphics.FromImage(backbuffer);
            //renderSurface.SmoothingMode = SmoothingMode.HighQuality;

            return true;
        }

        public void AfterRender()
        {
            if (!Created)
                return;

            #if DEBUG
            renderSurface.DrawString("FPS: " + (int) timer.FPS,
                                     new Font(FontFamily.GenericSansSerif, 14),
                                     this.greyBrush,
                                     10,
                                     10);
            #endif
            // Flip the backbuffer
            Graphics frontBuffer = Graphics.FromHwnd(Handle);
#if DEBUG
            frontBuffer.DrawImageUnscaled(backbuffer, 0, 0);
#else
            while (paintRegion.Any())
            {
                RectangleF region = paintRegion.Dequeue();
                //renderSurface.DrawRectangle(new Pen(Color.DeepSkyBlue), Rectangle.Round(region));
                frontBuffer.DrawImage(backbuffer, region, region, GraphicsUnit.Pixel);
            }
#endif

            renderSurface.Clear(Color.White);
        }
        #endregion

        private void IncrementShapeCounter()
        {
            shapeCounter++;
            if (shapeCounter > 4)
                shapeCounter = 1;
        }

        private static Image RotateImage(Image image, Single angle)
        {
            Bitmap bitmap = new Bitmap(image.Width + 75, image.Height + 100);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TranslateTransform(bitmap.Width / 2, bitmap.Height / 2);
                g.RotateTransform(angle);
                g.TranslateTransform(-image.Width / 2, -image.Height / 2);
                g.DrawImage(image, Point.Empty);
            }
            return bitmap;
        }
    }
}