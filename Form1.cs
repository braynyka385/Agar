using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Agar
{
    /*Primary Wants:
     * Actually show player in centre of screen
     * Player grows when eating, eating in general
     * Map border
     * Splitting
     * Speed changes with player size
     * Skins, audio (not that important but very easy)
     * 
     * Secondary wants: 
     * Viruses
     * Basic AI enemies
     * Eat enemies, only when size is larger than them
     * Feeding (not too hard, could prob do in 5 mins)
     * 
     * Tertiary Goals: 
     * Networking
     * Multiplayer
     * Adaptive view area
     */
    public partial class Form1 : Form
    {
        public static int mapSize = 24000;
        public static int mapScale = 25;
        public static int defMapScale = 25;
        Pen gridPen = new Pen(Color.Black, 1);
        Pen redPen = new Pen(Color.Red, 1);
        Player player = new Player();
        public static List<Player> playerObjects = new List<Player>();

        int foodAmount = 50000;
        List<Food> foodItems = new List<Food>();
        public static Random random = new Random();
        bool[] keyPressed = new bool[5];

        public static int[] mousePos = new int[2];

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            gameTimer.Enabled = true;
            //player.size = 10;  
            player.color = Color.FromArgb(random.Next(1, 255), random.Next(1, 255), random.Next(1, 255));
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    keyPressed[0] = true;
                    break;
                case Keys.S:
                    keyPressed[1] = true;
                    break;
                case Keys.A:
                    keyPressed[2] = true;
                    break;
                case Keys.D:
                    keyPressed[3] = true;
                    break;
                case Keys.Space:
                    keyPressed[4] = true;
                    break;
            }
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    keyPressed[0] = false;
                    break;
                case Keys.S:
                    keyPressed[1] = false;
                    break;
                case Keys.A:
                    keyPressed[2] = false;
                    break;
                case Keys.D:
                    keyPressed[3] = false;
                    break;
                case Keys.Space:
                    keyPressed[4] = false;
                    break;
            }
        }
        private void gameTimer_Tick(object sender, EventArgs e)
        {
            if (keyPressed[4] == true)
            {
                player.Split();
            }
            Graphics g = this.CreateGraphics();
            Rectangle playerBox = new Rectangle(this.Width / 2 - Convert.ToInt32(player.size / 4) + player.x, this.Height / 2 - Convert.ToInt32(player.size / 4) + player.y, Convert.ToInt32(player.size / 2), Convert.ToInt32(player.size / 2));
            g.DrawRectangle(redPen, playerBox);
            if (foodItems.Count > 0)
            {
                for (int i = foodItems.Count - 1; i >= 0; i--)
                {
                    if (foodItems[i].hitbox.IntersectsWith(playerBox))
                    {
                        foodItems[i] = null;
                        foodItems.RemoveAt(i);
                        player.size++;
                    }
                }
            }
            
            
            player.speed = player.baseSpeed * Qrsqrt(player.size * 0.1f);
            foreach (Player p in playerObjects)
            {
                p.speed = p.baseSpeed * Qrsqrt(p.size * 0.1f);
            }

            if (player.speed < 0)
            {
                player.speed *= -1;
            }
            if (keyPressed[0] == true)
            {
                player.y -= Convert.ToInt32(player.speed);
                foreach (Player p in playerObjects)
                {
                    if (player.y - p.y > 100)
                    {
                        p.y -= Convert.ToInt32(p.speed);
                    }
                }
            }
            if (keyPressed[1] == true)
            {
                player.y += Convert.ToInt32(player.speed);
                foreach (Player p in playerObjects)
                {
                    if (p.y - player.y < this.Height - 100)
                    {
                        p.y += Convert.ToInt32(p.speed);
                    }
                }
            }
            if (keyPressed[2] == true)
            {
                player.x -= Convert.ToInt32(player.speed);
                foreach (Player p in playerObjects)
                {
                    if (player.x - p.x > 0)
                    {
                        p.x -= Convert.ToInt32(p.speed);
                    }
                }
            }
            if (keyPressed[3] == true)
            {
                player.x += Convert.ToInt32(player.speed);
                foreach (Player p in playerObjects)
                {
                    if (player.x - p.x > this.Width)
                    {
                        p.x += Convert.ToInt32(p.speed);
                    }
                }
            }
            if (foodItems.Count < foodAmount)
            {
                for (int i = 0; i <= foodAmount - foodItems.Count; i++)
                {
                    int x = random.Next(1, mapSize);
                    int y = random.Next(1, mapSize);
                    Food food = new Food();
                    food.x = x;
                    food.y = y;
                    food.color = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                    Rectangle hitbox = new Rectangle(x, y, 5, 5);
                    food.hitbox = hitbox;
                    //food.Generate();
                    
                    foodItems.Add(food);
                }
            }
            Refresh();
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Brush playerBrush = new SolidBrush(player.color);

            //Show a grid pattern
            
            for (int x = 0; x <= mapSize; x += 25)
            {
                if (x >= player.x - this.Width && x <= player.x + this.Width)
                {
                    e.Graphics.DrawLine(gridPen, x - player.x, 0, x - player.x, this.Height);
                }
            }
            for (int y = 0; y <= mapSize; y += 25)
            {
                if (y >= player.y - this.Height && y <= player.y + this.Height)
                {
                    e.Graphics.DrawLine(gridPen, 0, y - player.y, this.Width, y - player.y);
                }
            }

            //Display local food
            foreach (Food f in foodItems)
            {
                if (f.x >= player.x - this.Width && f.x <= player.x + this.Width && f.y >= player.y - this.Height && f.y <= player.y + this.Height)
                {
                    Pen pen = new Pen(f.color, 4);
                    e.Graphics.DrawEllipse(pen, f.x - player.x, f.y - player.y, 5, 5);
                    //e.Graphics.DrawRectangle(pen, f.hitbox);
                }
            }
            e.Graphics.FillRectangle(playerBrush, this.Width / 2 - Convert.ToInt32(player.size / 4), this.Height / 2 - Convert.ToInt32(player.size / 4), Convert.ToInt32(player.size / 2), Convert.ToInt32(player.size / 2));
            foreach (Player p in playerObjects)
            {
                e.Graphics.FillRectangle(playerBrush, this.Width / 2 - Convert.ToInt32(p.size / 4) + player.x - p.x, this.Height / 2 - Convert.ToInt32(p.size / 4) + player.y - p.y, Convert.ToInt32(p.size / 2), Convert.ToInt32(p.size / 2));
            }
        }

        static float Qrsqrt(float number)
        {
            unsafe
            {
                long i;
                float x2, y;
                float threehalfs = 1.5f;

                x2 = number * 0.5f;
                y = number;
                i = *(long*)&y;                       // evil floating point bit level hacking
                i = 0x5f3759df - (i >> 1);               // what the fuck? 
                y = *(float*)&i;
                y = y * (threehalfs - (x2 * y * y));   // 1st iteration
                                                       //y = y * (threehalfs - (x2 * y * y));   // 2nd iteration, this can be removed
                return y;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            mousePos[0] = e.Location.X;
            mousePos[1] = e.Location.Y;
        }
    }

    public class Player
    {
        public int baseSpeed = 18;
        //public int normalSpeed = 2;
        public float size = 100;
        public float speed = 2;
        public int x = 4000;
        public int y = 4000;
        public Color color = new Color();
        public void Split()
        {
            if (this.size > 100)
            {
                if (Form1.playerObjects.Count > 0)
                {
                    foreach (Player f in Form1.playerObjects)
                    {

                    }
                }
                Player p = new Player();
                p.size = this.size / 2;
                p.x = this.x - 400 + Form1.mousePos[0];
                p.y = this.y - 300 + Form1.mousePos[1];
                this.size /= 2;
                p.baseSpeed = 12;
                Form1.playerObjects.Add(p);
            }

            
        }
    }

    public class Food
    {
        public int size = 1;
        public int x;
        public int y;
        public Rectangle hitbox = new Rectangle();
        public void Generate()
        {
            x = Form1.random.Next(1, Form1.mapSize);
            y = Form1.random.Next(1, Form1.mapSize);
            hitbox.X = x;
            hitbox.Y = y;
            hitbox.Width = 1;
            hitbox.Height = 1;
        }
        
        public Color color = new Color();
    }
}