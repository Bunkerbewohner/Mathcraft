using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Mathcraft
{
    class TextInputComponent : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D background;
        Texture2D cursor;
        bool show = true;

        Vector2 cursorPos = Vector2.Zero;
        int cursorOffset = 0;

        const int CURSOR_START_Y = 20;
        const int CURSOR_START_X = 20;
        const float CURSOR_WIDTH = 12;

        const int CURSOR_BLINK_DELAY_MS = 200;
        int cursor_blink = 0;
        bool drawCursor = true;

        StringBuilder inputBuffer = new StringBuilder();

        KeyboardState prevKbd;

        public TextInputComponent(Game game)
            : base(game)
        {
            cursorPos = new Vector2(CURSOR_START_X, CURSOR_START_Y);
        }

        protected override void LoadContent()
        {
            font = Game.Content.Load<SpriteFont>("MathFont");
            background = Game.Content.Load<Texture2D>("deviantart-bashcorpo-paper");
            cursor = Game.Content.Load<Texture2D>("text-cursor");
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            base.LoadContent();
        }

        public void Show()
        {            
            show = true;
        }

        public override void Draw(GameTime gameTime)
        {
            int width = Game.GraphicsDevice.PresentationParameters.BackBufferWidth;
            int height = Game.GraphicsDevice.PresentationParameters.BackBufferHeight;

            if (show)
            {
                spriteBatch.Begin();

                spriteBatch.Draw(background, new Rectangle(0, 0, width, height), Color.White);

                String text;
                if (drawCursor && cursorOffset >= inputBuffer.Length) text = inputBuffer.ToString() + "_";
                else if (drawCursor && cursorOffset > 0)
                {
                    char temp = inputBuffer[cursorOffset];
                    inputBuffer[cursorOffset] = '_';
                    text = inputBuffer.ToString();
                    inputBuffer[cursorOffset] = temp;
                }
                else text = inputBuffer.ToString();

                spriteBatch.DrawString(font, text, new Vector2(CURSOR_START_X, CURSOR_START_Y), Color.Black);                

                spriteBatch.End();
            }
            
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            var kbd = Keyboard.GetState();

            if (show)
            {
                if (kbd.IsKeyDown(Keys.Escape) && prevKbd.IsKeyUp(Keys.Escape))
                    show = false;

                if (cursor_blink < CURSOR_BLINK_DELAY_MS)
                    cursor_blink += gameTime.ElapsedGameTime.Milliseconds;

                if (cursor_blink > CURSOR_BLINK_DELAY_MS)
                {
                    drawCursor = !drawCursor;
                    cursor_blink = 0;
                }

                if (kbd.IsKeyDown(Keys.Left) && prevKbd.IsKeyUp(Keys.Left))
                    cursorOffset = Math.Max(0, cursorOffset - 1);
                if (kbd.IsKeyDown(Keys.Right) && prevKbd.IsKeyUp(Keys.Right))
                    cursorOffset = Math.Min(inputBuffer.Length, cursorOffset + 1);

                Keys[] keys = kbd.GetPressedKeys();
                if (keys.Length > 0)
                {
                    Keys key = keys[0];                   
                    Keys key2 = keys.Length > 1 ? keys[1] : Keys.BrowserBack;

                    bool shift = key == Keys.LeftShift || key2 == Keys.LeftShift;
                    bool alt = kbd.IsKeyDown(Keys.RightAlt);
                    if (key == Keys.LeftShift) key = key2;
                    if (key == Keys.LeftControl && keys.Length == 3) key = keys[2];

                    if (key == Keys.Back && prevKbd.IsKeyUp(Keys.Back))
                    {
                        inputBuffer.Remove(inputBuffer.Length - 1, 1);
                        cursorOffset--;
                    }
                    else if (IsValidChar(key) && prevKbd.IsKeyUp(key))
                    {
                        char ch = KeyCodeToChar(key, shift, alt);                        

                        if (cursorOffset >= inputBuffer.Length)
                        {
                            inputBuffer.Append(ch);
                        }
                        else
                        {
                            inputBuffer[cursorOffset] = ch;
                        }
                        cursorOffset++;                        
                    }

                    cursorPos.X = CURSOR_START_X + cursorOffset * CURSOR_WIDTH;
                }
            }

            prevKbd = kbd;
            
            base.Update(gameTime);
        }       
        
        bool IsValidChar(Keys key)
        {
            Keys[] invalid = new Keys[] {
                Keys.Left, Keys.Right, Keys.Up, Keys.Down,
                Keys.Back, Keys.Delete, Keys.BrowserBack, Keys.LeftShift,
                Keys.LeftAlt, Keys.LeftControl, Keys.LeftWindows, Keys.Enter
            };

            if (invalid.Contains(key)) return false;

            char c = KeyCodeToChar(key);
            return !Char.IsControl(c);
        }

        public static char KeyCodeToChar(Keys kb, bool shift = false, bool alt = false)
        {
            const string LOOKUP =
            "################################" + // 0..31 junk
            " ###############0123456789######" + // 32(space), 48..57(0..9)
            "#abcdefghijklmnopqrstuvwxyxz####" + // 65=A, 90=Z
            "0123456789*+,-./################" +
            "################################" +
            "##########################;+,-.#" +
            "`##########################[^]‘#" + // 192..223
            "##<#############################";

            const string LOOKUP_SHIFT =
            "################################" + // 0..31 junk
            " ###############=!\"§$%&/()######" + // 32(space), 48..57(0..9)
            "#ABCDEFGHIJKLMNOPQRSTUVWXYXZ####" + // 65=A, 90=Z
            "=!\"§$%&/()**;_:/################" +
            "################################" +
            "##########################;*;_:/" +
            "`##########################[°]‘#" + // 192..223
            "##>#############################";

            const string LOOKUP_ALT =
            "################################" + // 0..31 junk
            " ###############}1²³456{[]######" + // 32(space), 48..57(0..9)
            "#abcd€fghijklµnop@rstuvwxyxz####" + // 65=A, 90=Z
            "}1²³456{[]*~,-./################" + // 96..127
            "################################" + // 128..159
            "##########################;~,-./" + // 160..191
            "`##########################[\\]‘#" + // 192..223
            "##|#############################"; // 224..256

            if (alt) return LOOKUP_ALT[(int)kb];
            if (shift) return LOOKUP_SHIFT[(int)kb];
            return LOOKUP[(int)kb];
        }
    }
}
