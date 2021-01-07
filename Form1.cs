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
     * Player grows when eating
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     */
    public partial class Form1 : Form
    {
        int mapSize = 8000;
        Pen gridPen = new Pen(Color.Black, 1);
        Player player = new Player();

        int foodAmount = 10000;
        List<Food> foodItems = new List<Food>();
        Random random = new Random();
        bool[] keyPressed = new bool[5];
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            gameTimer.Enabled = true;
            player.size = 10;  
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
            }
        }
        private void gameTimer_Tick(object sender, EventArgs e)
        {
            if (keyPressed[0] == true)
            {
                player.y -= player.speed;
            }
            if (keyPressed[1] == true)
            {
                player.y += player.speed;
            }
            if (keyPressed[2] == true)
            {
                player.x -= player.speed;
            }
            if (keyPressed[3] == true)
            {
                player.x += player.speed;
            }
            if (foodItems.Count < foodAmount)
            {
                for (int i = 0; i <= foodAmount - foodItems.Count; i++)
                {
                    Food food = new Food();
                    food.x = random.Next(1, mapSize);
                    food.y = random.Next(1, mapSize);
                    food.color = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                    foodItems.Add(food);
                }
            }
            Refresh();
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
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
                }
            }
        }
    }

    public class Player
    {
        public int size = 10;
        public int speed = 2;
        public int x = 4000;
        public int y = 4000;
    }

    public class Food
    {
        public int size = 1;
        public int x;
        public int y;
        public Color color = new Color();
    }
}