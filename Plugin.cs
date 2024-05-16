using Aki.Reflection.Patching;
using BepInEx;
using BepInEx.Configuration;
using EFT;
using System;
using System.Reflection;

namespace Solarint.LouderSuppressors
{
    [BepInPlugin("solarint.loudSuppressors", "Solarint.LouderSuppressors", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            try
            {
                Settings.Init(Config);
                new SuppressedSoundRolloffPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }
    }

    public class SuppressedSoundRolloffPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return HarmonyLib.AccessTools.Method(typeof(WeaponSoundPlayer), "FireBullet");
        }

        [PatchPrefix]
        public static void PatchPrefix(WeaponSoundPlayer __instance)
        {
            if (__instance?.IsSilenced == true 
                && Settings.ModEnabled.Value)
            {
                float maxDist = Settings.SuppressorVolume.Value;
                SetRolloff(__instance.TailSilenced, maxDist);
                SetRolloff(__instance.BodySilenced, maxDist);
            }
        }

        private static void SetRolloff(SoundBank soundBank, float maxDist)
        {
            if (soundBank != null
                && soundBank.Rolloff != maxDist)
            {
                soundBank.Rolloff = maxDist;

                if (soundBank.BlendValues != null)
                {
                    soundBank.BlendValues[0] *= 1.5f;
                    soundBank.BlendValues[1] *= 1.75f;
                    soundBank.BlendValues[2] *= 2f;
                    soundBank.BlendValues[3] *= 2.25f;

                    if (soundBank.BlendValues[3] < maxDist)
                    {
                        soundBank.BlendValues[3] = maxDist;
                    }
                }
            }
        }
    }

    internal class Settings
    {
        private const string GeneralSectionTitle = "General";
        public static ConfigEntry<bool> ModEnabled;
        public static ConfigEntry<float> SuppressorVolume;

        public static void Init(ConfigFile Config)
        {
            ModEnabled = Config.Bind(
                GeneralSectionTitle,
                "Enable Louder Suppressors",
                true,
                "Turns this mod on or Off. Requires restart if in raid."
                );

            SuppressorVolume = Config.Bind(
                GeneralSectionTitle,
                "Suppressor Max Audible Distance",
                225f,
                new ConfigDescription(
                    "The max distance you will be able to hear suppressors. Requires restart if in raid.",
                    new AcceptableValueRange<float>(100f, 400f)
                ));
        }
    }
}
