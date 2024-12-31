using FairMultiplayerCutsceneExperience.Config;
using FairMultiplayerCutsceneExperience.Menus;
using FairMultiplayerCutsceneExperience.Utils;
using HarmonyLib;
using StardewValley;
using StardewValley.Network;

namespace FairMultiplayerCutsceneExperience.Patches
{
    public static class PauseCommandPatch
    {
        public static void ApplyPatch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(NetWorldState), "set_IsPaused"),
                postfix: new HarmonyMethod(typeof(PauseCommandPatch), nameof(Postfix))
            );
        }

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