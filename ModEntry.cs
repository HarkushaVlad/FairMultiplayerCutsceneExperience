using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Minigames;

namespace FairMultiplayerCutsceneExperience
{
    internal sealed class ModEntry : Mod
    {
        private const string MessageTypeSendChatMessage = "sendChatMessage";
        private const string MessageTypeStartPause = "startPause";
        private const string MessageTypeEndPause = "endPause";
        private const string MessageTypeAddPlayerToInitiators = "addPlayerToInitiators";
        private const string MessageTypeRemovePlayerFromInitiators = "removePlayerFromInitiators";

        private static ButtonState _previousLeftButtonState = ButtonState.Released;
        private static readonly HashSet<long> CutsceneInitiators = new();

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.RenderingHud += OnRenderingHud;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
            helper.Events.Multiplayer.PeerDisconnected += OnPeerDisconnected;
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (Game1.CurrentEvent != null)
            {
                Farmer? initiator = Game1.CurrentEvent.farmer;
                if (initiator == null)
                    return;

                long playerId = initiator.UniqueMultiplayerID;

                if (!IsPlayerInCutscene(playerId) &&
                    Game1.CurrentEvent.eventCommands.Contains("skippable"))
                {
                    string message = $"{initiator.Name} has started a cutscene!";
                    BroadcastMessage(message);

                    CutsceneInitiators.Add(playerId);
                    SendAddInitiatorMessageToAll(playerId);

                    SendOpenMinigameMessageToAll();
                }
            }
            else if (IsCutsceneActive() &&
                     IsPlayerInCutscene(Game1.player.UniqueMultiplayerID) &&
                     Game1.CurrentEvent == null)
            {
                SendCloseMinigameMessageToAll();

                CutsceneInitiators.Remove(Game1.player.UniqueMultiplayerID);
                SendRemoveInitiatorMessageToAll(Game1.player.UniqueMultiplayerID);

                string message = $"{Game1.player.Name} has finished a cutscene!";
                BroadcastMessage(message);
            }
        }

        private void OnPeerDisconnected(object? sender, PeerDisconnectedEventArgs e)
        {
            if (CutsceneInitiators.Contains(e.Peer.PlayerID))
            {
                CutsceneInitiators.Remove(e.Peer.PlayerID);
                SendRemoveInitiatorMessageToAll(e.Peer.PlayerID);
                SendCloseMinigameMessageToAll();
            }
        }

        private void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == ModManifest.UniqueID)
            {
                switch (e.Type)
                {
                    case MessageTypeSendChatMessage:
                        Game1.chatBox.addMessage(e.ReadAs<string>(), Color.Gold);
                        break;
                    case MessageTypeStartPause:
                        if (!IsPlayerInCutscene(Game1.player.UniqueMultiplayerID))
                            StartPause();
                        break;
                    case MessageTypeEndPause:
                        if (!IsPlayerInCutscene(Game1.player.UniqueMultiplayerID))
                            EndPause();
                        break;
                    case MessageTypeAddPlayerToInitiators:
                        CutsceneInitiators.Add(e.ReadAs<long>());
                        break;
                    case MessageTypeRemovePlayerFromInitiators:
                        CutsceneInitiators.Remove(e.ReadAs<long>());
                        break;
                }
            }
        }

        private void OnRenderingHud(object? sender, RenderingHudEventArgs e)
        {
            if (
                IsCutsceneActive() &&
                !IsPlayerInCutscene(Game1.player.UniqueMultiplayerID))
            {
                StartPause();
            }
        }

        private void BroadcastMessage(string message)
        {
            Monitor.Log(message, LogLevel.Info);
            Game1.chatBox.addMessage(message, Color.Gold);
            SendChatMessageToAll(message);
        }

        private void SendChatMessageToAll(string message)
        {
            Helper.Multiplayer.SendMessage(
                message: message,
                messageType: MessageTypeSendChatMessage,
                modIDs: new[] { ModManifest.UniqueID }
            );
        }

        private void SendOpenMinigameMessageToAll()
        {
            Helper.Multiplayer.SendMessage(
                message: MessageTypeStartPause,
                messageType: MessageTypeStartPause,
                modIDs: new[] { ModManifest.UniqueID }
            );
        }

        private void SendCloseMinigameMessageToAll()
        {
            Helper.Multiplayer.SendMessage(
                message: MessageTypeEndPause,
                messageType: MessageTypeEndPause,
                modIDs: new[] { ModManifest.UniqueID }
            );
        }

        private void SendAddInitiatorMessageToAll(long playerId)
        {
            Helper.Multiplayer.SendMessage(
                message: playerId,
                messageType: MessageTypeAddPlayerToInitiators,
                modIDs: new[] { ModManifest.UniqueID }
            );
        }

        private void SendRemoveInitiatorMessageToAll(long playerId)
        {
            Helper.Multiplayer.SendMessage(
                message: playerId,
                messageType: MessageTypeRemovePlayerFromInitiators,
                modIDs: new[] { ModManifest.UniqueID }
            );
        }

        private static void StartPause()
        {
            Game1.activeClickableMenu = new PauseMenu(Game1.getOnlineFarmers()
                .First(farmer => farmer.UniqueMultiplayerID == CutsceneInitiators.ToArray()[0]).Name);
        }

        private static void EndPause()
        {
            Game1.currentMinigame?.forceQuit();
            Game1.currentMinigame = null;
            Game1.activeClickableMenu = null;
        }

        private static void OpenMinigame()
        {
            Game1.currentMinigame = new MineCart(new Random().Next(0, 9), 3);
        }


        private bool IsPlayerInCutscene(long playerId)
        {
            return CutsceneInitiators.Contains(playerId);
        }

        private bool IsCutsceneActive()
        {
            return CutsceneInitiators.Count > 0;
        }

        private class PauseMenu : IClickableMenu
        {
            private readonly string _initiatorPlayerName;
            private ClickableTextureComponent? _junimoKartButton;

            public PauseMenu(string initiatorPlayerName)
                : base(
                    (Game1.viewport.Width - Game1.tileSize * 17) / 2,
                    (Game1.viewport.Height - Game1.tileSize * 7) / 2,
                    Game1.tileSize * 17,
                    Game1.tileSize * 7
                )
            {
                _initiatorPlayerName = initiatorPlayerName;
            }

            public override void draw(SpriteBatch spriteBatch)
            {
                DrawBackground();
                int currentYPosition = yPositionOnScreen + Game1.tileSize;
                currentYPosition = DrawPauseMessage(spriteBatch, currentYPosition);
                currentYPosition = DrawPlayerMessage(spriteBatch, currentYPosition);
                DrawJunimoKartButton(spriteBatch, currentYPosition);
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
                    "Game Paused",
                    xPositionOnScreen + width / 2,
                    startY
                );

                return startY + Game1.tileSize;
            }

            private int DrawPlayerMessage(SpriteBatch spriteBatch, int startY)
            {
                int messageX = xPositionOnScreen + Game1.tileSize;

                string playerMessage =
                    $"{(_initiatorPlayerName.Length > 0 ? _initiatorPlayerName : "Player")} is currently in a cutscene!";
                List<string> wrappedMessageLines = WrapText(playerMessage, width - Game1.tileSize * 2);

                foreach (string line in wrappedMessageLines)
                {
                    SpriteText.drawString(spriteBatch, line, messageX, startY);
                    startY += Game1.tileSize;
                }

                string waitMessage = "While you wait for the cutscene to finish:";
                SpriteText.drawString(spriteBatch, waitMessage, messageX, startY);

                return startY + Game1.tileSize;
            }

            private void DrawJunimoKartButton(SpriteBatch spriteBatch, int buttonY)
            {
                int buttonX = xPositionOnScreen + width / 2 - Game1.tileSize / 2;

                _junimoKartButton = new ClickableTextureComponent(
                    new Rectangle(buttonX, buttonY, Game1.tileSize, Game1.tileSize * 2),
                    Game1.content.Load<Texture2D>("TileSheets/Craftables"),
                    new Rectangle(112, 608, 16, 32),
                    4f,
                    true
                )
                {
                    hoverText = "Play Junimo Kart"
                };

                _junimoKartButton.draw(spriteBatch);

                if (_junimoKartButton.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                {
                    Game1.mouseCursor = Game1.cursor_grab;
                    drawMouse(spriteBatch, false, Game1.cursor_grab);

                    drawHoverText(spriteBatch, _junimoKartButton.hoverText, Game1.dialogueFont);
                    HandleButtonClick();
                }
                else
                {
                    Game1.mouseCursor = Game1.cursor_default;
                    drawMouse(spriteBatch, false, Game1.cursor_default);
                }

                if (Game1.input.GetGamePadState().IsConnected)
                {
                    currentlySnappedComponent = _junimoKartButton;
                    snapCursorToCurrentSnappedComponent();
                }
            }

            private void HandleButtonClick()
            {
                if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed &&
                    _previousLeftButtonState == ButtonState.Released)
                {
                    Game1.playSound("coin");
                    OpenMinigame();
                    _previousLeftButtonState = ButtonState.Pressed;
                    exitThisMenuNoSound();
                }

                if (Game1.input.GetMouseState().LeftButton == ButtonState.Released)
                {
                    _previousLeftButtonState = ButtonState.Released;
                }
            }

            public override void receiveGamePadButton(Buttons b)
            {
                base.receiveGamePadButton(b);

                if (_junimoKartButton != null &&
                    currentlySnappedComponent == _junimoKartButton &&
                    (b is Buttons.A or Buttons.X))
                {
                    Game1.playSound("coin");
                    OpenMinigame();
                    exitThisMenuNoSound();
                    currentlySnappedComponent = null;
                    _junimoKartButton = null;
                }
            }
        }

        private static List<string> WrapText(string text, int maxWidth)
        {
            List<string> lines = new List<string>();
            string[] words = text.Split(' ');
            string currentLine = "";

            foreach (string word in words)
            {
                if (SpriteText.getWidthOfString(word) > maxWidth)
                {
                    string truncatedWord = TruncateWord(word, maxWidth);
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        lines.Add(currentLine);
                        currentLine = "";
                    }

                    lines.Add(truncatedWord);
                }
                else
                {
                    string testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
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
            string truncatedWord = "";
            foreach (char c in word)
            {
                string testWord = truncatedWord + c;
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