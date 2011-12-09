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
        bool show = false;
        bool showNextTime = false;

        Vector2 cursorPos = Vector2.Zero;
        int cursorOffset = 0;

        int CURSOR_START_Y = 20;
        int CURSOR_START_X = 20;
        const float CURSOR_WIDTH = 12;

        const int CURSOR_BLINK_DELAY_MS = 200;
        int cursor_blink = 0;
        bool drawCursor = true;
        bool justOpened = false;

        Dictionary<Keys, int> pressDurations = new Dictionary<Keys, int>();
        StringBuilder inputBuffer = new StringBuilder();
        KeyboardState prevKbd, kbd;

        InputEvaluator eval;
        EvalResult? lastResult = null;

        public bool IsOpen
        {
            get { return show; }
        }

        public TextInputComponent(Game game)
            : base(game)
        {
            cursorPos = new Vector2(CURSOR_START_X, CURSOR_START_Y);
            DrawOrder = 2;
        }

        protected override void LoadContent()
        {
            font = Game.Content.Load<SpriteFont>("MathFont");
            background = Game.Content.Load<Texture2D>("deviantart-bashcorpo-paper");
            cursor = Game.Content.Load<Texture2D>("text-cursor");
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            base.LoadContent();
        }

        public override void Initialize()
        {
            CURSOR_START_Y = 20 + Game.GraphicsDevice.PresentationParameters.BackBufferHeight / 2;

            eval = new InputEvaluator(Game);
            
            base.Initialize();
        }

        public void Show()
        {
            if (show) return;
            showNextTime = true;
            justOpened = true;
        }

        void Close()
        {
            show = false;
            (Game.Services.GetService(typeof(Camera)) as Camera).Enabled = true;
        }

        public override void Draw(GameTime gameTime)
        {
            int width = Game.GraphicsDevice.PresentationParameters.BackBufferWidth;
            int height = Game.GraphicsDevice.PresentationParameters.BackBufferHeight;

            if (show)
            {
                spriteBatch.Begin();

                spriteBatch.Draw(background, new Rectangle(0, height/2, width, height), Color.White);

                String text;
                if (false && drawCursor && cursorOffset >= inputBuffer.Length) text = inputBuffer.ToString() + "_";
                else if (false && drawCursor && cursorOffset > 0 && inputBuffer[cursorOffset] != '\n')
                {
                    char temp = inputBuffer[cursorOffset];
                    inputBuffer[cursorOffset] = '_';
                    text = inputBuffer.ToString();
                    inputBuffer[cursorOffset] = temp;
                }
                else text = inputBuffer.ToString();

                string cursorText = new String((from i in Enumerable.Range(0, text.Length + 1) 
                                 select i == cursorOffset ? '_' : 
                                 (i >= 0 && i < inputBuffer.Length && inputBuffer[i] == '\n' ? '\n' : ' ')).ToArray()); 

                if (drawCursor) 
                    spriteBatch.DrawString(font, cursorText, new Vector2(CURSOR_START_X, CURSOR_START_Y), Color.Blue);

                spriteBatch.DrawString(font, text, new Vector2(CURSOR_START_X, CURSOR_START_Y), Color.Black);

                if (lastResult.HasValue)
                {
                    EvalResult result = lastResult.Value;

                    spriteBatch.DrawString(font, result.Message, new Vector2(19, 19), Color.Black);
                    spriteBatch.DrawString(font, result.Message, new Vector2(20, 20), Color.White);                    
                }

                spriteBatch.End();
            }
            
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            kbd = Keyboard.GetState();

            if (show)
            {
                if (kbd.IsKeyDown(Keys.C) && prevKbd.IsKeyUp(Keys.C) && kbd.IsKeyDown(Keys.LeftControl))
                {
                    Close();
                    return;
                }

                if (cursor_blink < CURSOR_BLINK_DELAY_MS)
                    cursor_blink += gameTime.ElapsedGameTime.Milliseconds;

                if (cursor_blink > CURSOR_BLINK_DELAY_MS)
                {
                    drawCursor = !drawCursor;
                    cursor_blink = 0;
                }

                if (kbd.IsKeyDown(Keys.Left) && prevKbd.IsKeyUp(Keys.Left))
                    cursorOffset = Math.Max(0, cursorOffset - 1);
                else if (kbd.IsKeyDown(Keys.Right) && prevKbd.IsKeyUp(Keys.Right))
                    cursorOffset = Math.Min(inputBuffer.Length, cursorOffset + 1);
                else if (IsPressed(Keys.Up))
                {
                    int started = cursorOffset;
                    while (cursorOffset > inputBuffer.Length - 1) cursorOffset--;
                    while (cursorOffset > 0 && inputBuffer[cursorOffset] != '\n')
                        cursorOffset--;

                    int lineOffset = started - cursorOffset;
                    if (cursorOffset > 0) cursorOffset--;
                    while (cursorOffset > 0 && inputBuffer[cursorOffset] != '\n')
                        cursorOffset--;

                    cursorOffset += lineOffset;
                }
                else if (IsPressed(Keys.Down))
                {
                    int started = cursorOffset;
                    while (cursorOffset > inputBuffer.Length - 1) cursorOffset--;
                    while (cursorOffset > 0 && inputBuffer[cursorOffset] != '\n')
                        cursorOffset--;

                    int lineOffset = started - cursorOffset;

                    cursorOffset = Math.Min(inputBuffer.Length - 1, cursorOffset + lineOffset + 1);
                    while (cursorOffset < inputBuffer.Length - 1 && inputBuffer[cursorOffset] != '\n')
                        cursorOffset++;

                    cursorOffset = Math.Min(cursorOffset + lineOffset, inputBuffer.Length - 1);
                }
                else if (IsPressed(Keys.End))
                {
                    while (cursorOffset < inputBuffer.Length - 1 && inputBuffer[cursorOffset] != '\n')
                        cursorOffset++;
                }
                else if (IsPressed(Keys.Home))
                {
                    while (cursorOffset > inputBuffer.Length - 1) cursorOffset--;
                    while (cursorOffset > 0 && inputBuffer[cursorOffset] != '\n')
                        cursorOffset--;
                }
                else if (IsPressed(Keys.F5))
                {
                    var result = eval.Evaluate(inputBuffer.ToString());
                    if (result.Success) Close();

                    lastResult = result;
                }
                else
                {
                    Keys[] keys = kbd.GetPressedKeys();

                    if (keys.Length > 0 && (int)keys[0] > 0 || keys.Length > 1)
                    {
                        Keys[] skip = new Keys[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.None };
                        Keys key = (from k in keys where !skip.Contains(k) && prevKbd.IsKeyUp(k) select k).FirstOrDefault();

                        bool shift = kbd.IsKeyDown(Keys.LeftShift);
                        bool alt = kbd.IsKeyDown(Keys.RightAlt);

                        if (IsPressed(Keys.Delete))
                        {
                            if (kbd.IsKeyDown(Keys.LeftControl))
                            {
                                cursorOffset = 0;
                                inputBuffer.Clear();
                            }
                            else if (cursorOffset >= 0 && cursorOffset < inputBuffer.Length)
                            {
                                inputBuffer.Remove(cursorOffset, 1);
                            }
                        }
                        else if (IsPressed(Keys.Back))
                        {
                            if (inputBuffer.Length >= 0 && cursorOffset > 0)
                            {
                                inputBuffer.Remove(cursorOffset - 1, 1);
                                cursorOffset = Math.Max(cursorOffset - 1, 0);
                            }
                        }
                        else if (key == Keys.Tab && prevKbd.IsKeyUp(Keys.Tab))
                        {
                            inputBuffer.Append("  ");
                            cursorOffset += 2;
                        }
                        else if (key == Keys.Enter && prevKbd.IsKeyUp(Keys.Enter) && cursorOffset > 0)
                        {
                            inputBuffer.Insert(cursorOffset, '\n');
                            cursorOffset++;
                        }
                        else if (IsPressed(Keys.D) && kbd.IsKeyDown(Keys.LeftControl))
                        {
                            // Delete Complete Line
                            while (cursorOffset > inputBuffer.Length - 1) cursorOffset--;
                            while (cursorOffset > 0 && inputBuffer[cursorOffset] != '\n')
                            {
                                inputBuffer.Remove(cursorOffset, 1);
                                cursorOffset--;
                            }
                        }
                        else if (IsValidChar(key) && IsPressed(key))
                        {
                            char ch = KeyCodeToChar(key, shift, alt);

                            if (cursorOffset >= inputBuffer.Length)
                            {
                                inputBuffer.Append(ch);
                            }
                            else
                            {
                                inputBuffer.Insert(cursorOffset, ch);
                                //inputBuffer[cursorOffset] = ch;
                            }
                            cursorOffset++;
                        }

                        cursorPos.X = CURSOR_START_X + cursorOffset * CURSOR_WIDTH;
                    }
                }
            }
            else if (showNextTime)
            {
                show = true;
                showNextTime = false;
            }

            prevKbd = kbd;
            
            base.Update(gameTime);
        }       

        bool IsPressed(Keys k)
        {
            if (k == Keys.None) return false;
            if (!pressDurations.ContainsKey(k)) pressDurations.Add(k, 0);

            if (kbd.IsKeyDown(k)) pressDurations[k] += 2;

            if (kbd.IsKeyDown(k) && prevKbd.IsKeyUp(k))
            {
                return true;
            }
            else if (pressDurations[k] > 60 && kbd.IsKeyDown(k))
            {
                pressDurations[k] = 0;
                return true;
            }
            else
            {
                return false;
            }
        }
        
        bool IsValidChar(Keys key)
        {
            Keys[] invalid = new Keys[] {
                Keys.Left, Keys.Right, Keys.Up, Keys.Down,
                Keys.Back, Keys.Delete, Keys.BrowserBack, Keys.LeftShift,
                Keys.LeftAlt, Keys.LeftControl, Keys.LeftWindows, Keys.Enter,
                Keys.Escape, Keys.RightAlt
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
            "#abcdefghijklmnopqrstuvwxyzz####" + // 65=A, 90=Z
            "0123456789*+,-./################" +
            "################################" +
            "##########################ü+,-.#" +
            "###########################[^]##" + // 192..223
            "##<#############################";

            const string LOOKUP_SHIFT =
            "################################" + // 0..31 junk
            " ###############=!\"§$%&/()######" + // 32(space), 48..57(0..9)
            "#ABCDEFGHIJKLMNOPQRSTUVWXYZZ####" + // 65=A, 90=Z
            "=!\"§$%&/()**;_:/################" +
            "################################" +
            "##########################Ü*;_:'" +
            "###########################?°]##" + // 192..223
            "##>#############################";

            const string LOOKUP_ALT =
            "################################" + // 0..31 junk
            " ###############}1²³456{[]######" + // 32(space), 48..57(0..9)
            "#abcd€fghijklµnop@rstuvwxyxz####" + // 65=A, 90=Z
            "}1²³456{[]*~,-./################" + // 96..127
            "################################" + // 128..159
            "##########################;~,-./" + // 160..191
            "###########################[\\]##" + // 192..223
            "##|#############################"; // 224..256

            if (alt) return LOOKUP_ALT[(int)kb];
            if (shift) return LOOKUP_SHIFT[(int)kb];
            return LOOKUP[(int)kb];
        }
    }
}
