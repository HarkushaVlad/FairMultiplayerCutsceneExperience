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
        private const string MessageTypeSpecificStartPause = "specificStartPause";
        private const string MessageTypeEndPause = "endPause";

        private static ButtonState _previousLeftButtonState = ButtonState.Released;
        private static readonly HashSet<long> CutsceneInitiators = new();
        private static string? _hostModVersion;
        private static string? _currTip;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.RenderingHud += OnRenderingHud;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
            helper.Events.Multiplayer.PeerDisconnected += OnPeerDisconnected;
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.CurrentEvent != null)
            {
                Farmer? initiator = Game1.CurrentEvent.farmer;
                if (initiator == null)
                    return;

                long playerId = initiator.UniqueMultiplayerID;

                if (!IsPlayerInCutscene(playerId) &&
                    Game1.CurrentEvent.eventCommands.Contains("skippable"))
                {
                    string message = Helper.Translation.Get("message.startCutscene", new { initiator.name });
                    BroadcastMessage(message);

                    CutsceneInitiators.Add(playerId);

                    SendStartPauseMessageToAll(playerId);
                }
            }
            else if (IsCutsceneActive() &&
                     IsPlayerInCutscene(Game1.player.UniqueMultiplayerID) &&
                     Game1.CurrentEvent == null)
            {
                SendEndPauseMessageToAll(Game1.player.UniqueMultiplayerID);

                CutsceneInitiators.Remove(Game1.player.UniqueMultiplayerID);

                string message = Helper.Translation.Get("message.finishCutscene", new { Game1.player.name });
                BroadcastMessage(message);
            }
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (Game1.player.UniqueMultiplayerID == Game1.MasterPlayer.UniqueMultiplayerID)
            {
                _hostModVersion = ModManifest.Version.ToString();
            }
        }

        private void OnPeerConnected(object? sender, PeerConnectedEventArgs e)
        {
            var peer = e.Peer;
            var peerName = Game1.getOnlineFarmers().ToList()
                .Find(farmer => farmer.UniqueMultiplayerID == peer.PlayerID)?.Name;
            var peerMod = peer.Mods.FirstOrDefault(mod => mod.ID == ModManifest.UniqueID);

            if (peer.IsHost)
            {
                _hostModVersion = peerMod?.Version.ToString();
                return;
            }

            if (Game1.player.UniqueMultiplayerID != Game1.MasterPlayer.UniqueMultiplayerID)
                return;

            Thread.Sleep(3000);

            if (
                IsCutsceneActive() &&
                !IsPlayerInCutscene(e.Peer.PlayerID))
            {
                SendSpecificStartPauseMessage(e.Peer.PlayerID, CutsceneInitiators.ToArray()[0]);
            }

            if (peerMod == null)
            {
                string message = $"[{ModManifest.Name}] " +
                                 Helper.Translation.Get("message.noMod", new { name = peerName });
                BroadcastMessage(message, true);
                return;
            }

            if (peerMod?.Version.ToString() != _hostModVersion)
            {
                string message = $"[{ModManifest.Name}] " + Helper.Translation.Get(
                    "message.modVersionMismatch",
                    new { name = peerName, modVersion = peerMod?.Version.ToString(), hostModVersion = _hostModVersion }
                );
                BroadcastMessage(message, true);
            }
        }


        private void OnPeerDisconnected(object? sender, PeerDisconnectedEventArgs e)
        {
            if (CutsceneInitiators.Contains(e.Peer.PlayerID))
            {
                CutsceneInitiators.Remove(e.Peer.PlayerID);
                SendEndPauseMessageToAll(e.Peer.PlayerID);
            }
        }

        private void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == ModManifest.UniqueID)
            {
                switch (e.Type)
                {
                    case MessageTypeSendChatMessage:
                        var messageTuple = e.ReadAs<(string, bool)>();
                        Monitor.Log(messageTuple.Item1, messageTuple.Item2 ? LogLevel.Warn : LogLevel.Info);
                        Game1.chatBox.addMessage(messageTuple.Item1, messageTuple.Item2 ? Color.Orange : Color.Gold);
                        break;
                    case MessageTypeStartPause:
                        CutsceneInitiators.Add(e.ReadAs<long>());
                        if (!IsPlayerInCutscene(Game1.player.UniqueMultiplayerID))
                            StartPause();
                        break;
                    case MessageTypeSpecificStartPause:
                        var playerIds = e.ReadAs<(long, long)>();
                        if (Game1.player.UniqueMultiplayerID == playerIds.Item1)
                        {
                            var initiatorName = Game1.otherFarmers.ToList()
                                .Find(farmer => farmer.Key == playerIds.Item2).Value.name;
                            Game1.chatBox.addMessage(
                                Helper.Translation.Get("message.startCutscene",
                                    new { name = initiatorName.ToString() }), Color.Gold);
                            CutsceneInitiators.Add(playerIds.Item2);
                            StartPause();
                        }

                        break;
                    case MessageTypeEndPause:
                        CutsceneInitiators.Remove(e.ReadAs<long>());
                        if (!IsPlayerInCutscene(Game1.player.UniqueMultiplayerID))
                            EndPause();
                        break;
                }
            }
        }

        private void OnRenderingHud(object? sender, RenderingHudEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;

            if (
                IsCutsceneActive() &&
                !IsPlayerInCutscene(Game1.player.UniqueMultiplayerID))
            {
                StartPause();
            }
        }

        private void BroadcastMessage(string message, bool isWarning = false)
        {
            Monitor.Log(message, isWarning ? LogLevel.Warn : LogLevel.Info);
            Game1.chatBox.addMessage(message, isWarning ? Color.Orange : Color.Gold);
            SendChatMessageToAll(message, isWarning);
        }

        private void SendChatMessageToAll(string message, bool isError = false)
        {
            Helper.Multiplayer.SendMessage(
                message: (message, isError),
                messageType: MessageTypeSendChatMessage,
                modIDs: new[] { ModManifest.UniqueID }
            );
        }

        private void SendStartPauseMessageToAll(long initiatorId)
        {
            Helper.Multiplayer.SendMessage(
                message: initiatorId,
                messageType: MessageTypeStartPause,
                modIDs: new[] { ModManifest.UniqueID }
            );
        }

        private void SendSpecificStartPauseMessage(long playerId, long initiatorId)
        {
            Helper.Multiplayer.SendMessage(
                message: (playerId, initiatorId),
                messageType: MessageTypeSpecificStartPause,
                modIDs: new[] { ModManifest.UniqueID }
            );
        }

        private void SendEndPauseMessageToAll(long initiatorId)
        {
            Helper.Multiplayer.SendMessage(
                message: initiatorId,
                messageType: MessageTypeEndPause,
                modIDs: new[] { ModManifest.UniqueID }
            );
        }

        private void StartPause()
        {
            string initiatorName = Game1.getOnlineFarmers()
                .First(farmer => farmer.UniqueMultiplayerID == CutsceneInitiators.ToArray()[0]).Name;
            if (String.IsNullOrEmpty(_currTip))
                _currTip = Helper.Translation.Get($"tips.tip{new Random().Next(1, 37)}");
            Game1.activeClickableMenu = new PauseMenu(Helper, initiatorName, _currTip);
        }

        private void EndPause()
        {
            Game1.currentMinigame?.unload();
            Game1.currentMinigame = null;
            Game1.activeClickableMenu = null;
            _currTip = null;
        }

        private static void OpenJunimoCartMinigame()
        {
            Game1.currentMinigame = new MineCart(new Random().Next(0, 9), 3);
        }

        private static void OpenPrairieKingMinigame()
        {
            Game1.currentMinigame = new AbigailGame();
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
            private const int MenuWidth = Game1.tileSize * 17;
            private const int HeightWithoutPlayerLines = Game1.tileSize * 9 + Game1.tileSize / 2;

            private readonly IModHelper _helper;
            private readonly List<string> _messageLines;
            private readonly List<string> _tipLines;
            private ClickableTextureComponent? _prairieKingButton;
            private ClickableTextureComponent? _junimoKartButton;

            public PauseMenu(IModHelper helper, string initiatorPlayerName, string tipMessage)
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
                _helper = helper;
                string playerMessage = GetPlayerMessage(initiatorPlayerName);
                _messageLines = WrapText(playerMessage, width - Game1.tileSize * 2);
                _tipLines = WrapText(tipMessage, width);
            }

            private static int CalculateMenuHeight(string[] spriteMessages, string[] smallMessages)
            {
                int height = HeightWithoutPlayerLines;

                foreach (string message in spriteMessages)
                {
                    height += Game1.tileSize * (WrapText(message, MenuWidth - Game1.tileSize * 2).Count - 1);
                }

                foreach (string message in smallMessages)
                {
                    height += Game1.tileSize / 2 * (WrapText(message, MenuWidth).Count - 1);
                }

                return height;
            }


            private string GetPlayerMessage(string initiatorPlayerName)
            {
                string playerName = initiatorPlayerName.Length > 0
                    ? initiatorPlayerName
                    : _helper.Translation.Get($"menu.player");

                return $" {_helper.Translation.Get($"menu.inCutscene", new { name = playerName })}";
            }

            public override void draw(SpriteBatch spriteBatch)
            {
                DrawBackground();
                int currentYPosition = yPositionOnScreen + Game1.tileSize;
                currentYPosition = DrawPauseMessage(spriteBatch, currentYPosition);
                currentYPosition = DrawPlayerMessage(spriteBatch, currentYPosition);
                DrawJunimoKartButton(spriteBatch, currentYPosition);
                DrawPrairieKingButton(spriteBatch, currentYPosition);
                DrawTip(spriteBatch, currentYPosition + Game1.tileSize * 3);
                HandleCursorType(spriteBatch);
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
                    _helper.Translation.Get($"menu.title"),
                    xPositionOnScreen + width / 2,
                    startY
                );

                return startY + Game1.tileSize + Game1.tileSize / 2;
            }

            private int DrawPlayerMessage(SpriteBatch spriteBatch, int startY)
            {
                int messageX = xPositionOnScreen + Game1.tileSize;

                foreach (string line in _messageLines)
                {
                    SpriteText.drawString(spriteBatch, line, messageX, startY);
                    startY += Game1.tileSize;
                }

                SpriteText.drawString(spriteBatch, _helper.Translation.Get("menu.whileWait"), messageX, startY);

                return startY + Game1.tileSize + Game1.tileSize / 2;
            }

            private void DrawPrairieKingButton(SpriteBatch spriteBatch, int buttonY)
            {
                int buttonX = xPositionOnScreen + width / 2 - Game1.tileSize - Game1.tileSize / 2;

                _prairieKingButton = new ClickableTextureComponent(
                    new Rectangle(buttonX, buttonY, Game1.tileSize, Game1.tileSize * 2),
                    Game1.content.Load<Texture2D>("TileSheets/Craftables"),
                    new Rectangle(80, 544, 16, 32),
                    4f,
                    true
                )
                {
                    hoverText = _helper.Translation.Get($"menu.playPrairieKing")
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
                int buttonX = xPositionOnScreen + width / 2 + Game1.tileSize / 2;

                _junimoKartButton = new ClickableTextureComponent(
                    new Rectangle(buttonX, buttonY, Game1.tileSize, Game1.tileSize * 2),
                    Game1.content.Load<Texture2D>("TileSheets/Craftables"),
                    new Rectangle(112, 608, 16, 32),
                    4f,
                    true
                )
                {
                    hoverText = _helper.Translation.Get($"menu.playJunimoKart")
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
                foreach (string line in _tipLines)
                {
                    int tipX = xPositionOnScreen + (width / 2) - (int)(Game1.smallFont.MeasureString(line).X / 2);
                    spriteBatch.DrawString(Game1.smallFont, line, new Vector2(tipX, tipY), Color.Black);
                    tipY += Game1.tileSize / 2;
                }
            }

            private void HandleCursorType(SpriteBatch spriteBatch)
            {
                bool isPrairieKingButtonHovered = _prairieKingButton != null &&
                                                  _prairieKingButton.containsPoint(Game1.getMouseX(),
                                                      Game1.getMouseY());

                bool isJunimoKartButton = _junimoKartButton != null &&
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
                    _previousLeftButtonState == ButtonState.Released)
                {
                    Game1.playSound("coin");
                    openMinigame();
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

                    int currentIndex = menuButtons.FindIndex(clickableComponent =>
                        clickableComponent?.myID == currentlySnappedComponent?.myID);

                    int nextIndex = (currentIndex + (b == Buttons.DPadRight ? 1 : -1) + menuButtons.Count) %
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