using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace YeetMachine
{

    [BepInPlugin(PluginId, "YeetMachine", "0.0.7")]
    public class KickBan : BaseUnityPlugin
    {
        public const string PluginId = "YeetMachine";
        private Harmony _harmony;
        private AssetBundle uitest;
        private static GameObject customcanvas;
        private static GameObject menu;

        public static ConfigEntry<KeyCode> overlayKey;

        private void Awake()
        {
            overlayKey = Config.Bind("1 - General", "Key to open overlay", KeyCode.O, new ConfigDescription("Key that opens the overlay"));

            LoadAssets();
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginId);
        }

        public static void TryRegisterFabs(ZNetScene zNetScene)
        {
            if (zNetScene == null || zNetScene.m_prefabs == null || zNetScene.m_prefabs.Count <= 0)
            {
                return;
            }
            zNetScene.m_prefabs.Add(customcanvas);
        }
        private static AssetBundle GetAssetBundleFromResources(string filename)
        {
            var execAssembly = Assembly.GetExecutingAssembly();
            var resourceName = execAssembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(filename));

            using (var stream = execAssembly.GetManifestResourceStream(resourceName))
            {
                return AssetBundle.LoadFromStream(stream);
            }
        }
        private void LoadAssets()
        {
            uitest = GetAssetBundleFromResources("uitest");
            customcanvas = uitest.LoadAsset<GameObject>("Canvas");

            uitest?.Unload(false);

        }

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        public static class ZNetScene_Awake_Patch
        {
            public static bool Prefix(ZNetScene __instance)
            {

                TryRegisterFabs(__instance);
#if DEBUG
                Debug.Log("Loading the Menu thing");
#endif
                return true;
            }
        }
        
        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
        
        private static bool ButtonPressed(KeyCode button)
        {
            try
            {
                return Input.GetKeyDown(button);
            }
            catch
            {
                return false;
            }
        }
        
        private void Update()
        {
            PatchPlayerInputActive.skipOverlayActiveCheck = true;
            try
            {
                if (ButtonPressed(overlayKey.Value) && (!Chat.instance || !Chat.instance.HasFocus()) && !Console.IsVisible() && !TextInput.IsVisible() && !StoreGui.IsVisible() && !InventoryGui.IsVisible() && !Menu.IsVisible() && (!TextViewer.instance || !TextViewer.instance.IsVisible()) && !Minimap.IsOpen() && !GameCamera.InFreeFly())
                {
                    if (menu is null)
                    {
                        menu = Instantiate(customcanvas, GameObject.Find("IngameGui(Clone)").transform);
                        menu.name = "guildoverlay";
                        menu.transform.SetSiblingIndex(menu.transform.GetSiblingIndex() - 4);
                    }

                    menu.SetActive(!menu.activeSelf);
                }
            }
            finally
            {
                PatchPlayerInputActive.skipOverlayActiveCheck = false;
            }
        }

        [HarmonyPatch(typeof(Menu), "IsVisible")]
        class PatchPlayerInputActive
        {
            public static bool skipOverlayActiveCheck = false;

            private static bool Prefix(ref bool __result)
            {
                if (menu != null && menu.gameObject.activeSelf && !skipOverlayActiveCheck)
                {
                    __result = true;
                    return false;
                }

                return true;
            }
        }
    }
}