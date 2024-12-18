using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Minigames;

namespace FairMultiplayerCutsceneExperience.Menus
{
    internal class CutscenePauseMenu : IClickableMenu
    {
        private const int MenuWidth = Game1.tileSize * 17;
        private const int HeightWithoutPlayerLines = Game1.tileSize * 9 + Game1.tileSize / 2;

        private readonly List<string> _messageLines;
        private readonly List<string> _tipLines;
        private ClickableTextureComponent? _prairieKingButton;
        private ClickableTextureComponent? _junimoKartButton;

        public CutscenePauseMenu(string initiatorPlayerName, string tipMessage)
            : base(
                (Game1.uiViewport.Width - MenuWidth) / 2,
                (Game1.uiViewport.Height - CalculateMenuHeight(new[]
                    {
                        initiatorPlayerName
                    },
                    new[] { tipMessage })) / 2,
                MenuWidth,
                CalculateMenuHeight(new[] { initiatorPlayerName }, new[] { tipMessage })
            )
        {
            var playerMessage = GetPlayerMessage(initiatorPlayerName);
            _messageLines = WrapText(playerMessage, width - Game1.tileSize * 2);
            _tipLines = WrapText(tipMessage, width);
        }

        private static int CalculateMenuHeight(string[] spriteMessages, string[] smallMessages)
        {
            var height = HeightWithoutPlayerLines;

            height += spriteMessages.Sum(message =>
                Game1.tileSize * (WrapText(message, MenuWidth - Game1.tileSize * 2).Count - 1));

            height += smallMessages.Sum(message => Game1.tileSize / 2 * (WrapText(message, MenuWidth).Count - 1));

            return height;
        }


        private string GetPlayerMessage(string initiatorPlayerName)
        {
            var playerName = initiatorPlayerName.Length > 0
                ? initiatorPlayerName
                : ModEntry.GetString($"menu.player");

            return $" {ModEntry.GetString($"menu.inCutscene", new { name = playerName })}";
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            DrawBackground();
            var currentYPosition = yPositionOnScreen + Game1.tileSize;
            currentYPosition = DrawPauseMessage(spriteBatch, currentYPosition);
            currentYPosition = DrawPlayerMessage(spriteBatch, currentYPosition);
            DrawTip(spriteBatch, currentYPosition + Game1.tileSize * 3);
            DrawJunimoKartButton(spriteBatch, currentYPosition);
            DrawPrairieKingButton(spriteBatch, currentYPosition);
            HandleCursorType(spriteBatch);
            base.draw(spriteBatch);
        }

        private static void OpenJunimoCartMinigame()
        {
            Game1.currentMinigame = new MineCart(new Random().Next(0, 9), 3);
        }

        private static void OpenPrairieKingMinigame()
        {
            Game1.currentMinigame = new AbigailGame();
        }

        private void DrawBackground()
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);
        }

        private int DrawPauseMessage(SpriteBatch spriteBatch, int startY)
        {
            SpriteText.drawStringWithScrollCenteredAt(
                spriteBatch,
                ModEntry.GetString($"menu.title"),
                xPositionOnScreen + width / 2,
                startY
            );

            return startY + Game1.tileSize + Game1.tileSize / 2;
        }

        private int DrawPlayerMessage(SpriteBatch spriteBatch, int startY)
        {
            var messageX = xPositionOnScreen + Game1.tileSize;

            foreach (var line in _messageLines)
            {
                SpriteText.drawString(spriteBatch, line, messageX, startY);
                startY += Game1.tileSize;
            }

            SpriteText.drawString(spriteBatch, ModEntry.GetString("menu.whileWait"), messageX, startY);

            return startY + Game1.tileSize + Game1.tileSize / 2;
        }

        private void DrawPrairieKingButton(SpriteBatch spriteBatch, int buttonY)
        {
            var buttonX = xPositionOnScreen + width / 2 - Game1.tileSize - Game1.tileSize / 2;

            _prairieKingButton = new ClickableTextureComponent(
                new Rectangle(buttonX, buttonY, Game1.tileSize, Game1.tileSize * 2),
                Game1.content.Load<Texture2D>("TileSheets/Craftables"),
                new Rectangle(80, 544, 16, 32),
                4f,
                true
            )
            {
                hoverText = ModEntry.GetString($"menu.playPrairieKing")
            };

            _prairieKingButton.name = "Prairie King Button";
            _prairieKingButton.myID = 0;

            _prairieKingButton.draw(spriteBatch);

            if (_prairieKingButton.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
            {
                drawHoverText(spriteBatch, _prairieKingButton.hoverText, Game1.dialogueFont);
                HandleButtonClick(OpenPrairieKingMinigame);
            }
        }

        private void DrawJunimoKartButton(SpriteBatch spriteBatch, int buttonY)
        {
            var buttonX = xPositionOnScreen + width / 2 + Game1.tileSize / 2;

            _junimoKartButton = new ClickableTextureComponent(
                new Rectangle(buttonX, buttonY, Game1.tileSize, Game1.tileSize * 2),
                Game1.content.Load<Texture2D>("TileSheets/Craftables"),
                new Rectangle(112, 608, 16, 32),
                4f,
                true
            )
            {
                hoverText = ModEntry.GetString($"menu.playJunimoKart")
            };

            _junimoKartButton.name = "Junimo Kart Button";
            _junimoKartButton.myID = 1;

            _junimoKartButton.draw(spriteBatch);

            if (_junimoKartButton.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
            {
                drawHoverText(spriteBatch, _junimoKartButton.hoverText, Game1.dialogueFont);
                HandleButtonClick(OpenJunimoCartMinigame);
            }
        }

        private void DrawTip(SpriteBatch spriteBatch, int tipY)
        {
            foreach (var line in _tipLines)
            {
                var tipX = xPositionOnScreen + (width / 2) - (int)(Game1.smallFont.MeasureString(line).X / 2);
                spriteBatch.DrawString(Game1.smallFont, line, new Vector2(tipX, tipY), Color.Black);
                tipY += Game1.tileSize / 2;
            }
        }

        private void HandleCursorType(SpriteBatch spriteBatch)
        {
            var isPrairieKingButtonHovered = _prairieKingButton != null &&
                                             _prairieKingButton.containsPoint(Game1.getMouseX(),
                                                 Game1.getMouseY());

            var isJunimoKartButton = _junimoKartButton != null &&
                                     _junimoKartButton.containsPoint(Game1.getMouseX(), Game1.getMouseY());

            if (isPrairieKingButtonHovered || isJunimoKartButton)
            {
                Game1.mouseCursor = Game1.cursor_gamepad_pointer;
                drawMouse(spriteBatch, false, Game1.cursor_gamepad_pointer);
            }
            else
            {
                Game1.mouseCursor = Game1.cursor_default;
                drawMouse(spriteBatch, false, Game1.cursor_default);
            }
        }

        private void HandleButtonClick(Action openMinigame)
        {
            if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed &&
                ModEntry.PreviousLeftButtonState == ButtonState.Released)
            {
                Game1.playSound("coin");
                openMinigame();
                ModEntry.PreviousLeftButtonState = ButtonState.Pressed;
                exitThisMenuNoSound();
            }

            if (Game1.input.GetMouseState().LeftButton == ButtonState.Released)
            {
                ModEntry.PreviousLeftButtonState = ButtonState.Released;
            }
        }

        public override void snapCursorToCurrentSnappedComponent()
        {
            if (currentlySnappedComponent == null)
                return;

            Game1.setMousePosition(
                currentlySnappedComponent.bounds.Right - currentlySnappedComponent.bounds.Width / 4,
                currentlySnappedComponent.bounds.Bottom - currentlySnappedComponent.bounds.Height / 2, true);
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);

            List<ClickableComponent?> menuButtons = new() { _prairieKingButton, _junimoKartButton };

            if ((b == Buttons.DPadRight || b == Buttons.DPadLeft || b == Buttons.DPadUp || b == Buttons.DPadDown) &&
                menuButtons.Count > 0)
            {
                if (currentlySnappedComponent == null)
                {
                    currentlySnappedComponent = menuButtons.First();
                    return;
                }

                if (b == Buttons.DPadUp || b == Buttons.DPadDown)
                {
                    snapCursorToCurrentSnappedComponent();
                    return;
                }

                var currentIndex = menuButtons.FindIndex(clickableComponent =>
                    clickableComponent?.myID == currentlySnappedComponent?.myID);

                var nextIndex = (currentIndex + (b == Buttons.DPadRight ? 1 : -1) + menuButtons.Count) %
                                menuButtons.Count;

                currentlySnappedComponent = menuButtons[nextIndex];
            }

            if (_junimoKartButton != null &&
                currentlySnappedComponent?.myID == _junimoKartButton?.myID &&
                b == Buttons.A)
            {
                Game1.playSound("coin");
                OpenJunimoCartMinigame();
                exitThisMenuNoSound();
                currentlySnappedComponent = null;
            }
            else if (_prairieKingButton != null &&
                     currentlySnappedComponent?.myID == _prairieKingButton?.myID &&
                     b == Buttons.A)
            {
                Game1.playSound("coin");
                OpenPrairieKingMinigame();
                exitThisMenuNoSound();
                currentlySnappedComponent = null;
            }
        }

        private static List<string> WrapText(string text, int maxWidth)
        {
            var lines = new List<string>();
            var words = text.Split(' ');
            var currentLine = "";

            foreach (var word in words)
            {
                if (SpriteText.getWidthOfString(word) > maxWidth)
                {
                    var truncatedWord = TruncateWord(word, maxWidth);
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        lines.Add(currentLine);
                        currentLine = "";
                    }

                    lines.Add(truncatedWord);
                }
                else
                {
                    var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
                    if (SpriteText.getWidthOfString(testLine) <= maxWidth)
                    {
                        currentLine = testLine;
                    }
                    else
                    {
                        lines.Add(currentLine);
                        currentLine = word;
                    }
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
            }

            return lines;
        }

        private static string TruncateWord(string word, int maxWidth)
        {
            var truncatedWord = "";
            foreach (var c in word)
            {
                var testWord = truncatedWord + c;
                if (SpriteText.getWidthOfString(testWord + "...") <= maxWidth)
                {
                    truncatedWord = testWord;
                }
                else
                {
                    break;
                }
            }

            return truncatedWord + "...";
        }
    }
}