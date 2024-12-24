using FairMultiplayerCutsceneExperience.Config;
using FairMultiplayerCutsceneExperience.Menus;
using FairMultiplayerCutsceneExperience.Utils;
using HarmonyLib;
using StardewValley;
using StardewValley.Network;

namespace FairMultiplayerCutsceneExperience.Patches
{
    [HarmonyPatch(typeof(NetWorldState), "set_IsPaused")]
    public static class PauseCommandPatch
    {
        public static void Postfix(bool value)
        {
            if (Game1.netWorldState.Value.IsPaused)
            {
                ModEntry.SendOpenPauseMenuMessageToAll();

                if (ModEntry.StaticHelper.ReadConfig<ModConfig>().EnablePauseMenu)
                {
                    MenuStack.PushMenu(new PauseMenu());
                }
            }
            else
            {
                ModEntry.SendClosePauseMenuMessageToAll();

                if (ModEntry.StaticHelper.ReadConfig<ModConfig>().EnablePauseMenu)
                {
                    MenuStack.PopMenu();
                }
            }
        }
    }
}