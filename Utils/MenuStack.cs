using StardewValley;
using StardewValley.Menus;

namespace FairMultiplayerCutsceneExperience.Utils
{
    public static class MenuStack
    {
        private static readonly Stack<IClickableMenu> MenuStackStorage = new();

        public static void PushMenu(IClickableMenu menu)
        {
            if (Game1.activeClickableMenu != null)
                MenuStackStorage.Push(Game1.activeClickableMenu);

            Game1.activeClickableMenu = menu;
        }

        public static void PopMenu()
        {
            Game1.activeClickableMenu = MenuStackStorage.Count > 0 ? MenuStackStorage.Pop() : null;
        }

        public static void Clear()
        {
            MenuStackStorage.Clear();
        }
    }
}