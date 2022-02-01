

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace AimExample
{
    public partial class Form1 : Form
    {
        List<Player>? playerList;

        Bitmap bmpLive2D;
        Bitmap bmpLast2D;
        Bitmap bmpLive3D;
        Bitmap bmpLast3D;

        Pen p1Pen = new Pen(Color.Green);
        Pen p2Pen = new Pen(Color.Red);
        int circleSize = 20;

        GLControl glControl1;

        public Form1()
        {
            InitializeComponent();
            bmpLive2D = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            bmpLast2D = (Bitmap)bmpLive2D.Clone();
            bmpLive3D = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            bmpLast3D = (Bitmap)bmpLive3D.Clone();
            playerList = new List<Player>()
            {
               new Player()
                {
                    X = trackBar1.Value,
                    Y = trackBar2.Value,
                    Z = trackBar3.Value,
                    Rotation = trackBar4.Value,
                    Pitch = trackBar5.Value,
                    pen = p1Pen,
                    me=true,
                },
                new Player()
                {
                    X = trackBar10.Value,
                    Y = trackBar9.Value,
                    Z = trackBar8.Value,
                    Rotation = trackBar7.Value,
                    Pitch = trackBar6.Value,
                    pen = p2Pen,
                    me = false,
                }
            };
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            playerList[0].X = trackBar1.Value;
            playerList[0].Y = trackBar2.Value;
            playerList[0].Z = trackBar3.Value;
            playerList[0].Rotation = trackBar4.Value;
            playerList[0].Pitch = trackBar5.Value;

            playerList[1].X = trackBar10.Value;
            playerList[1].Y = trackBar9.Value;
            playerList[1].Z = trackBar8.Value;
            playerList[1].Rotation = trackBar7.Value;
            playerList[1].Pitch = trackBar6.Value;

            label11.Text = playerList[0].X.ToString();
            label12.Text = playerList[0].Y.ToString();
            label13.Text = playerList[0].Z.ToString();
            label14.Text = playerList[0].Rotation.ToString();
            label15.Text = playerList[0].Pitch.ToString();

            label16.Text = playerList[1].X.ToString();
            label17.Text = playerList[1].Y.ToString();
            label18.Text = playerList[1].Z.ToString();
            label19.Text = playerList[1].Rotation.ToString();
            label20.Text = playerList[1].Pitch.ToString();

            Draw2D();
           Draw3D();
        }

        void SetupViewPort()
        {
            glControl1 = new GLControl();
            glControl1.Size = new System.Drawing.Size(200, 200);
            glControl1.Dock = DockStyle.Fill;
            panel1.Controls.Add(glControl1);

            float wt = Math.Max(1, glControl1.Width);
            float ht = Math.Max(1, glControl1.Height);
            float sz = (float)Math.Sqrt(ht * wt);
            GL.Viewport((int)(wt - sz) / 2, (int)(ht - sz) / 2, (int)sz, (int)sz);
            var ortho = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45.0f), 1f, 0.1f, 500f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref ortho);

            glControl1.Paint += glControl1_Paint;
            GL.Enable(EnableCap.DepthTest);
        }

        private void Draw2D()
        {
            bmpLive2D = (Bitmap)bmpLast2D.Clone();
            using (Graphics g = Graphics.FromImage(bmpLive2D))
            {
                g.Clear(Color.Black);

                foreach (var player in playerList)
                {
                    double rad = player.Rotation * Math.PI / 180;
                    var x2 = player.X + 40 * Math.Sin(rad);
                    var y2 = player.Y + 40 * Math.Cos(rad);

                    g.DrawEllipse(player.pen, player.X - circleSize / 2, bmpLive2D.Height - player.Y - circleSize / 2, circleSize, circleSize);
                    g.DrawLine(player.pen, player.X, bmpLive2D.Height - player.Y, (int)(x2), bmpLive2D.Height - (int)y2);
                }
                
                bmpLast2D = (Bitmap)bmpLive2D.Clone();
                bmpLive2D.Dispose();
                pictureBox1.Image = bmpLast2D;
            }
        }

        private void Draw3D()
        {
            glControl1.Refresh();
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            glControl1.MakeCurrent();
            GL.ClearColor(glControl1.BackColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            SetupCamera();
            foreach (var player in playerList.Where(x => x.me == false))
            {
                // Draw a single point
                GL.PointSize(20f);
                var vector = player.PointCoordinates;
                GL.Begin(PrimitiveType.Points);
                GL.Color3(Color.Red);
                GL.Vertex4(vector);
                GL.End();
            }


            glControl1.SwapBuffers();
        }

        void SetupCamera()
        {
            var me = playerList.FirstOrDefault(x => x.me);

            double rad = me.Rotation * Math.PI / 180;
            double pitch = me.Pitch * Math.PI / 180;
            var x2 = (float)(me.X + 400 * Math.Sin(rad));
            var y2 = (float)(me.Y + 400 * Math.Cos(rad));
            var z2 = (float)(me.Z + 400 * Math.Cos(pitch));

            Matrix4 lookAt = Matrix4.LookAt(
                            me.X, me.Y, me.Z,
                            x2, y2, z2,
                            0f, 0f, 1f);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookAt);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //SetupViewPort();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SetupViewPort();
            timer1.Enabled = true;
        }
    }
    public class Player
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Rotation { get; set; }
        public float Pitch { get; set; }
        public Pen? pen { get; set; }
        public bool me { get; set; }
        public float prevY { get; set; }
        public Vector4 PointCoordinates { 
            get
            {
                return new Vector4(X, Y, Z, 1f);
            }
        }
    }

}