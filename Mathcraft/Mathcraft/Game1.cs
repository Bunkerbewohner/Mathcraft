using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Mathcraft
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KeyboardState prevKbd;
        TextInputComponent textInput;

        MathGeometry geom;
        Random rand = new Random();

        public Game1()
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
            textInput = new TextInputComponent(this);
            Components.Add(textInput);

            Camera cam = new FirstPersonCamera(this);
            Components.Add(cam);

            cam.Activate();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            GeometryParameters param = new GeometryParameters(
                new Point3D(-50, -50, -200), new Point3D(100, 100, 100), TestVisibility, TestMaterial);
             geom = new MathGeometry(this, param);
        }

        bool TestVisibility(Point3D p)        
        {
            // Creates a striped hollow sphere hull and a core
            double sphere = Math.Pow(p.X - 10, 2) + Math.Pow(p.Y - 10, 2) + Math.Pow(p.Z - 10, 2);
            return sphere >= 40 && sphere <= 50 && p.Y % 2 == 0 || sphere >= 0 && sphere <= 5 ||
                p.X % 4 == 0 && p.Y % 3 == 0 && p.Z % 6 == 0;
        }

        int TestMaterial(Point3D p)
        {
            // assigns materials to core and hull
            double sphere = Math.Pow(p.X - 10, 2) + Math.Pow(p.Y - 10, 2) + Math.Pow(p.Z - 10, 2);
            if (sphere >= 0 && sphere <= 5) return 23;
            if (sphere >= 40 && sphere <= 50) return 3;
            return rand.Next(255);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
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

            KeyboardState kbd = Keyboard.GetState();

            if (kbd.IsKeyDown(Keys.Enter) && prevKbd.IsKeyUp(Keys.Enter))
                textInput.Show();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            geom.Draw();

            base.Draw(gameTime);
        }
    }
}
