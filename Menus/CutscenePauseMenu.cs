using FairMultiplayerCutsceneExperience.Utils;
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
        private ClickableTextureComponent? _inventoryButton;
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
            _messageLines = TextUtils.WrapText(playerMessage, width - Game1.tileSize * 2);
            _tipLines = TextUtils.WrapText(tipMessage, width);
        }

        private static int CalculateMenuHeight(string[] spriteMessages, string[] smallMessages)
        {
            var height = HeightWithoutPlayerLines;

            height += spriteMessages.Sum(message =>
                Game1.tileSize * (TextUtils.WrapText(message, MenuWidth - Game1.tileSize * 2).Count - 1));

            height += smallMessages.Sum(message =>
                Game1.tileSize / 2 * (TextUtils.WrapText(message, MenuWidth).Count - 1));

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
            DrawInventoryButton(spriteBatch,
                yPositionOnScreen + Game1.tileSize + Game1.tileSize / 2 + Game1.tileSize / 6);
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

        private void DrawInventoryButton(SpriteBatch spriteBatch, int buttonY)
        {
            var buttonX = xPositionOnScreen + MenuWidth - Game1.tileSize - Game1.tileSize / 3;

            _inventoryButton = new ClickableTextureComponent(
                new Rectangle
                (
                    buttonX,
                    buttonY,
                    Game1.tileSize / 2 + Game1.tileSize / 5,
                    Game1.tileSize / 2 + Game1.tileSize / 5
                ),
                Game1.content.Load<Texture2D>("LooseSprites/Cursors"),
                new Rectangle(127, 412, 10, 11),
                4f,
                true
            )
            {
                hoverText = ModEntry.GetString("menu.openInventory"),
            };

            _inventoryButton.name = "Inventory Button";
            _inventoryButton.myID = 0;

            _inventoryButton.draw(spriteBatch);

            if (_inventoryButton.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
            {
                drawHoverText(spriteBatch, _inventoryButton.hoverText, Game1.dialogueFont);
                HandleButtonClick(new GameMenu());
            }
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
            _prairieKingButton.myID = 1;

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
            _junimoKartButton.myID = 2;

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
            var isInventoryButtonHovered = _inventoryButton != null &&
                                           _inventoryButton.containsPoint(Game1.getMouseX(),
                                               Game1.getMouseY());

            var isPrairieKingButtonHovered = _prairieKingButton != null &&
                                             _prairieKingButton.containsPoint(Game1.getMouseX(),
                                                 Game1.getMouseY());

            var isJunimoKartButton = _junimoKartButton != null &&
                                     _junimoKartButton.containsPoint(Game1.getMouseX(), Game1.getMouseY());

            if (isInventoryButtonHovered || isPrairieKingButtonHovered || isJunimoKartButton)
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

        private void HandleButtonClick(IClickableMenu menu)
        {
            if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed &&
                ModEntry.PreviousLeftButtonState == ButtonState.Released)
            {
                Game1.activeClickableMenu = menu;
                ModEntry.PreviousLeftButtonState = ButtonState.Pressed;
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

            Game1.setMousePosition
            (
                currentlySnappedComponent.bounds.Right - currentlySnappedComponent.bounds.Width / 4,
                currentlySnappedComponent.bounds.Bottom - currentlySnappedComponent.bounds.Height / 2,
                true
            );
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);

            List<ClickableComponent?> minigameButtons = new() { _prairieKingButton, _junimoKartButton };

            if (b is Buttons.DPadUp or Buttons.DPadDown)
            {
                if (currentlySnappedComponent == null)
                {
                    currentlySnappedComponent = _inventoryButton;
                    return;
                }

                if (currentlySnappedComponent?.myID == _inventoryButton?.myID && b == Buttons.DPadDown)
                {
                    currentlySnappedComponent = minigameButtons.First();
                    return;
                }

                if (currentlySnappedComponent?.myID != _inventoryButton?.myID && b == Buttons.DPadUp)
                {
                    currentlySnappedComponent = _inventoryButton;
                    return;
                }
            }

            if (b is Buttons.DPadRight or Buttons.DPadLeft)
            {
                if (currentlySnappedComponent == null)
                {
                    currentlySnappedComponent = minigameButtons.First();
                    return;
                }

                if (currentlySnappedComponent?.myID == _inventoryButton?.myID)
                {
                    snapCursorToCurrentSnappedComponent();
                    return;
                }

                var currentIndex = minigameButtons.FindIndex(clickableComponent =>
                    clickableComponent?.myID == currentlySnappedComponent?.myID);

                var nextIndex = (currentIndex + (b == Buttons.DPadRight ? 1 : -1) + minigameButtons.Count) %
                                minigameButtons.Count;

                currentlySnappedComponent = minigameButtons[nextIndex];

                return;
            }

            if (_inventoryButton != null &&
                currentlySnappedComponent?.myID == _inventoryButton?.myID &&
                b == Buttons.A)
            {
                Game1.activeClickableMenu = new GameMenu();
                currentlySnappedComponent = null;
            }
            else if (_junimoKartButton != null &&
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
    }
}