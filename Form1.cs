using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

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

        int mergeTime = 30000;
        Pen gridPen = new Pen(Color.Black, 1);
        Pen redPen = new Pen(Color.Red, 1);
        Player player = new Player();
        public static List<Player> playerObjects = new List<Player>();
        public static List<Player> enemies = new List<Player>();

        bool loaded3D = false;

        int enemyCount = 10;
        int foodAmount = 60000;
        List<Food> foodItems = new List<Food>();
        public static Random random = new Random();
        bool[] keyPressed = new bool[6];

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
                case Keys.E:
                    keyPressed[5] = true;
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
                case Keys.E:
                    keyPressed[5] = false;
                    break;
            }
        }
        private void gameTimer_Tick(object sender, EventArgs e)
        {
            if (enemies.Count < enemyCount)
            {
                Player p = new Player();
                p.x = random.Next(0, mapSize);
                p.y = random.Next(0, mapSize);
                p.color = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                p.size = random.Next(100, 300);
                enemies.Add(p);
            }
            player.speed = player.baseSpeed * Qrsqrt(player.size * 0.1f);
            foreach (Player p in playerObjects)
            {
                p.speed = p.baseSpeed * Qrsqrt(p.size * 0.1f);
                if (p.speed < 0)
                {
                    p.speed *= -1;
                }
            }
            foreach (Player p in enemies)
            {
                p.speed = p.baseSpeed * Qrsqrt(p.size * 0.1f);
                if (p.speed < 0)
                {
                    p.speed *= -1;
                }
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
                    p.y -= Convert.ToInt32(p.speed);
                }
            }
            if (keyPressed[1] == true)
            {
                player.y += Convert.ToInt32(player.speed);
                foreach (Player p in playerObjects)
                {
                    p.y += Convert.ToInt32(p.speed);
                }
            }
            if (keyPressed[2] == true)
            {
                player.x -= Convert.ToInt32(player.speed);
                foreach (Player p in playerObjects)
                {
                    p.x -= Convert.ToInt32(p.speed);
                }
            }
            if (keyPressed[3] == true)
            {
                player.x += Convert.ToInt32(player.speed);
                foreach (Player p in playerObjects)
                {
                    p.x += Convert.ToInt32(p.speed);
                }
            }
            if (keyPressed[4] == true)
            {
                player.Split();
            }
            if (keyPressed [5] == true && playerObjects.Count > 0)
            {
                foreach (Player p in playerObjects)
                {
                    if (p.x > player.x + this.Width / 2 + player.size / 2)
                    {
                        p.x -= Convert.ToInt32(p.speed);
                    }
                    else if (p.x < player.x + this.Width / 2 - player.size / 2)
                    {
                        p.x += Convert.ToInt32(p.speed);
                    }
                    if (p.y > player.y + this.Height / 2 - player.size / 2)
                    {
                        p.y -= Convert.ToInt32(p.speed);
                    }
                    else if (p.y < player.y + this.Height / 2 - player.size / 2)
                    {
                        p.y += Convert.ToInt32(p.speed);
                    }
                }
            }

            //Making hitboxes for split player cells
            foreach (Player p in playerObjects)
            {
                Rectangle hitbox = new Rectangle(p.x, p.y, Convert.ToInt32(p.size / 2), Convert.ToInt32(p.size / 2));
                p.hitbox = hitbox;
            }

            //Making hitboxes for enemies, calculating enemy movement and behavior

            for (int i = enemies.Count - 2; i >= 0; i--)
            {
                if (enemies[i].hitbox.IntersectsWith(player.hitbox))
                {
                    if (player.size > enemies[i].size)
                    {
                        player.size += enemies[i].size;
                        enemies[i] = null;
                        enemies.RemoveAt(i);
                    }
                    else
                    {
                        enemies[i].size += player.size;
                        player = null;
                    }
                }

                for (int j = enemies.Count - 2; j >= 0; j--)
                {
                    if (enemies[i].hitbox.IntersectsWith(enemies[j].hitbox) && i != j)
                    {
                        if (enemies[i].size > enemies[j].size)
                        {
                            enemies[i].size += enemies[j].size;
                            enemies[j] = null;
                            enemies.RemoveAt(j);

                        }
                        else
                        {
                            enemies[j].size += enemies[i].size;
                            enemies[i] = null;
                            enemies.RemoveAt(i);
                        }
                    }
                }
            }
            foreach (Player p in enemies)
            {
                Rectangle hitbox = new Rectangle(p.x, p.y, Convert.ToInt32(p.size / 2), Convert.ToInt32(p.size / 2));
                p.hitbox = hitbox;
                int[] closest = {1000, 1000};
                int _distance = 10000;
                int _bestDistance = 10000;

                foreach (Food f in foodItems)
                {
                    _distance = Math.Abs(p.x - f.x) + Math.Abs(p.y - f.y);
                    
                    if (f.x > p.x - this.Width / 2 && f.x < p.x + this.Width / 2 && f.y > p.y - this.Height / 2 && f.y < p.y + this.Height / 2 && p.chase == false && p.flee == false)
                    {
                        if (_distance <= _bestDistance)
                        {
                            _bestDistance = _distance;
                            closest[0] = f.x;
                            closest[1] = f.y;
                        }
                        if (p.x > closest[0])
                        {
                            p.x -= Convert.ToInt32(p.speed);
                        }
                        else if (p.x < closest[0])
                        {
                            p.x += Convert.ToInt32(p.speed);
                        }
                        if (p.y > closest[1])
                        {
                            p.y -= Convert.ToInt32(p.speed);
                        }
                        else if (p.y < closest[1])
                        {
                            p.y += Convert.ToInt32(p.speed);
                        }
                    }
                }
                //TESTING
                /*int[] test = { 10000, 10000 };
                if (Math.Abs(p.x - player.x) < closest[0] && Math.Abs(p.y - player.y) < closest[1])
                {
                    test[0] = p.x;
                    test[1] = p.y;
                }
                debugLabel.Text = player.x + "\n" + player.y + "\n" + test[0] + "\n" + test[1];*/
            }
            //Testing AI
            int[] test = new int[2];
            int bestDistance = 10000;
            int distance = 10000;
            foreach (Food f in foodItems)
            {
                if (f.x > player.x && f.x < player.x + this.Width && f.y > player.y && f.y < player.y + this.Height && player.chase == false && player.flee == false)
                {
                    distance = Math.Abs(player.x + this.Width / 2 - f.x) + Math.Abs(player.y + this.Height / 2 - f.y);
                    if (distance <= bestDistance)
                    {
                        bestDistance = distance;
                        test[0] = f.x - this.Width / 2;
                        test[1] = f.y - this.Height / 2;
                    }
                    
                }
            }
            if (player.x > test[0])
            {
                player.x -= Convert.ToInt32(player.speed);
            }
            else if (player.x < test[0])
            {
                player.x += Convert.ToInt32(player.speed);
            }
            if (player.y > test[1])
            {
                player.y -= Convert.ToInt32(player.speed);
            }
            else if (player.y < test[1])
            {
                player.y += Convert.ToInt32(player.speed);
            }

            if (playerObjects.Count > 0)
            {
                foreach (Player p in playerObjects)
                {
                    if (p.x > player.x + this.Width / 2 + player.size / 2)
                    {
                        p.x -= Convert.ToInt32(p.speed);
                    }
                    else if (p.x < player.x + this.Width / 2 - player.size / 2)
                    {
                        p.x += Convert.ToInt32(p.speed);
                    }
                    if (p.y > player.y + this.Height / 2 - player.size / 2)
                    {
                        p.y -= Convert.ToInt32(p.speed);
                    }
                    else if (p.y < player.y + this.Height / 2 - player.size / 2)
                    {
                        p.y += Convert.ToInt32(p.speed);
                    }
                }
            }
            debugLabel.Text = player.x + " " + player.y + "\n" + test[0] + " " + test[1];
                //End of testing AI

                Rectangle playerBox = new Rectangle(this.Width / 2 - Convert.ToInt32(player.size / 4) + player.x, this.Height / 2 - Convert.ToInt32(player.size / 4) + player.y, Convert.ToInt32(player.size / 2), Convert.ToInt32(player.size / 2));
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

                    for (int x = playerObjects.Count - 1; x >= 0; x--)
                    {
                        if (playerObjects[x].hitbox.IntersectsWith(foodItems[i].hitbox))
                        {
                            foodItems[i] = null;
                            foodItems.RemoveAt(i);
                            playerObjects[x].size++;
                        }
                        if (playerObjects[x].hitbox.IntersectsWith(playerBox) && playerObjects[x].mergeTimer.ElapsedMilliseconds >= mergeTime)
                        {
                            player.size += playerObjects[x].size;

                            player.mergeTimer.Reset();
                            player.mergeTimer.Stop();
                            playerObjects[x].mergeTimer.Reset();
                            playerObjects[x].mergeTimer.Stop();
                            playerObjects[x] = null;
                            playerObjects.RemoveAt(x);
                        }
                    }
                }
                for (int i = foodItems.Count - 5; i >= 0; i--)
                {
                    foreach (Player p in enemies)
                    {
                        if (p.hitbox.IntersectsWith(foodItems[i].hitbox))
                        {
                            foodItems[i] = null;
                            foodItems.RemoveAt(i);
                            p.size++;
                        }
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
                if (p.x >= player.x - this.Width && p.x <= player.x + this.Width && p.y >= player.y - this.Height && p.y <= player.y + this.Height)
                {
                    e.Graphics.FillRectangle(playerBrush, p.x - player.x, p.y - player.y, Convert.ToInt32(p.size / 2), Convert.ToInt32(p.size / 2));
                }
            }
            foreach (Player p in enemies)
            {
                if (p.x >= player.x - this.Width && p.x <= player.x + this.Width && p.y >= player.y - this.Height && p.y <= player.y + this.Height)
                {
                    Brush eBrush = new SolidBrush(p.color);
                    e.Graphics.FillRectangle(eBrush, p.x - player.x, p.y - player.y, Convert.ToInt32(p.size / 2), Convert.ToInt32(p.size / 2));
                }
            }


        }

        static float Qrsqrt(float number)
        {
            unsafe
            {
                long i;
                float x2, y;

                x2 = number * 0.5f;
                y = number;
                i = *(long*)&y;                       // evil floating point bit level hacking
                i = 0x5f3759df - (i >> 1);               
                y = *(float*)&i;
                y = y * (1.5f - (x2 * y * y));   
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
        public float size = 100;
        public float speed = 2;
        public int x = 4000;
        public int y = 4000;
        public bool chase = false;
        public bool flee = false;
        public Color color = new Color();
        public Rectangle hitbox = new Rectangle();
        public Stopwatch mergeTimer = new Stopwatch();
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
                p.baseSpeed = 18;
                this.mergeTimer.Start();
                p.mergeTimer.Start();
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