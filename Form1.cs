using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Media;

namespace Agar
{
    /*Primary Wants:
     * Actually show player in centre of screen | DONE
     * Player grows when eating, eating in general | DONE
     * Map border | DONE
     * Splitting | Done?
     * Speed changes with player size | DONE
     * Skins, audio (not that important but very easy)
     * 
     * Secondary wants: 
     * Viruses // Probably not
     * Basic AI enemies | Done?
     * Eat enemies, only when size is larger than them | Done?
     * Feeding (not too hard, could prob do in 5 mins) // Probably won't do
     * 
     * Tertiary Goals: 
     * Networking // NOPE
     * Multiplayer // NOPE
     * Adaptive view area // Probably not
     */
    public partial class Form1 : Form
    {
        //Global variables

        public static int mapSize = 20000; //Size of map


        int[] leaderboard = new int[5]; //Tracks leaderboard scores
        bool[] playerBoard = new bool[5]; //Checks if a leaderboard score is for the player
        bool[] lastPlayerBoard = new bool[5];

        int mergeTime = 30000; //Amount of time to merge
        Pen gridPen = new Pen(Color.Black, 1); //Draws grid
        Pen redPen = new Pen(Color.Red, 1);
        Player player = new Player(); //Create player
        public static List<Player> playerObjects = new List<Player>(); //Tracks splits of the player
        public static List<Player> enemies = new List<Player>(); //Tracks enemies
        Brush playerBrush; //To draw player/player splits
        Brush eBrush; //To draw enemies

        int enemyCount = 15; //Assigns limit to how many enemies there are
        int foodAmount = 20000; //Assigns limit to how much food there is
        List<Food> foodItems = new List<Food>(); //Tracks food
        public static Random random = new Random(); //Obvious what this does
        bool[] keyPressed = new bool[6]; //Tracks what keys are pressed

        Stopwatch splitTimer = new Stopwatch(); //Times when pieces can merge
        bool firstSplit = true; //Tracks if it is the first ever split. Used to fix bug
        int splitTime = 1000; //Amount of time between splits

        public static int[] mousePos = new int[2]; //Tracks the position of the mouse for splitting

        Pen pen = new Pen(Color.Red, 4); //Draws food

        int selectedColour = 0;
        string name;

        bool generating = true;

        System.Windows.Media.MediaPlayer leading;

        SoundPlayer eating = new SoundPlayer(Properties.Resources.slurp);

        Font nameFont = new Font("Arial", 16, FontStyle.Bold);
        SolidBrush nameBrush = new SolidBrush(Color.Black);

        public Form1()
        {
            InitializeComponent();

            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            leading = new System.Windows.Media.MediaPlayer();
            leading.Open(new Uri(Application.StartupPath + "/Resources/first.wav"));

            Begin();
        }

        private void Begin()
        {
            gameTimer.Enabled = false;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i] = null;
                enemies.RemoveAt(i);
            }
            nameBox.Visible = true;
            selectedLabel.Visible = true;
            redButton.Visible = true;
            blueButton.Visible = true;
            greenButton.Visible = true;
            randomButton.Visible = true;
            nameLabel.Visible = true;
            colourLabel.Visible = true;
            startLabel.Visible = true;
            playButton.Visible = true;
            leaveButton.Visible = true;
        }

        //Checking for key presses
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

        //Checking for when key presses end
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
            //Generates new enemies if there aren't enough
            while (enemies.Count < enemyCount) 
            {
                GenerateEnemies();
            }
            //Calculating the speed of enemies, player
            SpeedCalc();

            //What to do when a key is pressed
            Movement();

            //Making hitboxes for split player cells
            foreach (Player p in playerObjects)
            {
                Rectangle hitbox = new Rectangle(p.x, p.y, Convert.ToInt32(p.size / 2), Convert.ToInt32(p.size / 2));
                p.hitbox = hitbox;
            }

            //Making hitboxes for enemies, calculating enemy movement and behavior

            EnemyHitDetection();

            //Enemy AI
            EnemyAI();

            //Hit detection for food, merging mechanics
            PlayerHitDetection();

            //Generating food
            GenerateFood();
            Leaderboard();
            Refresh();
        }
        private void GenerateEnemies()
        {
            Player p = new Player();
            p.x = random.Next(0, mapSize);
            p.y = random.Next(0, mapSize);
            p.color = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
            p.size = random.Next(100, 300);
            p.baseSpeed = 18;
            enemies.Add(p);
        }
        private void SpeedCalc()
        {
            player.speed = player.baseSpeed * Qrsqrt(player.size * 0.1f);
            if (player.speed < 0)
            {
                player.speed *= -1;
            }
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
        }
        private void Movement()
        {
            if (keyPressed[0] == true && player.y + this.Height / 2 > 0)
            {
                player.y -= Convert.ToInt32(player.speed);
                foreach (Player p in playerObjects)
                {
                    if (p.y > 0 && keyPressed[5] == false)
                    {
                        p.y -= Convert.ToInt32(p.speed);
                    }
                }
            }
            if (keyPressed[1] == true && player.y + this.Height / 2 < mapSize)
            {
                player.y += Convert.ToInt32(player.speed);
                foreach (Player p in playerObjects)
                {
                    if (p.y < mapSize && keyPressed[5] == false)
                    {
                        p.y += Convert.ToInt32(p.speed);
                    }
                }
            }
            if (keyPressed[2] == true && player.x + this.Width / 2 > 0)
            {
                player.x -= Convert.ToInt32(player.speed);
                foreach (Player p in playerObjects)
                {
                    if (p.x > 0 && keyPressed[5] == false)
                    {
                        p.x -= Convert.ToInt32(p.speed);
                    }
                }
            }
            if (keyPressed[3] == true && player.x + this.Width / 2 < mapSize)
            {
                player.x += Convert.ToInt32(player.speed);
                foreach (Player p in playerObjects)
                {
                    if (p.x < mapSize && keyPressed[5] == false)
                    {
                        p.x += Convert.ToInt32(p.speed);
                    }
                }
            }
            if (keyPressed[4] == true && (splitTimer.ElapsedMilliseconds > 1000 || firstSplit == true))
            {
                splitTimer.Restart();
                player.Split();
                firstSplit = false;
            }

            //This key brings all split pieces together if the player is split
            if (keyPressed[5] == true && playerObjects.Count > 0)
            {
                foreach (Player p in playerObjects)
                {
                    if (p.x > player.x + this.Width / 2 + player.size / 5)
                    {
                        p.x -= Convert.ToInt32(p.speed);
                    }
                    else if (p.x < player.x + this.Width / 2 - player.size / 5)
                    {
                        p.x += Convert.ToInt32(p.speed);
                    }
                    if (p.y > player.y + this.Height / 2 - player.size / 5)
                    {
                        p.y -= Convert.ToInt32(p.speed);
                    }
                    else if (p.y < player.y + this.Height / 2 - player.size / 5)
                    {
                        p.y += Convert.ToInt32(p.speed);
                    }
                }
            }
        }
        private void EnemyAI()
        {
            foreach (Player p in enemies)
            {
                int[] closest = { 1000, 1000 };
                int _distance = 10000;
                int _bestDistance = 10000;
                foreach (Player f in enemies)
                {

                    if (f.x > p.x - this.Width / 2 && f.x < p.x + this.Width / 2 && f.y > p.y - this.Height / 2 && f.y < p.y + this.Height / 2 && f != p)
                    {
                        if (f.size > p.size)
                        {
                            p.flee = true;
                            p.chase = false;
                        }
                        else
                        {
                            p.chase = true;
                            p.flee = false;
                        }
                        if (_distance <= _bestDistance)
                        {
                            _bestDistance = _distance;
                            closest[0] = f.x - Convert.ToInt32(p.size / 4);
                            closest[1] = f.y - Convert.ToInt32(p.size / 4);
                        }
                        _distance = Math.Abs(p.x - f.x) + Math.Abs(p.y - f.y);
                    }
                    else
                    {
                        p.chase = false;
                        p.flee = false;
                    }
                }
                if (player.x > p.x - this.Width / 2 && player.x < p.x + this.Width / 2 && player.y > p.y - this.Height / 2 && player.y < p.y + this.Height / 2)
                {
                    if (player.size > p.size)
                    {
                        p.flee = true;
                        p.chase = false;
                    }
                    else
                    {
                        p.chase = true;
                        p.flee = false;
                    }
                    if (_distance <= _bestDistance)
                    {
                        _bestDistance = _distance;
                        closest[0] = player.x - Convert.ToInt32(p.size / 4) + this.Width / 2;
                        closest[1] = player.y - Convert.ToInt32(p.size / 4) + this.Height / 2;
                    }
                    _distance = Math.Abs(p.x - player.x) + Math.Abs(p.y - player.y);
                }

                if (p.chase == false && p.flee == false)
                {
                    foreach (Food f in foodItems)
                    {
                        _distance = Math.Abs(p.x - f.x) + Math.Abs(p.y - f.y);

                        if (f.x > p.x - this.Width / 2 && f.x < p.x + this.Width / 2 && f.y > p.y - this.Height / 2 && f.y < p.y + this.Height / 2 && p.chase == false && p.flee == false)
                        {
                            if (_distance <= _bestDistance)
                            {
                                _bestDistance = _distance;
                                closest[0] = f.x - Convert.ToInt32(p.size / 4);
                                closest[1] = f.y - Convert.ToInt32(p.size / 4);
                            }
                        }
                    }
                }

                if (p.flee != true)
                {
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
                else
                {
                    if (p.x > closest[0] && p.x < mapSize)
                    {
                        p.x += Convert.ToInt32(p.speed);
                    }
                    else if (p.x < closest[0] && p.x > 0)
                    {
                        p.x -= Convert.ToInt32(p.speed);
                    }
                    if (p.y > closest[1] && p.y < mapSize)
                    {
                        p.y += Convert.ToInt32(p.speed);
                    }
                    else if (p.y < closest[1] && p.y > 0)
                    {
                        p.y -= Convert.ToInt32(p.speed);
                    }
                }

            }
        }
        private void EnemyHitDetection()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Rectangle hitbox = new Rectangle(enemies[i].x, enemies[i].y, Convert.ToInt32(enemies[i].size / 2), Convert.ToInt32(enemies[i].size / 2));
                enemies[i].hitbox = hitbox;
                if (enemies[i].hitbox.IntersectsWith(player.hitbox))
                {
                    if (player.size > enemies[i].size)
                    {
                        player.size += enemies[i].size;
                        enemies[i] = null;
                        enemies.RemoveAt(i);
                        break;
                    }
                    else
                    {
                        enemies[i].size += player.size;
                        
                        if (playerObjects.Count > 0)
                        {
                            if (playerObjects.Count > 1)
                            {
                                int max = 0;
                                int index = 0;
                                for (int j = 0; j >= playerObjects.Count; j--)
                                {
                                    if (playerObjects[j].size > max)
                                    {
                                        index = j;
                                        max = Convert.ToInt32(playerObjects[j].size);
                                    }
                                }
                                playerObjects[index] = null;
                                playerObjects.RemoveAt(index);
                                player = playerObjects[index];
                            }
                            else
                            {
                                player.x = playerObjects[0].x - this.Width / 2;
                                player.y = playerObjects[0].y - this.Height / 2;
                                player.size = playerObjects[0].size;
                                playerObjects[0] = null;
                                playerObjects.RemoveAt(0);
                                
                            }
                        }
                        else
                        {
                            gameTimer.Enabled = false;
                            generating = true;
                            Begin();
                            break;
                        }
                    }
                    
                }

                for (int j = playerObjects.Count - 1; j >= 0; j--)
                {
                    if (enemies[i].hitbox.IntersectsWith(playerObjects[j].hitbox))
                    {
                        if (playerObjects[j].size < enemies[i].size)
                        {
                            enemies[i].size += playerObjects[j].size;
                            playerObjects[j] = null;
                            playerObjects.RemoveAt(j);
                            break;
                        }
                        else
                        {
                            playerObjects[j].size += enemies[i].size;
                            enemies[i] = null;
                            enemies.RemoveAt(i);
                            break;
                        }
                    }
                }

                if (enemies.Count == enemyCount)
                {
                    for (int j = enemies.Count - 1; j >= 0; j--)
                    {
                        if (enemies[i].hitbox.IntersectsWith(enemies[j].hitbox) && i != j)
                        {
                            if (enemies[i].size > enemies[j].size)
                            {
                                enemies[i].size += enemies[j].size;
                                enemies[j] = null;
                                enemies.RemoveAt(j);
                                break;
                            }
                            else
                            {
                                enemies[j].size += enemies[i].size;
                                enemies[i] = null;
                                enemies.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
                /*if (enemies[i].size > 2000)
                {
                    enemies[i] = null;
                    enemies.RemoveAt(i);
                    break;
                }*/
            }
        }
        private void PlayerHitDetection()
        {
            Rectangle playerBox = new Rectangle(this.Width / 2 - Convert.ToInt32(player.size / 4) + player.x, this.Height / 2 - Convert.ToInt32(player.size / 4) + player.y, Convert.ToInt32(player.size / 2), Convert.ToInt32(player.size / 2));
            player.hitbox = playerBox;
            if (foodItems.Count > 0)
            {
                for (int i = foodItems.Count - 3; i >= 0; i--)
                {
                    if (foodItems[i].hitbox.IntersectsWith(playerBox))
                    {
                        foodItems[i] = null;
                        foodItems.RemoveAt(i);
                        player.size++;
                        //eating.Play();
                    }

                    for (int x = playerObjects.Count - 1; x >= 0; x--)
                    {
                        if (playerObjects[x].hitbox.IntersectsWith(foodItems[i].hitbox))
                        {
                            foodItems[i] = null;
                            foodItems.RemoveAt(i);
                            playerObjects[x].size++;
                            //eating.Play();
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
                            break;
                        }
                    }
                }
            }
        }
        private void GenerateFood()
        {
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
        }
        private void Leaderboard()
        {
            if (generating == false)
            {
                for (int i = 0; i < 5; i++)
                {
                    lastPlayerBoard[i] = playerBoard[i];
                    playerBoard[i] = false;
                    leaderboard[i] = 0;
                }
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    enemies[j].onBoard = false;
                }
                int index = 0;
                int size = Convert.ToInt32(player.size);
                foreach (Player p in playerObjects)
                {
                    size += Convert.ToInt32(p.size);
                }
                for (int i = 0; i < 5; i++)
                {
                    for (int j = enemies.Count - 1; j >= 0; j--)
                    {
                        if (enemies[j].size > leaderboard[i] && enemies[j].onBoard == false)
                        {
                            leaderboard[i] = Convert.ToInt32(enemies[j].size);
                            enemies[j].onBoard = true;
                            index = j;
                        }
                    }
                    if (size > leaderboard[i] && player.onBoard == false)
                    {
                        leaderboard[i] = Convert.ToInt32(size);
                        playerBoard[i] = true;
                        index = -1;
                        player.onBoard = true;
                    }
                    if (index != -1)
                    {
                        enemies[index].onBoard = true;
                    }

                }
                if (lastPlayerBoard[0] == false && playerBoard[0] == true)
                {
                    leading.Play();
                }

                player.onBoard = false;
            }
            
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (generating == false)
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

                        pen.Color = f.color;
                        e.Graphics.DrawEllipse(pen, f.x - player.x, f.y - player.y, 5, 5);
                        //e.Graphics.DrawRectangle(pen, f.hitbox);
                    }
                }

                //Drawing player and splits of player
                e.Graphics.FillRectangle(playerBrush, this.Width / 2 - Convert.ToInt32(player.size / 4), this.Height / 2 - Convert.ToInt32(player.size / 4), Convert.ToInt32(player.size / 2), Convert.ToInt32(player.size / 2));
                foreach (Player p in playerObjects)
                {
                    if (p.x >= player.x - this.Width && p.x <= player.x + this.Width && p.y >= player.y - this.Height && p.y <= player.y + this.Height)
                    {
                        e.Graphics.FillRectangle(playerBrush, p.x - player.x, p.y - player.y, Convert.ToInt32(p.size / 2), Convert.ToInt32(p.size / 2));
                    }
                }

                //Drawing enemies
                foreach (Player p in enemies)
                {
                    if (p.x >= player.x - this.Width && p.x <= player.x + this.Width && p.y >= player.y - this.Height && p.y <= player.y + this.Height)
                    {
                        eBrush = new SolidBrush(p.color);
                        e.Graphics.FillRectangle(eBrush, p.x - player.x, p.y - player.y, Convert.ToInt32(p.size / 2), Convert.ToInt32(p.size / 2));
                    }
                }
                //Drawing leaderboard
                debugLabel.Text = "";
                for (int i = 0; i < 5; i++)
                {
                    if (playerBoard[i] == false && leaderboard[i] != 0)
                    {
                        debugLabel.Text += i + 1 + ": " + leaderboard[i] + "\n";
                    }
                    else if (playerBoard[i] == true)
                    {
                        debugLabel.Text += i + 1 + ": " + leaderboard[i] + " (You)" + "\n";
                    }
                }

                //Drawing name of player
                e.Graphics.DrawString(name, nameFont, nameBrush, this.Width / 2 - name.Length * 4, this.Height / 2 - 8);
            }
        }

        //Calculates everything that's nonlinear. Not my code, but it's super efficient. No matter, I will explain it in comments. Comes from Quake III and is supposed to be ~3X faster than 1/sqrt(n). 
        static float Qrsqrt(float number) //Takes a float as input
        {
            unsafe //This is necessary or else it won't compile. 
            {
                long i; 
                float x2, y;
                x2 = number * 0.5f; //Gets half of the inputted value, stores it in x2
                y = number; //Stores the inputted number in y
                i = *(long*)&y;                       // evil floating point bit level hacking. Pretty much takes the inputted number (stored as y) and looks at the individual bits that create the number. It places those EXACT bits into the long. Meaning, a float with a bit value of(000000..001) would equal 1 as a long, whereas it would be a decimal number as a float.
                i = 0x5f3759df - (i >> 1);            // Takes the hexadecimal number 0x5f3759df and subtracts i/2 from it. i >> 1 is equivalent to a division of two because it takes every bit that represents the number and moves it right by one. This is not perfectly accurate (rounds odds) but it is fast.
                y = *(float*)&i;                      // Does the same as 2 lines ago, but in reverse.
                y = y * (1.5f - (x2 * y * y));        // This is a mathematical algorithm called Newton's method. Just makes all the previous approx's more accurate.
                return y;
            }
        }

        //Gets mouse position for splitting.
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            mousePos[0] = e.Location.X;
            mousePos[1] = e.Location.Y;
        }
        private void playButton_Click(object sender, EventArgs e)
        {
            nameBox.Visible = false;
            selectedLabel.Visible = false;
            redButton.Visible = false;
            blueButton.Visible = false;
            greenButton.Visible = false;
            randomButton.Visible = false;
            nameLabel.Visible = false;
            colourLabel.Visible = false;
            startLabel.Visible = false;
            playButton.Visible = false;
            leaveButton.Visible = false;

            switch (selectedColour)
            {
                case 0:
                    player.color = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                    break;
                case 1:
                    player.color = Color.Red;
                    break;
                case 2:
                    player.color = Color.Blue;
                    break;
                case 3:
                    player.color = Color.Green;
                    break;
            }
            playerBrush = new SolidBrush(player.color);

            name = nameBox.Text;

            gameTimer.Enabled = true; //Starts timer loop
            this.Focus();
            player.size = 200; //Sets player's starting mass
            playerBrush = new SolidBrush(player.color); //Sets player colour to the brush that draws the player
            player.x = mapSize / 2; //Places player on X-axis
            player.y = mapSize / 2; //Places player on Y-axis

            generating = false;
        }

        private void leaveButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void redButton_Click(object sender, EventArgs e)
        {
            selectedColour = 1;
            selectedLabel.Text = "Selected: Red";
        }

        private void blueButton_Click(object sender, EventArgs e)
        {
            selectedColour = 2;
            selectedLabel.Text = "Selected: Blue";
        }

        private void greenButton_Click(object sender, EventArgs e)
        {
            selectedColour = 3;
            selectedLabel.Text = "Selected: Green";
        }

        private void randomButton_Click(object sender, EventArgs e)
        {
            selectedColour = 0;
            selectedLabel.Text = "Selected: Random";
        }
    }

    //Every object on screen that can move uses this. (Player, enemies, splits)
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
        public bool onBoard = false;
        public void Split()
        {
            for (int i = Form1.playerObjects.Count - 1; i >= 0; i--)
            {
                if (Form1.playerObjects[i].size > 100)
                {
                    Player p1 = new Player();
                    p1.size = Form1.playerObjects[i].size / 2;
                    p1.x = this.x + Form1.mousePos[0];
                    p1.y = this.y + Form1.mousePos[1];
                    Form1.playerObjects[i].size /= 2;
                    p1.baseSpeed = 18;
                    Form1.playerObjects[i].mergeTimer.Start();
                    p1.mergeTimer.Start();
                    Form1.playerObjects.Add(p1);
                }
            }
                
            
            if (this.size > 100)
            {
                Player p = new Player();
                p.size = this.size / 2;
                p.x = this.x + Form1.mousePos[0] - Convert.ToInt32(p.size / 4);
                p.y = this.y + Form1.mousePos[1] - Convert.ToInt32(p.size / 4);
                this.size /= 2;
                p.baseSpeed = 18;
                this.mergeTimer.Start();
                p.mergeTimer.Start();
                Form1.playerObjects.Add(p);
            }
        }
    }

    //Pretty self-explanatory
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