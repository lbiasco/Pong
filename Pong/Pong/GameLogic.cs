using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Pong
{
    class GameLogic
    {
        /// <summary>
        /// Simple scoring mechanic when the selected box moves past the desired side (by onLeft)
        /// of the given bound.
        /// <summary>
        static public int Score(int score, int bound, bool onLeft, Box box, SoundEffect sound)
        {
            bool scored = false;
            int xReset = 0;

            if (onLeft == true)
            {
                if (box.Rect.Left <= bound)
                { 
                    score++;
                    scored = true;
                    xReset = 550;
                }
            }
            else 
            {
                if (box.Rect.Right >= bound)
                { 
                    score++;
                    scored = true;
                    xReset = 650;
                }
            }

            if (scored == true)
            {
                box.Reset(xReset, 400, 4);
                sound.Play();
            }

            return score;
        }

        /// <summary>
        /// Gives starting screen functionality and also checks for playing against AI.
        /// <summary>
        static public bool Start(ref Box select, ref bool AI, SoundEffect sound)
        {
            bool startGame = false;
            KeyboardState pressedKey = Keyboard.GetState();

            if (pressedKey.IsKeyDown(Keys.Left) || pressedKey.IsKeyDown(Keys.A))
            {
                if (select.Rect.X == 670)
                { sound.Play(); }
                select = new Box(280, 392, 230, 100, 0, 0);
            }
            else if (pressedKey.IsKeyDown(Keys.Right) || pressedKey.IsKeyDown(Keys.D))
            {
                if (select.Rect.X == 280)
                { sound.Play(); }
                select = new Box(670, 392, 290, 100, 0, 0);
            }

            if (pressedKey.IsKeyDown(Keys.Enter))
            {
                sound.Play();
                if (select.Rect.X == 280)
                { 
                    startGame = true;
                    AI = false;
                }
                if (select.Rect.X == 670)
                { 
                    startGame = true;
                    AI = true;
                }
            }

            return startGame;
        }

        /// <summary>
        /// Checks for which player wins by who reaches score = 7 first. (I might make it win by 2 later).
        /// <summary>
        static public string WinLose(int score1, int score2, ref bool win, ref bool gameRunning)
        {
            string whoWon = "";

            if (score1 == 7)
            {
                gameRunning = false;
                win = true;
                whoWon = "Player 1";
            }

            if (score2 == 7)
            {
                gameRunning = false;
                win = true;
                whoWon = "Player 2";
            }

            return whoWon + " wins!";
        }

        /// <summary>
        /// Allows for restart from any point in the game (using Escape) or when someone wins (using Enter).
        /// There is a slight delay (.5 sec) to the method finishing.
        /// <summary>
        static public void Restart(ref int score1, ref int score2, ref bool gameRunning, 
                                    ref bool win, ref string result, ref Box paddle1Box, ref Box paddle2Box, 
                                    ref Box ballBox, ref Box selection, SoundEffect sound)
        {
            KeyboardState pressedKey = Keyboard.GetState();

            if (pressedKey.IsKeyDown(Keys.Enter) && win == true || pressedKey.IsKeyDown(Keys.Escape))
            {
                score1 = 0;
                score2 = 0;

                gameRunning = false;
                win = false;
                result = "";

                paddle1Box = new Box(100, 350, 10, 100, 0, 0);
                paddle2Box = new Box(1090, 350, 10, 100, 0, 0);
                ballBox = new Box(550, 400, 8, 8, 4, 0);

                selection = new Box(280, 392, 230, 100, 0, 0);

                sound.Play();
                Thread.Sleep(500);
            }
        }
    }
}
