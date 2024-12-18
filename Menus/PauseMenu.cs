using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace FairMultiplayerCutsceneExperience.Menus
{
    internal class PauseMenu : IClickableMenu
    {
        private const int MenuWidth = Game1.tileSize * 13;
        private const int BaseHeight = Game1.tileSize * 6;

        private static List<string> _tipLines = new();
        private static int _menuHeight;

        public PauseMenu(string? tipMessage = null)
            : base(
                (Game1.uiViewport.Width - MenuWidth) / 2,
                (Game1.uiViewport.Height - CalculateMenuHeight(tipMessage)) / 2,
                MenuWidth,
                _menuHeight
            )
        {
        }

        private static int CalculateMenuHeight(string? tipMessage = null)
        {
            var height = BaseHeight;

            _tipLines = WrapText(tipMessage ?? ModEntry.GetRandomTip(), MenuWidth);

            height += Game1.tileSize / 2 * (_tipLines.Count - 1);

            _menuHeight = height;

            return height;
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            DrawBackground();
            var currentYPosition = yPositionOnScreen + Game1.tileSize;
            currentYPosition = DrawPauseMessage(spriteBatch, currentYPosition);
            DrawTip(spriteBatch, currentYPosition + Game1.tileSize);
            base.draw(spriteBatch);
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

        private void DrawTip(SpriteBatch spriteBatch, int tipY)
        {
            foreach (var line in _tipLines)
            {
                var tipX = xPositionOnScreen + (width / 2) - (int)(Game1.smallFont.MeasureString(line).X / 2);
                spriteBatch.DrawString(Game1.smallFont, line, new Vector2(tipX, tipY), Color.Black);
                tipY += Game1.tileSize / 2;
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