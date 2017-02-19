using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AnimalEvolution
{
    class Camera
    {
        private Matrix transform;
        public Matrix Transform
        {
            get
            {
                return transform;
            }
        }

        private bool hasLastState = false;
        private MouseState lastMouseState;
        private KeyboardState lastKeyboardState;

        public Camera()
        {
            transform = Matrix.CreateScale(16);
        }

        public void Update()
        {

            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            if (!hasLastState)
            {
                lastMouseState = mouseState;
                lastKeyboardState = keyboardState;
                hasLastState = true;
                return;
            }

            if(mouseState.ScrollWheelValue != lastMouseState.ScrollWheelValue)
            {
                transform = transform * Matrix.CreateTranslation(-mouseState.X, -mouseState.Y, 0);
                transform = transform * Matrix.CreateScale((float)Math.Pow(1.1f, (mouseState.ScrollWheelValue- lastMouseState.ScrollWheelValue) /100f));
                transform = transform * Matrix.CreateTranslation(mouseState.X, mouseState.Y, 0);
                Console.WriteLine(mouseState.ScrollWheelValue);
            }
            
            if(lastMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Pressed)
            {
                if(lastMouseState.X != mouseState.X || lastMouseState.Y != mouseState.Y)
                {
                    transform = transform * Matrix.CreateTranslation(mouseState.X - lastMouseState.X, mouseState.Y - lastMouseState.Y, 0);
                }
            }
            
            lastKeyboardState = keyboardState;
            lastMouseState = mouseState;
        }
    }
}
