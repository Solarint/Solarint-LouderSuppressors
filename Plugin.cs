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
                float maxDist = Settings.Rolloff.Value;
                SetRolloff(__instance.TailSilenced, maxDist, Settings.TailDistModifier.Value);
                SetRolloff(__instance.BodySilenced, maxDist, 1f);
            }
        }

        private static void SetRolloff(SoundBank soundBank, float maxDist, float modifier)
        {
            if (soundBank != null && soundBank.Rolloff != maxDist * modifier)
            {
                soundBank.Rolloff = maxDist * modifier;

                if (soundBank.BlendValues != null)
                {
                    soundBank.BlendValues[0] = Settings.BlendVal1.Value * modifier;
                    soundBank.BlendValues[1] = Settings.BlendVal2.Value * modifier;
                    soundBank.BlendValues[2] = Settings.BlendVal3.Value * modifier;
                    soundBank.BlendValues[3] = Settings.BlendVal4.Value * modifier;
                }
            }
        }
    }

    internal class Settings
    {
        private const string GeneralSectionTitle = "General";
        public static ConfigEntry<bool> ModEnabled;
        public static ConfigEntry<float> Rolloff;
        public static ConfigEntry<float> BlendVal1;
        public static ConfigEntry<float> BlendVal2;
        public static ConfigEntry<float> BlendVal3;
        public static ConfigEntry<float> BlendVal4;
        public static ConfigEntry<float> TailDistModifier;

        public static void Init(ConfigFile Config)
        {
            ModEnabled = Config.Bind(GeneralSectionTitle, "Enable Louder Suppressors", true, "Turns this mod on or Off. Requires restart if in raid.");
            Rolloff = Config.Bind(GeneralSectionTitle, "Suppressor Max Rolloff Distance", 200f, new ConfigDescription("", new AcceptableValueRange<float>(100f, 400f)));
            BlendVal1 = Config.Bind(GeneralSectionTitle, "Blend Val 1", 10f, new ConfigDescription("", new AcceptableValueRange<float>(5f, 400f)));
            BlendVal2 = Config.Bind(GeneralSectionTitle, "Blend Val 2", 40f, new ConfigDescription("", new AcceptableValueRange<float>(20f, 400f)));
            BlendVal3 = Config.Bind(GeneralSectionTitle, "Blend Val 3", 80f, new ConfigDescription("", new AcceptableValueRange<float>(50f, 400f)));
            BlendVal4 = Config.Bind(GeneralSectionTitle, "Blend Val 4", 175f, new ConfigDescription("", new AcceptableValueRange<float>(100f, 400f)));
            TailDistModifier = Config.Bind(GeneralSectionTitle, "TailDistModifier", 1.25f, new ConfigDescription("", new AcceptableValueRange<float>(0.75f, 1.5f)));
        }
    }
}