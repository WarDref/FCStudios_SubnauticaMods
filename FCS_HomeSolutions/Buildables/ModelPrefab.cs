﻿using System;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;


namespace FCS_HomeSolutions.Buildables
{
    internal static class ModelPrefab
    {
        private static bool _initialized;
        internal static GameObject ColorItemPrefab { get; set; }
        internal static GameObject ItemPrefab { get; set; }
        internal static string BodyMaterial => $"{Mod.ModName}_COL";
        internal static string SecondaryMaterial => $"{Mod.ModName}_COL_S";
        internal static string DecalMaterial => $"{Mod.ModName}_DECALS";
        internal static string DetailsMaterial => $"{Mod.ModName}_DETAILS";
        internal const string CurtainDecalMaterial = "CurtainPackTemplate_Decal";
        internal static string SpecTexture => $"{Mod.ModName}_S";
        internal static string LUMTexture => $"{Mod.ModName}_E";
        internal static string EmissionControllerMaterial => $"{Mod.ModName}_E_Controller";
        internal static string NormalTexture => $"{Mod.ModName}_N";
        internal static string DetailTexture => $"{Mod.ModName}_D";
        public static AssetBundle GlobalBundle { get; set; }
        public static AssetBundle ModBundle { get; set; }
        internal static GameObject PaintToolPrefab { get; set; }
        internal static GameObject BaseOperatorPrefab { get; set; }
        public static GameObject HoverLiftPadPrefab { get; set; }
        public static GameObject SmallOutdoorPot { get; set; }
        public static GameObject MiniFountainFilterPrefab { get; set; }
        public static GameObject SeaBreezeItemPrefab { get; set; }
        public static GameObject SeaBreezePrefab { get; set; }
        public static GameObject TrashReceptaclePrefab { get; set; }
        public static GameObject TrashRecyclerPrefab { get; set; }
        public static GameObject PaintCanPrefab { get; set; }
        public static GameObject QuantumTeleporterPrefab { get; set; }
        public static GameObject NetworkItemPrefab { get; set; }
        public static GameObject TemplateItem { get; set; }
        public static GameObject TrashRecyclerItemPrefab { get; set; }
        public static GameObject CurtainPrefab { get; set; }
        public static GameObject AlienChefPrefab { get; set; }
        public static GameObject Cabinet1Prefab { get; set; }
        public static GameObject Cabinet2Prefab { get; set; }
        public static GameObject Cabinet3Prefab { get; set; }
        public static GameObject CookerItemPrefab { get; set; }
        public static GameObject CookerOrderItemPrefab { get; set; }
        public static GameObject LedLightLongPrefab { get; set; }
        public static GameObject LedLightWallPrefab { get; set; }
        public static GameObject LedLightShortPrefab { get; set; }
        public static GameObject ObservationTankPrefab { get; set; }
        public static GameObject FireExtinguisherRefuelerPrefab { get; set; }
        public static GameObject AlterraMiniBathroomPrefab { get; set; }

        internal static void Initialize()
        {
            if (_initialized) return;

            if (GlobalBundle == null)
            {
                GlobalBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(FCSAssetBundlesService.PublicAPI.GlobalBundleName);
            }

            if (ModBundle == null)
            {
                ModBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(Mod.ModBundleName, Mod.GetModDirectory());
            }

            PaintToolPrefab = GetPrefab(Mod.PaintToolPrefabName);
            SmallOutdoorPot = GetPrefab(Mod.SmartPlanterPotPrefabName);
            BaseOperatorPrefab = GetPrefab(Mod.BaseOperatorPrefabName);
            HoverLiftPadPrefab = GetPrefab(Mod.HoverLiftPrefabName);
            MiniFountainFilterPrefab = GetPrefab(Mod.MiniFountainFilterPrefabName);
            SeaBreezePrefab = GetPrefab(Mod.SeaBreezePrefabName);
            TrashReceptaclePrefab = GetPrefab(Mod.TrashReceptaclePrefabName);
            TrashRecyclerPrefab = GetPrefab(Mod.RecyclerPrefabName);
            PaintCanPrefab = GetPrefab(Mod.PaintCanPrefabName);
            QuantumTeleporterPrefab = GetPrefab(Mod.QuantumTeleporterPrefabName);
            AlienChefPrefab = GetPrefab(Mod.AlienChefPrefabName);
            Cabinet1Prefab = GetPrefab(Mod.Cabinet1PrefabName);
            Cabinet2Prefab = GetPrefab(Mod.Cabinet2PrefabName);
            Cabinet3Prefab = GetPrefab(Mod.Cabinet3PrefabName);
            AlterraMiniBathroomPrefab = GetPrefab(Mod.AlterraMiniBathroomPrefabName,true);
            FireExtinguisherRefuelerPrefab = GetPrefab(Mod.FireExtinguisherRefuelerPrefabName);
            ObservationTankPrefab = GetPrefab(Mod.EmptyObservationTankPrefabName);
            TrashRecyclerItemPrefab = GetPrefab("RecyclerItem");
            SeaBreezeItemPrefab = GetPrefab("ARSItem");
            NetworkItemPrefab = GetPrefab("NetworkItem");
            TemplateItem = GetPrefab("TemplateItem");
            CurtainPrefab = GetPrefab("Curtain");
            CookerItemPrefab = GetPrefab("CookerItem");
            CookerOrderItemPrefab = GetPrefab("OrderItem");
            LedLightLongPrefab = GetPrefab("FCS_LedLightStick_03");
            LedLightShortPrefab = GetPrefab("FCS_LedLightStick_01");
            LedLightWallPrefab = GetPrefab("FCS_LedLightStick_02");
            _initialized = true;
        }
        
        internal static GameObject GetPrefab(string prefabName, bool isV2 = false)
        {
            try
            {
                GameObject prefabGo;

                QuickLogger.Debug($"Getting Prefab: {prefabName}");
                if (isV2)
                {
                    if (!LoadAssetV2(prefabName, ModBundle, out prefabGo)) return null;
                }
                else
                {
                    if (!LoadAsset(prefabName, ModBundle, out prefabGo)) return null;
                }
                
                return prefabGo;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }
        
        private static bool LoadAsset(string prefabName, AssetBundle assetBundle, out GameObject go, bool applyShaders = true)
        {
            QuickLogger.Debug("Loading Asset");
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(prefabName);
            QuickLogger.Debug($"Loaded Prefab {prefabName}");

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                if (applyShaders)
                {
                    //Lets apply the material shader
                    ApplyShaders(prefab, assetBundle);
                    QuickLogger.Debug($"Applied shaderes to prefab {prefabName}");
                }

                go = prefab;
                QuickLogger.Debug($"{prefabName} Prefab Found!");
                return true;
            }

            QuickLogger.Error($"{prefabName} Prefab Not Found!");

            go = null;
            return false;
        }


        private static bool LoadAssetV2(string prefabName, AssetBundle assetBundle, out GameObject go, bool applyShaders = true)
        {
            QuickLogger.Debug("Loading Asset");
            //We have found the asset bundle and now we are going to continue by looking for the model.
            GameObject prefab = assetBundle.LoadAsset<GameObject>(prefabName);
            QuickLogger.Debug($"Loaded Prefab {prefabName}");

            //If the prefab isn't null lets add the shader to the materials
            if (prefab != null)
            {
                if (applyShaders)
                {
                    //Lets apply the material shader
                    AlterraHub.ApplyShadersV2(prefab,assetBundle);
                    QuickLogger.Debug($"Applied shaderes to prefab {prefabName}");
                }

                go = prefab;
                QuickLogger.Debug($"{prefabName} Prefab Found!");
                return true;
            }

            QuickLogger.Error($"{prefabName} Prefab Not Found!");

            go = null;
            return false;
        }



        /// <summary>
        /// Applies the shader to the materials of the reactor
        /// </summary>
        /// <param name="prefab">The prefab to apply shaders.</param>
        internal static void ApplyShaders(GameObject prefab, AssetBundle bundle = null)
        {
            #region BaseColor
            MaterialHelpers.ApplySpecShader(BodyMaterial, SpecTexture, prefab, 1, 3f, bundle);
            MaterialHelpers.ApplyEmissionShader(DecalMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyEmissionShader(DetailsMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyEmissionShader(EmissionControllerMaterial, LUMTexture, prefab, bundle, Color.white);
            MaterialHelpers.ApplyAlphaShader(DecalMaterial, prefab);
            MaterialHelpers.ApplyAlphaShader(DetailsMaterial, prefab);
            MaterialHelpers.ApplyAlphaShader(CurtainDecalMaterial, prefab);
            #endregion
        }

        public static Texture2D GetImageFromPrefab(string imageName)
        {
            var prefab = ModBundle.LoadAsset<Texture2D>(imageName);
            return prefab != null ? prefab : null;
        }
    }
}
