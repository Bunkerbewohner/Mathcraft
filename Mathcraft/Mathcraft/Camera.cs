using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Mathcraft
{
    abstract class Camera : GameComponent
    {
        public Matrix View
        {
            get { return _view; }
        }

        public Matrix Projection
        {
            get { return _projection; }
        }

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        protected Matrix _view;
        protected Matrix _projection;
        protected Vector3 _position;

        public Camera(Game game)
            : base(game)
        {

        }

        public void Activate()
        {
            Game.Services.RemoveService(typeof(Camera));
            Game.Services.AddService(typeof(Camera), this);
        }

        public override void Initialize()
        {
            float aspectRatio = Game.GraphicsDevice.Viewport.AspectRatio;
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 1000);

            base.Initialize();
        }
    }

    class FirstPersonCamera : Camera
    {
        float moveSpeed = 0.5f;
        float rotationSpeed = 0.01f;
        float horizontalRotation;
        float verticalRotation;
        bool first = true;

        MouseState previousMouseState;
        KeyboardState previousKeyboardState;

        public FirstPersonCamera(Game1 game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
            previousKeyboardState = Keyboard.GetState();
            previousMouseState = Mouse.GetState();

            base.Initialize();
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();

            if (currentMouseState != previousMouseState || first)
            {
                first = false;
                float xDiff = currentMouseState.X - previousMouseState.X;
                float yDiff = currentMouseState.Y - previousMouseState.Y;

                horizontalRotation -= rotationSpeed * xDiff;
                verticalRotation -= rotationSpeed * yDiff;

                Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);

                UpdateViewMatrix();
            }

            if (currentKeyboardState.IsKeyDown(Keys.W))
                AddToCameraPosition(new Vector3(0, 0, -1));
            if (currentKeyboardState.IsKeyDown(Keys.S))
                AddToCameraPosition(new Vector3(0, 0, 1));
            if (currentKeyboardState.IsKeyDown(Keys.A))
                AddToCameraPosition(new Vector3(-1, 0, 0));
            if (currentKeyboardState.IsKeyDown(Keys.D))
                AddToCameraPosition(new Vector3(1, 0, 0));

            if (currentKeyboardState.IsKeyDown(Keys.Q))
                AddToCameraPosition(new Vector3(0, -1, 0));
            if (currentKeyboardState.IsKeyDown(Keys.E))
                AddToCameraPosition(new Vector3(0, 1, 0));

            base.Update(gameTime);
        }

        private void AddToCameraPosition(Vector3 v)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(verticalRotation) *
                Matrix.CreateRotationY(horizontalRotation);
            Vector3 rotatedVector = Vector3.Transform(v, cameraRotation);
            _position += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(verticalRotation) *
                Matrix.CreateRotationY(horizontalRotation);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = _position + cameraRotatedTarget;

            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);
            Vector3 cameraFinalUpVector = _position + cameraRotatedUpVector;

            _view = Matrix.CreateLookAt(_position, cameraFinalTarget, cameraRotatedUpVector);
        }
    }
}
