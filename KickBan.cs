using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private void Awake()
        {
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

        [HarmonyPatch(typeof(ConnectPanel), "IsVisible")]
        public static class ConnectPanel_Patch
        {
            public static void Prefix(ConnectPanel __instance)
            {

                var customkickmenu = Object.Instantiate(customcanvas, __instance.gameObject.transform, false);
                var scrollbar = customkickmenu.GetComponentInChildren<Scrollbar>();
                scrollbar = __instance.m_playerListScroll;
                var text = customkickmenu.GetComponentInChildren<Text>().text;
                text = __instance.m_activePeers.ToString();
                customkickmenu.SetActive(true);
            }
        }
        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}