using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Pong
{
    /// <summary>
    /// The main attributes of the Box class are a Rectangle (x, y, sizeX, sizeY)
    /// and movement Vector2 (move, angle), and there are interactions checks, too.
    /// <summary>
    class Box
    {
        private Rectangle rect;
        private Vector2 move;
        private bool canInteract;
        private bool interacted;

        private double oldBallAngle = 1;
        private double newBallAngle = 1;
        private double paddleY;

        static Random random = new Random();
        static float pi = (float)Math.PI;

        public Box()
        {
            rect = new Rectangle();
            move = new Vector2();
            canInteract = true;
            interacted = false;
        }

        public Box(int sizeX, int sizeY)
        {
            rect = new Rectangle(0, 0, sizeX, sizeY);
            move = new Vector2(0, 0);
            canInteract = true;
            interacted = false;
        }

        public Box(int x, int y, int sizeX, int sizeY, float move, float angle)
        {
            rect = new Rectangle(x, y, sizeX, sizeY);
            this.move = new Vector2(move, angle);
            canInteract = true;
            interacted = false;
        }

        public Rectangle Rect
        {
            get { return rect; }
            private set { rect = value; }
        }

        public Vector2 Move
        {
            get { return move; }
            set { move = value; }
        }

        /// <summary>
        /// Sets this box's values equal to given box's without simply referencing.
        /// <summary>
        public void Equalize(Box box)
        {
            this.rect = box.rect;
            this.move = box.move;
            this.canInteract = box.canInteract;
            this.interacted = box.interacted;
        }

        /// <summary>
        /// Moves this box according to move.X and move.Y values and
        /// checks if box is moving out of range and then redirects.
        /// (Also plays sound effect for change in movement from bounds)
        /// <summary>
        public void BoxMovement(int lowBoundX, int lowBoundY, int highBoundX, int highBoundY, SoundEffect sound)
        {
            double moveX = this.move.X * Math.Cos(this.move.Y);
            double moveY = this.move.X * Math.Sin(this.move.Y);

            this.rect.X += (int)Math.Round(moveX);
            this.rect.Y += (int)Math.Round(moveY);

            if (this.rect.X < lowBoundX)
            {
                this.rect.X = 0;
                this.move.Y = pi - this.move.Y;
            }
            if (this.rect.Y < lowBoundY)
            {
                this.rect.Y = 0;
                this.move.Y = 2 * pi - this.move.Y;
                if (sound != null)
                { sound.Play(); }
            }

            if (this.rect.X + this.rect.Width > highBoundX)
            {
                this.rect.X = highBoundX - this.rect.Width;
                this.move.Y = pi - this.move.Y;
            }
            if (this.rect.Y + this.rect.Height > highBoundY)
            {
                this.rect.Y = highBoundY - this.rect.Height;
                this.move.Y = 2 * pi - this.move.Y;
                if (sound != null)
                { sound.Play(); }
            }

            if (this.move.Y < 0)
            { this.move.Y = 2 * pi + this.move.Y; }

            if (this.move.Y > 2 * pi)
            { this.move.Y = this.move.Y - (2 * pi) * (float)Math.Floor(this.move.Y / (2 * pi)); }
        }

        /// <summary>
        /// Checks defined user input for which direction to move this box. Boolean value
        /// of horizontal or vertical allow for those directional movements.
        /// <summary>
        public void MoveBox(int movement, bool horizontal, bool vertical, Keys keyLeft, Keys keyRight, Keys keyUp, Keys keyDown)
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(keyLeft) && horizontal == true)
            { move.X = movement; move.Y = pi; }
            else if (newState.IsKeyDown(keyRight) && horizontal == true)
            { move.X = movement; move.Y = 0; }
            else if (newState.IsKeyDown(keyUp) && vertical == true)
            { move.X = movement; move.Y = 3 * pi / 2; }
            else if (newState.IsKeyDown(keyDown) && vertical == true)
            { move.X = movement; move.Y = pi / 2; }
            else { move.X = 0; }
        }

        /// <summary>
        /// Calculates the predicted location of the ball at the x-coordinate
        /// of the paddle when the ball's angle changes.  Then it adds or
        /// subtracts due to the new random angle to hit the ball.
        /// <summary>
        public void aiMoveBox(int movement, Box ball)
        {
            oldBallAngle = newBallAngle;
            newBallAngle = ball.move.Y;

            // This checks if the ball's angle has changed.
            // If it has, the AI computes a linear pathway of the ball
            // and finds out where the paddle's y-coordinate should be.
            if (newBallAngle != oldBallAngle)
            {
                float angle = (2F * pi / 3F) + ((2F * pi / 3F) * (float)random.NextDouble());

                double time = (this.rect.X - ball.rect.X - ball.rect.Width) / (int)Math.Round(ball.move.X * Math.Cos(ball.move.Y));
                double predictedBallY = (int)Math.Round(ball.move.X * Math.Sin(ball.move.Y)) * time + ball.rect.Y;

                paddleY = (predictedBallY + (ball.rect.Height / 2) - (this.rect.Height / 2)) - (3 / 2) * (this.rect.Height) * (1 - (angle / pi));
            }

            // This changes the movement direction of the paddle
            // towards where it predicts the ball.
            // NOTE: This causes the vibrating of the AI paddle.
            if (this.rect.Y < paddleY)
            { this.move.Y = pi / 2; }
            if (this.rect.Y > paddleY)
            { this.move.Y = 3 * pi / 2; }

            if (this.rect.Y != paddleY)
            { this.move.X = movement; }
            else { this.move.X = 0; }
        }

        /// <summary>
        /// Checks to see if a moving box will pass a stationary box.  pixelDif is used
        /// to create a new "ghost box" for checking location between moving boxes locations
        /// (recommended pixelDif is box.rect.Width).
        /// <summary>
        public void Interact(Box box, double pixelDif)
        {
            bool intersected = false;

            Box oldBox = new Box();
            oldBox.Equalize(this);

            int numGhostBoxes = (int)(oldBox.move.X / pixelDif);
            Box[] ghostBoxes = new Box[numGhostBoxes];

            int i;
            for (i = 0; i < numGhostBoxes; i++)
            {
                ghostBoxes[i] = new Box();

                if (i == 0)
                { 
                    ghostBoxes[0].rect.X = oldBox.rect.X + (int)(oldBox.move.X * Math.Cos(oldBox.move.Y) / numGhostBoxes);
                    ghostBoxes[0].rect.Y = oldBox.rect.Y + (int)(oldBox.move.X * Math.Sin(oldBox.move.Y) / numGhostBoxes);
                }
                else 
                {
                    ghostBoxes[i].rect.X = ghostBoxes[i - 1].rect.X + (int)(oldBox.move.X * Math.Cos(oldBox.move.Y) / numGhostBoxes);
                    ghostBoxes[i].rect.Y = ghostBoxes[i - 1].rect.Y + (int)(oldBox.move.X * Math.Sin(oldBox.move.Y) / numGhostBoxes);
                }
                
                ghostBoxes[i].rect.Width = oldBox.rect.Width;
                ghostBoxes[i].rect.Height = oldBox.rect.Height;

                if (ghostBoxes[i].rect.Intersects(box.rect))
                { 
                    intersected = true;
                    break;
                }
            }
            if (oldBox.rect.Intersects(box.rect))
            {
                if (intersected == true) {}
                else 
                { 
                    intersected = true;
                    ghostBoxes = new Box[1];
                    i = 0;
                    ghostBoxes[0] = oldBox;
                }
            }

            if (intersected == true)
            {
                this.interacted = true;
                box.interacted = true;

                this.Bounce(box, ghostBoxes[i]);
            }
        }

        public void Bounce(Box box)
        {
            if (this.canInteract && box.canInteract == true)
            {
                if (this.rect.Intersects(box.rect))
                {
                    int xDif1 = (this.rect.Right - box.rect.Left);
                    int xDif2 = (this.rect.Left - box.rect.Right);
                    int yDif1 = (this.rect.Bottom - box.rect.Top);
                    int yDif2 = (this.rect.Top - box.rect.Bottom);

                    if (xDif1 <= this.move.X * Math.Cos(this.move.Y))
                    {
                        this.rect.X = box.rect.X - this.rect.Width;
                        this.move.Y = pi - this.move.Y;
                    }
                    if (xDif2 >= this.move.X * Math.Cos(this.move.Y))
                    {
                        this.rect.X = box.rect.X + box.rect.Width;
                        this.move.Y = pi - this.move.Y;
                    }
                    if (yDif1 <= this.move.X * Math.Sin(this.move.Y))
                    {
                        this.rect.Y = box.rect.Y - this.rect.Height;
                        this.move.Y = 2 * pi - this.move.Y;
                    }
                    if (yDif2 >= this.move.X * Math.Sin(this.move.Y))
                    {
                        this.rect.Y = box.rect.Y + box.rect.Height;
                        this.move.Y = 2 * pi - this.move.Y;
                    }

                    this.interacted = true;
                    box.interacted = true;
                }
            }
        }

        /// <summary>
        /// Used in Interact to change the movement of the ball.  The new direction of the ball
        /// depends on how far from the center of the paddle the ball hits.
        /// <summary>
        public void Bounce(Box box1, Box box2)
        {
            // This is still a work in progress
            // Corners sometimes cause unnatural movement
            // Sometimes the ball will teleport

            Box intersectingBox = new Box();
            intersectingBox.rect = Rectangle.Intersect(box2.rect, box1.rect);

            this.rect.Y = box2.rect.Y;
            if (this.move.Y < (pi / 2) || this.move.Y > (3 * pi / 2))
            { this.rect.X = box1.rect.X - this.rect.Width; }
            else if (this.move.Y > (pi / 2) && this.move.Y < (3 * pi / 2))
            { this.rect.X = box1.rect.X + box1.rect.Width;}

            if (intersectingBox.rect.Width <= intersectingBox.rect.Height)
            {
                float yDif = (this.rect.Y + (this.rect.Height / 2)) - (box1.rect.Y + (box1.rect.Height / 2));

                if (this.move.Y < (pi / 2) || this.move.Y > (3 * pi / 2))
                { this.move.Y = pi - (2 * yDif / box1.rect.Height) * (pi / 3); }
                else if (this.move.Y > (pi / 2) && this.move.Y < (3 * pi / 2))
                { this.move.Y = (2 * yDif / box1.rect.Height) * (pi / 3); }
            }
            if (intersectingBox.rect.Width > intersectingBox.rect.Height)
            {
                this.move.Y = (2 * pi) - this.move.Y;
            }
        }

        /// <summary>
        /// Places the ball at a given (x, y) and gives it a random angle
        /// towards either side depending on its current angle.
        /// <summary>
        public void Reset(int x, int y, float move)
        {
            float angleMultiplier = (float)random.NextDouble();

            this.rect.X = x;
            this.rect.Y = y;
            this.move.X = move;

            if (this.move.Y < (pi / 2) || this.move.Y > (3 * pi / 2))
            {
                this.move.Y = (5 * pi / 3) + ((2F / 3F) * pi * angleMultiplier);
            }
            else if (this.move.Y > (pi / 2) && this.move.Y < (3 * pi / 2))
            {
                this.move.Y = (2 * pi / 3) + ((2F / 3F) * pi * angleMultiplier);
            }
        }

        /// <summary>
        /// Increases the movement of the ball after Bounce (and plays
        /// the sound effect as well).
        /// <summary>
        public void IncreaseMove(float increase, SoundEffect sound)
        {
            if (this.interacted == true)
            {
                this.move.X += increase;
                this.interacted = false;
                sound.Play();
            }
        }
    }
}
