using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Pong : Microsoft.Xna.Framework.Game
    {
     GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Box selection;
        Box paddle1Box, paddle2Box;
        Box ballBox;

        Texture2D rectangle;
        Texture2D splitLine;
        Texture2D outline;

        SoundEffect paddleBlip;
        SoundEffect selectionBlip;

        private SpriteFont font;
        private int score1 = 0, score2 = 0;

        private int clWidth;
        private int clHeight;

        private const float ballStartSpeed = 4;
        private const float ballStartAngle = 0;

        private bool againstAI;
        private bool gameRunning = false;
        private bool win = false;
        private string result = "";

        public Pong()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 1200;
            graphics.ApplyChanges();

            clWidth = GraphicsDevice.Viewport.Width;
            clHeight = GraphicsDevice.Viewport.Height;

            paddle1Box = new Box(100, 350, 10, 100, 0, 0);
            paddle2Box = new Box(1090, 350, 10, 100, 0, 0);
            ballBox = new Box(550, 400, 8, 8, ballStartSpeed, ballStartAngle);

            selection = new Box(280, 392, 230, 100, 0, 0);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Score");
            outline = Content.Load<Texture2D>("outline");
            rectangle = Content.Load<Texture2D>("paddle");
            splitLine = Content.Load<Texture2D>("split line");
            paddleBlip = Content.Load<SoundEffect>("blip");
            selectionBlip = Content.Load<SoundEffect>("blip2");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (!gameRunning)
            { gameRunning = GameLogic.Start(ref selection, ref againstAI, selectionBlip); }

            if (gameRunning)
            {
                // Here is the actual logic used for the Pong game.

                paddle1Box.MoveBox(10, false, true, Keys.A, Keys.D, Keys.W, Keys.S);
                paddle1Box.BoxMovement(0, 0, clWidth, clHeight, null);
                paddle2Box.BoxMovement(0, 0, clWidth, clHeight, null);

                if (!againstAI)
                { paddle2Box.MoveBox(10, false, true, Keys.Left, Keys.Right, Keys.Up, Keys.Down); }
                else { paddle2Box.aiMoveBox(10, ballBox); }

                ballBox.BoxMovement(-512, 0, clWidth + 512, clHeight, paddleBlip);

                ballBox.Interact(paddle1Box, ballBox.Rect.Width);
                ballBox.Interact(paddle2Box, ballBox.Rect.Width);
                ballBox.IncreaseMove(1F, paddleBlip);

                score1 = GameLogic.Score(score1, clWidth + 256, false, ballBox, selectionBlip);
                score2 = GameLogic.Score(score2, -256, true, ballBox, selectionBlip);

                result = GameLogic.WinLose(score1, score2, ref win, ref gameRunning);
            }

            // Placement allows for Restart at any point in the game.
            GameLogic.Restart(ref score1, ref score2, ref gameRunning, ref win, ref result, ref paddle1Box, ref paddle2Box, ref ballBox, ref selection, selectionBlip); 

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            
            // The first instance is drawing the start menu, the second is the actual game, and the third is the win screen.

            if (gameRunning == false && win == false)
            {
                spriteBatch.Draw(rectangle, graphics.GraphicsDevice.ScissorRectangle, Color.Black);
                spriteBatch.DrawString(font, "PONG", new Vector2(502, 100), Color.White);
                spriteBatch.DrawString(font, "P + P", new Vector2(305, 400), Color.White);
                spriteBatch.DrawString(font, "P + PC", new Vector2(700, 400), Color.White);
                spriteBatch.Draw(outline, selection.Rect, Color.White);
            }

            if (gameRunning == true && win == false)
            {
                spriteBatch.Draw(splitLine, graphics.GraphicsDevice.ScissorRectangle, Color.White);
                spriteBatch.DrawString(font, score1.ToString(), new Vector2(450, 100), Color.White);
                spriteBatch.DrawString(font, score2.ToString(), new Vector2(720, 100), Color.White);
                spriteBatch.Draw(rectangle, paddle1Box.Rect, Color.White);
                spriteBatch.Draw(rectangle, paddle2Box.Rect, Color.White);
                spriteBatch.Draw(rectangle, ballBox.Rect, Color.White);
            }

            if (win == true)
            {
                spriteBatch.Draw(rectangle, graphics.GraphicsDevice.ScissorRectangle, Color.Black);
                spriteBatch.DrawString(font, score1.ToString(), new Vector2(450, 100), Color.White);
                spriteBatch.DrawString(font, score2.ToString(), new Vector2(720, 100), Color.White);
                spriteBatch.DrawString(font, result, new Vector2(330, 400), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
