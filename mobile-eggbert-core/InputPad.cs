// WindowsPhoneSpeedyBlupi, Version=1.0.0.5, Culture=neutral, PublicKeyToken=6db12cd62dbec439
// WindowsPhoneSpeedyBlupi.InputPad
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace WindowsPhoneSpeedyBlupi
{
    public class InputPad
    {
        private static readonly int padRadius = 140;

        private readonly Game1 game1;

        private readonly Decor decor;

        private readonly Pixmap pixmap;

        private readonly Sound sound;

        private readonly GameData gameData;

        private readonly List<Def.ButtonGlyph> pressedGlyphs;

        private readonly Accelerometer accelSensor;

        private readonly Slider accelSlider;

        private bool padPressed;

        private bool showCheatMenu;

        private TinyPoint padTouchPos;

        private Def.ButtonGlyph lastButtonDown;

        private Def.ButtonGlyph buttonPressed;

        private int touchOrClickCount;

        private bool accelStarted;

        private bool accelActive;

        private double accelSpeedX;

        private bool accelLastState;

        private bool accelWaitZero;

        private int mission;

        public Def.Phase Phase { get; set; }

        public int SelectedGamer { get; set; }

        public TinyPoint PixmapOrigin { get; set; }

        public int TotalTouchOrClick
        {
            get
            {
                return touchOrClickCount;
            }
        }

        public Def.ButtonGlyph ButtonPressed
        {
            get
            {
                Def.ButtonGlyph result = buttonPressed;
                buttonPressed = Def.ButtonGlyph.None;
                return result;
            }
        }

        public bool ShowCheatMenu
        {
            get
            {
                return showCheatMenu;
            }
            set
            {
                showCheatMenu = value;
            }
        }

        private IEnumerable<Def.ButtonGlyph> ButtonGlyphs
        {
            get
            {
                switch (Phase)
                {
                    case Def.Phase.Init:
                        yield return Def.ButtonGlyph.InitGamerA;
                        yield return Def.ButtonGlyph.InitGamerB;
                        yield return Def.ButtonGlyph.InitGamerC;
                        yield return Def.ButtonGlyph.InitSetup;
                        yield return Def.ButtonGlyph.InitPlay;
                        if (game1.IsTrialMode)
                        {
                            yield return Def.ButtonGlyph.InitBuy;
                        }
                        if (game1.IsRankingMode)
                        {
                            yield return Def.ButtonGlyph.InitRanking;
                        }
                        break;
                    case Def.Phase.Play:
                        yield return Def.ButtonGlyph.PlayPause;
                        yield return Def.ButtonGlyph.PlayAction;
                        yield return Def.ButtonGlyph.PlayJump;
                        if (accelStarted)
                        {
                            yield return Def.ButtonGlyph.PlayDown;
                        }
                        yield return Def.ButtonGlyph.Cheat11;
                        yield return Def.ButtonGlyph.Cheat12;
                        yield return Def.ButtonGlyph.Cheat21;
                        yield return Def.ButtonGlyph.Cheat22;
                        yield return Def.ButtonGlyph.Cheat31;
                        yield return Def.ButtonGlyph.Cheat32;
                        break;
                    case Def.Phase.Pause:
                        yield return Def.ButtonGlyph.PauseMenu;
                        if (mission != 1)
                        {
                            yield return Def.ButtonGlyph.PauseBack;
                        }
                        yield return Def.ButtonGlyph.PauseSetup;
                        if (mission != 1 && mission % 10 != 0)
                        {
                            yield return Def.ButtonGlyph.PauseRestart;
                        }
                        yield return Def.ButtonGlyph.PauseContinue;
                        break;
                    case Def.Phase.Resume:
                        yield return Def.ButtonGlyph.ResumeMenu;
                        yield return Def.ButtonGlyph.ResumeContinue;
                        break;
                    case Def.Phase.Lost:
                    case Def.Phase.Win:
                        yield return Def.ButtonGlyph.WinLostReturn;
                        break;
                    case Def.Phase.Trial:
                        yield return Def.ButtonGlyph.TrialBuy;
                        yield return Def.ButtonGlyph.TrialCancel;
                        break;
                    case Def.Phase.MainSetup:
                        yield return Def.ButtonGlyph.SetupSounds;
                        yield return Def.ButtonGlyph.SetupJump;
                        yield return Def.ButtonGlyph.SetupZoom;
                        yield return Def.ButtonGlyph.SetupAccel;
                        yield return Def.ButtonGlyph.SetupReset;
                        yield return Def.ButtonGlyph.SetupReturn;
                        break;
                    case Def.Phase.PlaySetup:
                        yield return Def.ButtonGlyph.SetupSounds;
                        yield return Def.ButtonGlyph.SetupJump;
                        yield return Def.ButtonGlyph.SetupZoom;
                        yield return Def.ButtonGlyph.SetupAccel;
                        yield return Def.ButtonGlyph.SetupReturn;
                        break;
                    case Def.Phase.Ranking:
                        yield return Def.ButtonGlyph.RankingContinue;
                        break;
                }
                if (showCheatMenu)
                {
                    yield return Def.ButtonGlyph.Cheat1;
                    yield return Def.ButtonGlyph.Cheat2;
                    yield return Def.ButtonGlyph.Cheat3;
                    yield return Def.ButtonGlyph.Cheat4;
                    yield return Def.ButtonGlyph.Cheat5;
                    yield return Def.ButtonGlyph.Cheat6;
                    yield return Def.ButtonGlyph.Cheat7;
                    yield return Def.ButtonGlyph.Cheat8;
                    yield return Def.ButtonGlyph.Cheat9;
                }
            }
        }
        /// <summary>
        /// Returns the point of the center of the pad on the screen.
        /// </summary>
        private TinyPoint PadCenter
        {
            get
            {
                TinyRect drawBounds = pixmap.DrawBounds;
                int x = gameData.JumpRight ? 100 : drawBounds.Width - 100;
                return new TinyPoint(x, drawBounds.Height - 100);
            }
        }

        public InputPad(Game1 game1, Decor decor, Pixmap pixmap, Sound sound, GameData gameData)
        {
            //IL_0037: Unknown result type (might be due to invalid IL or missing references)
            //IL_0041: Expected O, but got Unknown
            this.game1 = game1;
            this.decor = decor;
            this.pixmap = pixmap;
            this.sound = sound;
            this.gameData = gameData;
            pressedGlyphs = new List<Def.ButtonGlyph>();
            accelSensor = new Accelerometer();
            ((SensorBase<AccelerometerReading>)(object)accelSensor).CurrentValueChanged += HandleAccelSensorCurrentValueChanged;
            accelSlider = new Slider(new TinyPoint(320, 400), this.gameData.AccelSensitivity);
            lastButtonDown = Def.ButtonGlyph.None;
            buttonPressed = Def.ButtonGlyph.None;
        }

        public void StartMission(int mission)
        {
            this.mission = mission;
            accelWaitZero = true;
        }

        public void Update()
        {
            pressedGlyphs.Clear();
            if (accelActive != gameData.AccelActive)
            {
                accelActive = gameData.AccelActive;
                if (accelActive)
                {
                    StartAccel();
                }
                else
                {
                    StopAccel();
                }
            }
            double horizontalChange = 0.0;
            double verticalChange = 0.0;
            int keyPress = 0;
            padPressed = false;
            Def.ButtonGlyph buttonGlyph = Def.ButtonGlyph.None;

            TouchCollection touches = default;
            if (Env.IMPL.isNotKNI())
            {
                touches = TouchPanel.GetState();
                touchOrClickCount = touches.Count;
            }

            List <TinyPoint> touchesOrClicks = new List<TinyPoint>();


            if(Env.IMPL.isNotKNI()) foreach (TouchLocation item in touches)
            {
                if (item.State == TouchLocationState.Pressed || item.State == TouchLocationState.Moved)
                {
                    TinyPoint touchPress = new TinyPoint((int)item.Position.X, (int)item.Position.Y);
                    touchesOrClicks.Add(touchPress);
                }
            }


            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                touchOrClickCount++;
                TinyPoint mouseClick = new TinyPoint(mouseState.X, mouseState.Y);
                touchesOrClicks.Add(mouseClick);
            }
            
            float screenWidth = game1.getGraphics().GraphicsDevice.Viewport.Width;
            float screenHeight = game1.getGraphics().GraphicsDevice.Viewport.Height;
            float screenRatio = screenWidth / screenHeight;

            if ((Env.PLATFORM.isAndroid() && screenRatio > 1.3333333333333333) || (Env.IMPL.isKNI()))
            {
                for (int i = 0; i < touchesOrClicks.Count; i++)
                {

                    var touchOrClick = touchesOrClicks[i];
                    if (touchOrClick.X == -1) continue;

                    float originalX = touchOrClick.X;
                    float originalY = touchOrClick.Y;

                    float widthHeightRatio = screenWidth / screenHeight;
                    float heightRatio = 480 / screenHeight;
                    float widthRatio = 640 / screenWidth;
                    
                    {
                    DDebug.WriteLine("-----");
                    DDebug.WriteLine("originalX=" + originalX);
                    DDebug.WriteLine("originalY=" + originalY);
                    DDebug.WriteLine("heightRatio=" + heightRatio);
                    DDebug.WriteLine("widthRatio=" + widthRatio);
                    DDebug.WriteLine("widthHeightRatio=" + widthHeightRatio);
                    }
                    if (screenHeight> 480) {
                    touchOrClick.X = (int)(originalX * heightRatio);
                    touchOrClick.Y = (int)(originalY * heightRatio);
                    touchesOrClicks[i] = touchOrClick;
                    }

                    DDebug.WriteLine("new X" + touchOrClick.X);
                    DDebug.WriteLine("new Y" + touchOrClick.Y);
                }
            }

            KeyboardState newKeyboardState = Keyboard.GetState();
            Keys[] keysToBeChecked = new Keys[7] { Keys.LeftControl, Keys.Up, Keys.Right, Keys.Down, Keys.Left, Keys.Space, Keys.Escape,};
            foreach(Keys keys in keysToBeChecked) {
                if (newKeyboardState.IsKeyDown(keys)) touchesOrClicks.Add(new TinyPoint(-1, (int)keys));
            }
            if (newKeyboardState.IsKeyDown(Keys.F11))
            {
                game1.ToggleFullScreen ();
                DDebug.WriteLine("F11 was pressed.");
            }

            Boolean keyPressedUp = false;
            Boolean keyPressedDown = false;
            Boolean keyPressedLeft = false;
            Boolean keyPressedRight = false;
            foreach (TinyPoint touchOrClickItem in touchesOrClicks)
            {
                Boolean keyboardPressed = false;
                if (touchOrClickItem.X == -1)
                {
                    keyboardPressed = true;
                }
                Keys keyPressed = keyboardPressed ? (Keys)touchOrClickItem.Y : Keys.None;
                keyPressedUp = keyPressed == Keys.Up ? true : keyPressedUp;
                keyPressedDown = keyPressed == Keys.Down ? true : keyPressedDown;
                keyPressedLeft = keyPressed == Keys.Left ? true : keyPressedLeft;
                keyPressedRight = keyPressed == Keys.Right ? true : keyPressedRight;

                {
                    TinyPoint touchOrClick = keyboardPressed ? new TinyPoint(1, 1) : touchOrClickItem;
                    if (!accelStarted && Misc.IsInside(GetPadBounds(PadCenter, padRadius), touchOrClick))
                    {
                        padPressed = true;
                        padTouchPos = touchOrClick;
                    }
                    if (keyPressedUp || keyPressedDown || keyPressedLeft || keyPressedRight)
                    {
                        padPressed = true;
                    }
                    DDebug.WriteLine("padPressed=" + padPressed);
                    Def.ButtonGlyph pressedGlyph = ButtonDetect(touchOrClick);
                    DDebug.WriteLine("buttonGlyph2 =" + pressedGlyph);
                    if (pressedGlyph != 0)
                    {
                        pressedGlyphs.Add(pressedGlyph);
                    }
                    if (keyboardPressed)
                    {
                        switch (keyPressed)
                        {
                            case Keys.LeftControl: pressedGlyph = Def.ButtonGlyph.PlayJump; pressedGlyphs.Add(pressedGlyph); break;
                            case Keys.Space: pressedGlyph = Def.ButtonGlyph.PlayAction; pressedGlyphs.Add(pressedGlyph); break;
                            case Keys.Escape: pressedGlyph = Def.ButtonGlyph.PlayPause; pressedGlyphs.Add(pressedGlyph); break;
                        }
                    }

                    if ((Phase == Def.Phase.MainSetup || Phase == Def.Phase.PlaySetup) && accelSlider.Move(touchOrClick))
                    {
                        gameData.AccelSensitivity = accelSlider.Value;
                    }
                    switch (pressedGlyph)
                    {
                        case Def.ButtonGlyph.PlayJump:
                            DDebug.WriteLine("Jumping detected");
                            accelWaitZero = false;
                            keyPress |= 1;
                            break;
                        case Def.ButtonGlyph.PlayDown:
                            accelWaitZero = false;
                            keyPress |= 4;
                            break;
                        case Def.ButtonGlyph.InitGamerA:
                        case Def.ButtonGlyph.InitGamerB:
                        case Def.ButtonGlyph.InitGamerC:
                        case Def.ButtonGlyph.InitSetup:
                        case Def.ButtonGlyph.InitPlay:
                        case Def.ButtonGlyph.InitBuy:
                        case Def.ButtonGlyph.InitRanking:
                        case Def.ButtonGlyph.WinLostReturn:
                        case Def.ButtonGlyph.TrialBuy:
                        case Def.ButtonGlyph.TrialCancel:
                        case Def.ButtonGlyph.SetupSounds:
                        case Def.ButtonGlyph.SetupJump:
                        case Def.ButtonGlyph.SetupZoom:
                        case Def.ButtonGlyph.SetupAccel:
                        case Def.ButtonGlyph.SetupReset:
                        case Def.ButtonGlyph.SetupReturn:
                        case Def.ButtonGlyph.PauseMenu:
                        case Def.ButtonGlyph.PauseBack:
                        case Def.ButtonGlyph.PauseSetup:
                        case Def.ButtonGlyph.PauseRestart:
                        case Def.ButtonGlyph.PauseContinue:
                        case Def.ButtonGlyph.PlayPause:
                        case Def.ButtonGlyph.PlayAction:
                        case Def.ButtonGlyph.ResumeMenu:
                        case Def.ButtonGlyph.ResumeContinue:
                        case Def.ButtonGlyph.RankingContinue:
                        case Def.ButtonGlyph.Cheat11:
                        case Def.ButtonGlyph.Cheat12:
                        case Def.ButtonGlyph.Cheat21:
                        case Def.ButtonGlyph.Cheat22:
                        case Def.ButtonGlyph.Cheat31:
                        case Def.ButtonGlyph.Cheat32:
                        case Def.ButtonGlyph.Cheat1:
                        case Def.ButtonGlyph.Cheat2:
                        case Def.ButtonGlyph.Cheat3:
                        case Def.ButtonGlyph.Cheat4:
                        case Def.ButtonGlyph.Cheat5:
                        case Def.ButtonGlyph.Cheat6:
                        case Def.ButtonGlyph.Cheat7:
                        case Def.ButtonGlyph.Cheat8:
                        case Def.ButtonGlyph.Cheat9:
                            accelWaitZero = false;
                            buttonGlyph = pressedGlyph;
                            showCheatMenu = false;
                            break;
                    }
                }
            }
            if (buttonGlyph != 0 && buttonGlyph != Def.ButtonGlyph.PlayAction && buttonGlyph != Def.ButtonGlyph.Cheat11 && buttonGlyph != Def.ButtonGlyph.Cheat12 && buttonGlyph != Def.ButtonGlyph.Cheat21 && buttonGlyph != Def.ButtonGlyph.Cheat22 && buttonGlyph != Def.ButtonGlyph.Cheat31 && buttonGlyph != Def.ButtonGlyph.Cheat32 && lastButtonDown == Def.ButtonGlyph.None)
            {
                TinyPoint pos = new TinyPoint(320, 240);
                sound.PlayImage(0, pos);
            }
            if (buttonGlyph == Def.ButtonGlyph.None && lastButtonDown != 0)
            {
                buttonPressed = lastButtonDown;
            }
            lastButtonDown = buttonGlyph;
            if (padPressed)
            {
                DDebug.WriteLine("PadCenter.X=" + PadCenter.X);
                DDebug.WriteLine("PadCenter.Y=" + PadCenter.Y);
                DDebug.WriteLine("padTouchPos.X=" + padTouchPos.X);
                DDebug.WriteLine("padTouchPos.Y=" + padTouchPos.Y);
                DDebug.WriteLine("keyPressedUp=" + keyPressedUp);
                DDebug.WriteLine("keyPressedDown=" + keyPressedDown);
                DDebug.WriteLine("keyPressedLeft=" + keyPressedLeft);
                DDebug.WriteLine("keyPressedRight=" + keyPressedRight);
                {
                    if (keyPressedUp)
                    {
                        padTouchPos.Y = PadCenter.Y - 30;
                        padTouchPos.X = PadCenter.X;
                        if (keyPressedLeft) padTouchPos.X = PadCenter.X - 30;
                        if (keyPressedRight) padTouchPos.X = PadCenter.X + 30;
                    }
                    if (keyPressedDown) { 
                        padTouchPos.Y = PadCenter.Y + 30;
                        padTouchPos.X = PadCenter.X;
                        if (keyPressedLeft) padTouchPos.X = PadCenter.X - 30;
                        if (keyPressedRight) padTouchPos.X = PadCenter.X + 30;
                    }
                    if (keyPressedLeft) { 
                        padTouchPos.X = PadCenter.X - 30;
                        padTouchPos.Y = PadCenter.Y;
                        if (keyPressedUp) padTouchPos.Y = PadCenter.Y - 30;
                        if (keyPressedDown) padTouchPos.Y = PadCenter.Y + 30;
                    }
                    if (keyPressedRight) { 
                        padTouchPos.X = PadCenter.X + 30;
                        padTouchPos.Y = PadCenter.Y;
                        if (keyPressedUp) padTouchPos.Y = PadCenter.Y - 30;
                        if (keyPressedDown) padTouchPos.Y = PadCenter.Y + 30;
                    }
                }
                double horizontalPosition = padTouchPos.X - PadCenter.X;
                double verticalPosition = padTouchPos.Y - PadCenter.Y;

                if (horizontalPosition > 20.0)
                {
                    horizontalChange += 1.0;
                    Debug.WriteLine(" horizontalChange += 1.0;");
                }
                if (horizontalPosition < -20.0)
                {
                    horizontalChange -= 1.0;
                    Debug.WriteLine(" horizontalChange -= 1.0;");

                }
                if (verticalPosition > 20.0)
                {
                    verticalChange += 1.0;
                    Debug.WriteLine(" verticalPosition += 1.0;");

                }
                if (verticalPosition < -20.0)
                {
                    verticalChange -= 1.0;
                    Debug.WriteLine(" verticalPosition -= 1.0;");
                }

            }
            if (accelStarted)
            {
                horizontalChange = accelSpeedX;
                verticalChange = 0.0;
                if (((uint)keyPress & 4u) != 0)
                {
                    verticalChange = 1.0;
                }
            }
            decor.SetSpeedX(horizontalChange);
            decor.SetSpeedY(verticalChange);
            decor.KeyChange(keyPress);
        }

        private Def.ButtonGlyph ButtonDetect(TinyPoint touchOrClick)
        {
            foreach (Def.ButtonGlyph buttonGlyph in ButtonGlyphs.Reverse())
            {
                TinyRect buttonRect = GetButtonRect(buttonGlyph);
                
                if (buttonGlyph == Def.ButtonGlyph.PlayJump || buttonGlyph == Def.ButtonGlyph.PlayAction || buttonGlyph == Def.ButtonGlyph.PlayDown || buttonGlyph == Def.ButtonGlyph.PlayPause)
                {
                    buttonRect = Misc.Inflate(buttonRect, 20);
                }
                
                if (Misc.IsInside(buttonRect, touchOrClick))
                {
                    return buttonGlyph;
                }
            }
            return Def.ButtonGlyph.None;
        }

        public void Draw()
        {
            if (!accelStarted && Phase == Def.Phase.Play)
            {
                pixmap.DrawIcon(14, 0, GetPadBounds(PadCenter, padRadius / 2), 1.0, false);
                TinyPoint center = (padPressed ? padTouchPos : PadCenter);
                pixmap.DrawIcon(14, 1, GetPadBounds(center, padRadius / 2), 1.0, false);
            }
            foreach (Def.ButtonGlyph buttonGlyph in ButtonGlyphs)
            {
                bool pressed = pressedGlyphs.Contains(buttonGlyph);
                bool selected = false;
                if (buttonGlyph >= Def.ButtonGlyph.InitGamerA && buttonGlyph <= Def.ButtonGlyph.InitGamerC)
                {
                    int selectedGamer = (int)(buttonGlyph - 1);
                    selected = selectedGamer == gameData.SelectedGamer;
                }
                if (buttonGlyph == Def.ButtonGlyph.SetupSounds)
                {
                    selected = gameData.Sounds;
                }
                if (buttonGlyph == Def.ButtonGlyph.SetupJump)
                {
                    selected = gameData.JumpRight;
                }
                if (buttonGlyph == Def.ButtonGlyph.SetupZoom)
                {
                    selected = gameData.AutoZoom;
                }
                if (buttonGlyph == Def.ButtonGlyph.SetupAccel)
                {
                    selected = gameData.AccelActive;
                }
                pixmap.DrawInputButton(GetButtonRect(buttonGlyph), buttonGlyph, pressed, selected);
            }
            if ((Phase == Def.Phase.MainSetup || Phase == Def.Phase.PlaySetup) && gameData.AccelActive)
            {
                accelSlider.Draw(pixmap);
            }
        }

        private TinyRect GetPadBounds(TinyPoint center, int radius)
        {
            return new TinyRect(center.X - radius, center.X + radius, center.Y - radius, center.Y + radius);
        }

        public TinyRect GetButtonRect(Def.ButtonGlyph glyph)
        {
            TinyRect drawBounds = pixmap.DrawBounds;
            double drawBoundsWidth = drawBounds.Width;
            double drawBoundsHeight = drawBounds.Height;
            double buttonSizeFactor1 = drawBoundsHeight / 5.0;
            double buttonSizeFactor2 = drawBoundsHeight * 140.0 / 480.0;
            double cheatButtonSizeFactor = drawBoundsHeight / 3.5;
            if (glyph >= Def.ButtonGlyph.Cheat1 && glyph <= Def.ButtonGlyph.Cheat9)
            {
                int cheatNumber = (int)(glyph - 35);
                TinyRect result = default(TinyRect);
                result.Left = 80 * cheatNumber;
                result.Right = 80 * (cheatNumber + 1);
                result.Top = 0;
                result.Bottom = 80;
                return result;
            }
            int leftXForButtonsInLeftColumn = (int)(20.0 + buttonSizeFactor2 * 0.0);
            int rightXForButtonsInLeftColumn = (int)(20.0 + buttonSizeFactor2 * 0.5);
            switch (glyph)
            {
                case Def.ButtonGlyph.InitGamerA:
                    {
                        TinyRect result19 = default(TinyRect);
                        result19.Left = leftXForButtonsInLeftColumn;
                        result19.Right = rightXForButtonsInLeftColumn;
                        result19.Top = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 2.1);
                        result19.Bottom = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 1.6);
                        return result19;
                    }
                case Def.ButtonGlyph.InitGamerB:
                    {
                        TinyRect result18 = default(TinyRect);
                        result18.Left = leftXForButtonsInLeftColumn;
                        result18.Right = rightXForButtonsInLeftColumn;
                        result18.Top = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 1.6);
                        result18.Bottom = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 1.1);
                        return result18;
                    }
                case Def.ButtonGlyph.InitGamerC:
                    {
                        TinyRect result15 = default(TinyRect);
                        result15.Left = leftXForButtonsInLeftColumn;
                        result15.Right = rightXForButtonsInLeftColumn;
                        result15.Top = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 1.1);
                        result15.Bottom = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 0.6);
                        return result15;
                    }
                case Def.ButtonGlyph.InitSetup:
                    {
                        TinyRect result14 = default(TinyRect);
                        result14.Left = leftXForButtonsInLeftColumn;
                        result14.Right = rightXForButtonsInLeftColumn;
                        result14.Top = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 0.5);
                        result14.Bottom = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 0.0);
                        return result14;
                    }
                case Def.ButtonGlyph.InitPlay:
                    {
                        TinyRect result11 = default(TinyRect);
                        result11.Left = (int)(drawBoundsWidth - 20.0 - buttonSizeFactor2 * 1.0);
                        result11.Right = (int)(drawBoundsWidth - 20.0 - buttonSizeFactor2 * 0.0);
                        result11.Top = (int)(drawBoundsHeight - 40.0 - buttonSizeFactor2 * 1.0);
                        result11.Bottom = (int)(drawBoundsHeight - 40.0 - buttonSizeFactor2 * 0.0);
                        return result11;
                    }
                case Def.ButtonGlyph.InitBuy:
                case Def.ButtonGlyph.InitRanking:
                    {
                        TinyRect result10 = default(TinyRect);
                        result10.Left = (int)(drawBoundsWidth - 20.0 - buttonSizeFactor2 * 0.75);
                        result10.Right = (int)(drawBoundsWidth - 20.0 - buttonSizeFactor2 * 0.25);
                        result10.Top = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 2.1);
                        result10.Bottom = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 1.6);
                        return result10;
                    }
                case Def.ButtonGlyph.PauseMenu:
                    {
                        TinyRect result37 = default(TinyRect);
                        result37.Left = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * -0.21);
                        result37.Right = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 0.79);
                        result37.Top = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 2.2);
                        result37.Bottom = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 3.2);
                        return result37;
                    }
                case Def.ButtonGlyph.PauseBack:
                    {
                        TinyRect result36 = default(TinyRect);
                        result36.Left = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 0.79);
                        result36.Right = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 1.79);
                        result36.Top = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 2.2);
                        result36.Bottom = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 3.2);
                        return result36;
                    }
                case Def.ButtonGlyph.PauseSetup:
                    {
                        TinyRect result35 = default(TinyRect);
                        result35.Left = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 1.79);
                        result35.Right = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 2.79);
                        result35.Top = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 2.2);
                        result35.Bottom = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 3.2);
                        return result35;
                    }
                case Def.ButtonGlyph.PauseRestart:
                    {
                        TinyRect result34 = default(TinyRect);
                        result34.Left = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 2.79);
                        result34.Right = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 3.79);
                        result34.Top = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 2.2);
                        result34.Bottom = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 3.2);
                        return result34;
                    }
                case Def.ButtonGlyph.PauseContinue:
                    {
                        TinyRect result33 = default(TinyRect);
                        result33.Left = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 3.79);
                        result33.Right = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 4.79);
                        result33.Top = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 2.2);
                        result33.Bottom = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 3.2);
                        return result33;
                    }
                case Def.ButtonGlyph.ResumeMenu:
                    {
                        TinyRect result32 = default(TinyRect);
                        result32.Left = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 1.29);
                        result32.Right = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 2.29);
                        result32.Top = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 2.2);
                        result32.Bottom = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 3.2);
                        return result32;
                    }
                case Def.ButtonGlyph.ResumeContinue:
                    {
                        TinyRect result31 = default(TinyRect);
                        result31.Left = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 2.29);
                        result31.Right = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 3.29);
                        result31.Top = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 2.2);
                        result31.Bottom = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 3.2);
                        return result31;
                    }
                case Def.ButtonGlyph.WinLostReturn:
                    {
                        TinyRect result30 = default(TinyRect);
                        result30.Left = (int)((double)PixmapOrigin.X + drawBoundsWidth - buttonSizeFactor1 * 2.2);
                        result30.Right = (int)((double)PixmapOrigin.X + drawBoundsWidth - buttonSizeFactor1 * 1.2);
                        result30.Top = (int)((double)PixmapOrigin.Y + buttonSizeFactor1 * 0.2);
                        result30.Bottom = (int)((double)PixmapOrigin.Y + buttonSizeFactor1 * 1.2);
                        return result30;
                    }
                case Def.ButtonGlyph.TrialBuy:
                    {
                        TinyRect result29 = default(TinyRect);
                        result29.Left = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 2.5);
                        result29.Right = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 3.5);
                        result29.Top = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 2.1);
                        result29.Bottom = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 3.1);
                        return result29;
                    }
                case Def.ButtonGlyph.TrialCancel:
                    {
                        TinyRect result28 = default(TinyRect);
                        result28.Left = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 3.5);
                        result28.Right = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 4.5);
                        result28.Top = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 2.1);
                        result28.Bottom = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 3.1);
                        return result28;
                    }
                case Def.ButtonGlyph.RankingContinue:
                    {
                        TinyRect result27 = default(TinyRect);
                        result27.Left = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 3.5);
                        result27.Right = (int)((double)PixmapOrigin.X + buttonSizeFactor2 * 4.5);
                        result27.Top = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 2.1);
                        result27.Bottom = (int)((double)PixmapOrigin.Y + buttonSizeFactor2 * 3.1);
                        return result27;
                    }
                case Def.ButtonGlyph.SetupSounds:
                    {
                        TinyRect result26 = default(TinyRect);
                        result26.Left = leftXForButtonsInLeftColumn;
                        result26.Right = rightXForButtonsInLeftColumn;
                        result26.Top = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 2.0);
                        result26.Bottom = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 1.5);
                        return result26;
                    }
                case Def.ButtonGlyph.SetupJump:
                    {
                        TinyRect result25 = default(TinyRect);
                        result25.Left = leftXForButtonsInLeftColumn;
                        result25.Right = rightXForButtonsInLeftColumn;
                        result25.Top = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 1.5);
                        result25.Bottom = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 1.0);
                        return result25;
                    }
                case Def.ButtonGlyph.SetupZoom:
                    {
                        TinyRect result24 = default(TinyRect);
                        result24.Left = leftXForButtonsInLeftColumn;
                        result24.Right = rightXForButtonsInLeftColumn;
                        result24.Top = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 1.0);
                        result24.Bottom = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 0.5);
                        return result24;
                    }
                case Def.ButtonGlyph.SetupAccel:
                    {
                        TinyRect result23 = default(TinyRect);
                        result23.Left = leftXForButtonsInLeftColumn;
                        result23.Right = rightXForButtonsInLeftColumn;
                        result23.Top = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 0.5);
                        result23.Bottom = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 0.0);
                        return result23;
                    }
                case Def.ButtonGlyph.SetupReset:
                    {
                        TinyRect result22 = default(TinyRect);
                        result22.Left = (int)(450.0 + buttonSizeFactor2 * 0.0);
                        result22.Right = (int)(450.0 + buttonSizeFactor2 * 0.5);
                        result22.Top = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 2.0);
                        result22.Bottom = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 1.5);
                        return result22;
                    }
                case Def.ButtonGlyph.SetupReturn:
                    {
                        TinyRect result21 = default(TinyRect);
                        result21.Left = (int)(drawBoundsWidth - 20.0 - buttonSizeFactor2 * 0.8);
                        result21.Right = (int)(drawBoundsWidth - 20.0 - buttonSizeFactor2 * 0.0);
                        result21.Top = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 0.8);
                        result21.Bottom = (int)(drawBoundsHeight - 20.0 - buttonSizeFactor2 * 0.0);
                        return result21;
                    }
                case Def.ButtonGlyph.PlayPause:
                    {
                        TinyRect result20 = default(TinyRect);
                        result20.Left = (int)(drawBoundsWidth - buttonSizeFactor1 * 0.7);
                        result20.Right = (int)(drawBoundsWidth - buttonSizeFactor1 * 0.2);
                        result20.Top = (int)(buttonSizeFactor1 * 0.2);
                        result20.Bottom = (int)(buttonSizeFactor1 * 0.7);
                        return result20;
                    }
                case Def.ButtonGlyph.PlayAction:
                    {
                        if (gameData.JumpRight)
                        {
                            TinyRect result16 = default(TinyRect);
                            result16.Left = (int)((double)drawBounds.Width - buttonSizeFactor1 * 1.2);
                            result16.Right = (int)((double)drawBounds.Width - buttonSizeFactor1 * 0.2);
                            result16.Top = (int)(drawBoundsHeight - buttonSizeFactor1 * 2.6);
                            result16.Bottom = (int)(drawBoundsHeight - buttonSizeFactor1 * 1.6);
                            return result16;
                        }
                        TinyRect result17 = default(TinyRect);
                        result17.Left = (int)(buttonSizeFactor1 * 0.2);
                        result17.Right = (int)(buttonSizeFactor1 * 1.2);
                        result17.Top = (int)(drawBoundsHeight - buttonSizeFactor1 * 2.6);
                        result17.Bottom = (int)(drawBoundsHeight - buttonSizeFactor1 * 1.6);
                        return result17;
                    }
                case Def.ButtonGlyph.PlayJump:
                    {
                        if (gameData.JumpRight)
                        {
                            TinyRect result12 = default(TinyRect);
                            result12.Left = (int)((double)drawBounds.Width - buttonSizeFactor1 * 1.2);
                            result12.Right = (int)((double)drawBounds.Width - buttonSizeFactor1 * 0.2);
                            result12.Top = (int)(drawBoundsHeight - buttonSizeFactor1 * 1.2);
                            result12.Bottom = (int)(drawBoundsHeight - buttonSizeFactor1 * 0.2);
                            return result12;
                        }
                        TinyRect result13 = default(TinyRect);
                        result13.Left = (int)(buttonSizeFactor1 * 0.2);
                        result13.Right = (int)(buttonSizeFactor1 * 1.2);
                        result13.Top = (int)(drawBoundsHeight - buttonSizeFactor1 * 1.2);
                        result13.Bottom = (int)(drawBoundsHeight - buttonSizeFactor1 * 0.2);
                        return result13;
                    }
                case Def.ButtonGlyph.PlayDown:
                    {
                        if (gameData.JumpRight)
                        {
                            TinyRect result8 = default(TinyRect);
                            result8.Left = (int)(buttonSizeFactor1 * 0.2);
                            result8.Right = (int)(buttonSizeFactor1 * 1.2);
                            result8.Top = (int)(drawBoundsHeight - buttonSizeFactor1 * 1.2);
                            result8.Bottom = (int)(drawBoundsHeight - buttonSizeFactor1 * 0.2);
                            return result8;
                        }
                        TinyRect result9 = default(TinyRect);
                        result9.Left = (int)((double)drawBounds.Width - buttonSizeFactor1 * 1.2);
                        result9.Right = (int)((double)drawBounds.Width - buttonSizeFactor1 * 0.2);
                        result9.Top = (int)(drawBoundsHeight - buttonSizeFactor1 * 1.2);
                        result9.Bottom = (int)(drawBoundsHeight - buttonSizeFactor1 * 0.2);
                        return result9;
                    }
                case Def.ButtonGlyph.Cheat11:
                    {
                        TinyRect result7 = default(TinyRect);
                        result7.Left = (int)(cheatButtonSizeFactor * 0.0);
                        result7.Right = (int)(cheatButtonSizeFactor * 1.0);
                        result7.Top = (int)(cheatButtonSizeFactor * 0.0);
                        result7.Bottom = (int)(cheatButtonSizeFactor * 1.0);
                        return result7;
                    }
                case Def.ButtonGlyph.Cheat12:
                    {
                        TinyRect result6 = default(TinyRect);
                        result6.Left = (int)(cheatButtonSizeFactor * 0.0);
                        result6.Right = (int)(cheatButtonSizeFactor * 1.0);
                        result6.Top = (int)(cheatButtonSizeFactor * 1.0);
                        result6.Bottom = (int)(cheatButtonSizeFactor * 2.0);
                        return result6;
                    }
                case Def.ButtonGlyph.Cheat21:
                    {
                        TinyRect result5 = default(TinyRect);
                        result5.Left = (int)(cheatButtonSizeFactor * 1.0);
                        result5.Right = (int)(cheatButtonSizeFactor * 2.0);
                        result5.Top = (int)(cheatButtonSizeFactor * 0.0);
                        result5.Bottom = (int)(cheatButtonSizeFactor * 1.0);
                        return result5;
                    }
                case Def.ButtonGlyph.Cheat22:
                    {
                        TinyRect result4 = default(TinyRect);
                        result4.Left = (int)(cheatButtonSizeFactor * 1.0);
                        result4.Right = (int)(cheatButtonSizeFactor * 2.0);
                        result4.Top = (int)(cheatButtonSizeFactor * 1.0);
                        result4.Bottom = (int)(cheatButtonSizeFactor * 2.0);
                        return result4;
                    }
                case Def.ButtonGlyph.Cheat31:
                    {
                        TinyRect result3 = default(TinyRect);
                        result3.Left = (int)(cheatButtonSizeFactor * 2.0);
                        result3.Right = (int)(cheatButtonSizeFactor * 3.0);
                        result3.Top = (int)(cheatButtonSizeFactor * 0.0);
                        result3.Bottom = (int)(cheatButtonSizeFactor * 1.0);
                        return result3;
                    }
                case Def.ButtonGlyph.Cheat32:
                    {
                        TinyRect result2 = default(TinyRect);
                        result2.Left = (int)(cheatButtonSizeFactor * 2.0);
                        result2.Right = (int)(cheatButtonSizeFactor * 3.0);
                        result2.Top = (int)(cheatButtonSizeFactor * 1.0);
                        result2.Bottom = (int)(cheatButtonSizeFactor * 2.0);
                        return result2;
                    }
                default:
                    return default(TinyRect);
            }
        }

        private void StartAccel()
        {
            try
            {
                accelSensor.Start();
                accelStarted = true;
            }
            catch (AccelerometerFailedException)
            {
                accelStarted = false;
            }
            catch (UnauthorizedAccessException)
            {
                accelStarted = false;
            }
        }

        private void StopAccel()
        {
            if (accelStarted)
            {
                try
                {
                    accelSensor.Stop();
                }
                catch (AccelerometerFailedException)
                {
                }
                accelStarted = false;
            }
        }


        private void HandleAccelSensorCurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0006: Unknown result type (might be due to invalid IL or missing references)

            AccelerometerReading sensorReading = e.SensorReading;
            float y = ((AccelerometerReading)(sensorReading)).Acceleration.Y;
            float sensitivityThreshold = (1f - (float)gameData.AccelSensitivity) * 0.06f + 0.04f;
            float adjustedThreshold = (accelLastState ? (sensitivityThreshold * 0.6f) : sensitivityThreshold);
            if (y > adjustedThreshold)
            {
                accelSpeedX = 0.0 - Math.Min((double)y * 0.25 / (double)sensitivityThreshold + 0.25, 1.0);
            }
            else if (y < 0f - adjustedThreshold)
            {
                accelSpeedX = Math.Min((double)(0f - y) * 0.25 / (double)sensitivityThreshold + 0.25, 1.0);
            }
            else
            {
                accelSpeedX = 0.0;
            }
            accelLastState = accelSpeedX != 0.0;
            if (accelWaitZero)
            {
                if (accelSpeedX == 0.0)
                {
                    accelWaitZero = false;
                }
                else
                {
                    accelSpeedX = 0.0;
                }
            }
        }
    }

}