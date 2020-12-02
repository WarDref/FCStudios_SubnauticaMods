﻿using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.ObjectPooler;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.DeepDriller.Buildable;
using FCS_ProductionSolutions.DeepDriller.Configuration;
using FCS_ProductionSolutions.DeepDriller.Enumerators;
using FCS_ProductionSolutions.DeepDriller.Structs;
using FCSCommon.Abstract;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.DeepDriller.Mono
{
    internal class FCSDeepDrillerDisplay : AIDisplay
    {
        private FCSDeepDrillerController _mono;
        private bool _isInitialized;
        private readonly Color _startColor = Color.grey;
        private readonly Color _hoverColor = Color.white;
        private readonly Color _colorEmpty = new Color(1f, 0f, 0f, 1f);
        private readonly Color _colorHalf = new Color(1f, 1f, 0f, 1f);
        private readonly Color _colorFull = new Color(0f, 1f, 0f, 1f);
        private int _pageHash;
        private Image _batteryFill;
        private Text _batteryPercentage;
        private Image _oilFill;
        private Text _oresPerDay;
        private Text _powerUsage;
        private GridHelperPooled _inventoryGrid;
        private GridHelper _filterGrid;
        private GridHelper _programmingGrid;
        private bool _isBeingDestroyed;
        private Text _filterBTNText;
        private readonly Dictionary<TechType, FCSToggleButton> _trackedFilterState = new Dictionary<TechType, FCSToggleButton>();
        private Text _itemCounter;
        private Text _unitID;
        private Text _batteryStatus;
        private Text _oilPercentage;
        private FCSToggleButton _filterToggle;
        private FCSToggleButton _filterBlackListToggle;
        private Text _statusLabel;
        private FCSToggleButton _alterraRangeToggle;
        private FCSToggleButton _alterraStorageToggle;
        private GridHelperPooled _alterraStorageGrid;
        private ObjectPooler _pooler;
        private HashSet<DrillInventoryButton> _trackedItems = new HashSet<DrillInventoryButton>();
        private InterfaceInteraction _interfaceInteraction;
        private const string AlterraStoragePoolTag = "AlterraStorage";
        private const string InventoryPoolTag = "Inventory";

        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }

        internal void Setup(FCSDeepDrillerController mono)
        {
            _mono = mono;
            _pageHash = Animator.StringToHash("Page");

            if (FindAllComponents())
            {
                _isInitialized = true;
                _mono.DeepDrillerPowerManager.OnBatteryUpdate += OnBatteryUpdate;
                _mono.DeepDrillerContainer.OnContainerUpdate += OnContainerUpdate;
                _mono.UpgradeManager.OnUpgradeUpdate += OnUpgradeUpdate;
                _inventoryGrid.DrawPage(1);
                _filterGrid.DrawPage(1);
                UpdateDisplayValues();
                PowerOnDisplay();
                //RecheckFilters();
                InvokeRepeating(nameof(Updater), 0.5f, 0.5f);
            }
        }

        private void OnContainerUpdate(int arg1, int arg2)
        {
            _inventoryGrid.DrawPage();
        }

        private void Updater()
        {
            UpdateOilLevel();
        }

        internal void UpdateUnitID()
        {
            if (!string.IsNullOrWhiteSpace(_mono.UnitID) && _unitID != null &&
                string.IsNullOrWhiteSpace(_unitID.text))
            {
                QuickLogger.Debug("Setting Unit ID", true);
                _unitID.text = $"UnitID: {_mono.UnitID}";
            }
        }

        internal void UpdateDisplayValues()
        {
            _powerUsage.text = _mono.DeepDrillerPowerManager.GetPowerUsage().ToString();
            _oresPerDay.text = _mono.OreGenerator.GetItemsPerDay();
        }
        
        internal bool IsInteraction()
        {
            return _interfaceInteraction.IsInRange;
        }

        private void OnBatteryUpdate(PowercellData data)
        {
            UpdateBatteryStatus(data);
        }

        public override void PowerOnDisplay()
        {
            QuickLogger.Debug("Powering On Display!", true);
            GotoPage(FCSDeepDrillerPages.Boot);
        }

        public override void PowerOffDisplay()
        {
            QuickLogger.Debug("Powering Off Display!", true);
            _mono.AnimationHandler.SetIntHash(_pageHash, 6);
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "InventoryBTN":
                    GotoPage(FCSDeepDrillerPages.Inventory);
                    break;
                case "ProgramBTN":
                    GotoPage(FCSDeepDrillerPages.Programming);
                    break;
                case "ProgrammingAddBTN":
                    _mono.UpgradeManager.Show();
                    break;
                case "SettingsBTN":
                    GotoPage(FCSDeepDrillerPages.Settings);
                    break;
                case "ExStorageBTN":
                    GotoPage(FCSDeepDrillerPages.AlterraStorage);
                    break;
                case "HomeBTN":
                    GotoPage(FCSDeepDrillerPages.Home);
                    break;
                case "ToggleRangeBTN":
                    _mono.ToggleRangeView();
                    break;
                case "ToggleFilterBTN":
                    QuickLogger.Debug("Toggling Filter", true);
                    _mono.OreGenerator.ToggleFocus();
                    break;
                case "FilterPageBTN":
                    GotoPage(FCSDeepDrillerPages.Filter);
                    break;
                case "ExportToggleBTN":
                    QuickLogger.Debug("Export Toggle", true);
                    _mono.TransferManager.Toggle();
                    break;
                case "PowercellDrainBTN":
                    QuickLogger.Debug("Opening powercell dump container", true);
                    _mono.PowercellDumpContainer.OpenStorage();
                    break;
                case "ItemBTN":
                    var item = (TechType)tag;
                    _mono.DeepDrillerContainer.RemoveItemFromContainer(item);
                    break;
                case "FilterToggleBTN":
                    var data = (FilterBtnData)tag;
                    QuickLogger.Debug($"Toggle for {data.TechType} is {data.Toggle.IsSelected}", true);

                    if (data.Toggle.IsSelected)
                    {
                        _mono.OreGenerator.AddFocus(data.TechType);
                    }
                    else
                    {
                        _mono.OreGenerator.RemoveFocus(data.TechType);
                    }

                    break;
                case "LubeRefillBTN":
                    QuickLogger.Debug("Opening Lube Drop Container", true);
                    _mono.OilDumpContainer.OpenStorage();
                    break;
                case "SettingsBackBTN":
                    GotoPage(FCSDeepDrillerPages.Home);
                    break;
                case "FilterBackBTN":
                    GotoPage(FCSDeepDrillerPages.Settings);
                    break;
                case "AlterraStorageBackBTN":
                    GotoPage(FCSDeepDrillerPages.Settings);
                    break;
                case "ProgrammingBackBTN":
                    GotoPage(FCSDeepDrillerPages.Settings);
                    break;
                case "ToggleBlackListBTN":
                    _mono.OreGenerator.SetBlackListMode(((FilterBtnData)tag).Toggle.IsSelected);
                    break;
            }
        }

        internal void GotoPage(FCSDeepDrillerPages page)
        {
            _mono.AnimationHandler.SetIntHash(_pageHash, (int)page);
        }

        public override bool FindAllComponents()
        {
            try
            {

                if (_pooler == null)
                {
                    _pooler = gameObject.AddComponent<ObjectPooler>();
                    _pooler.AddPool(AlterraStoragePoolTag, 6, ModelPrefab.DeepDrillerOreBTNPrefab);
                    _pooler.AddPool(InventoryPoolTag, 12, ModelPrefab.DeepDrillerItemPrefab);
                    _pooler.Initialize();
                }

                #region Canvas  
                var canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;
                _interfaceInteraction = canvasGameObject.AddComponent<InterfaceInteraction>();
                if (canvasGameObject == null)
                {
                    QuickLogger.Error("Canvas cannot be found");
                    return false;
                }
                #endregion

                #region Home
                var homePage = InterfaceHelpers.FindGameObject(canvasGameObject, "Home");
                #endregion

                #region Inventory Page
                var inventoryPage = InterfaceHelpers.FindGameObject(canvasGameObject, "InventoryPage");
                #endregion

                #region Alterra Storage Page
                var alterraStoragePage = InterfaceHelpers.FindGameObject(canvasGameObject, "AlterraStoragePage");
                #endregion

                #region Filter Page
                var filterPage = InterfaceHelpers.FindGameObject(canvasGameObject, "FilterPage");
                #endregion

                #region Settings Page
                var settingsPage = InterfaceHelpers.FindGameObject(canvasGameObject, "Settings");
                #endregion

                #region Programming Page
                var programmingPage = InterfaceHelpers.FindGameObject(canvasGameObject, "ProgrammingPage");
                #endregion

                //================= Statue Label =============//

                #region Status Label

                _statusLabel = InterfaceHelpers.FindGameObject(canvasGameObject, "StatusLabel").GetComponent<Text>();

                #endregion

                //================= Home Page ================//


                #region Inventory Button

                var inventoryBTN = GameObjectHelpers.FindGameObject(homePage, "StorageBTN");
                InterfaceHelpers.CreateButton(inventoryBTN, "InventoryBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.InventoryButton());

                #endregion

                #region Settings Button

                var settingsBTN = GameObjectHelpers.FindGameObject(homePage, "SettingsBTN");
                InterfaceHelpers.CreateButton(settingsBTN, "SettingsBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.SettingsButton());

                #endregion

                #region Battery Meter

                var batteryMeter = InterfaceHelpers.FindGameObject(homePage, "BatteryMeter");
                _batteryFill = batteryMeter?.FindChild("Fill")?.GetComponent<Image>();
                _batteryStatus = batteryMeter?.FindChild("BatteryStatus")?.GetComponent<Text>();
                _batteryStatus.text = $"0/{QPatch.DeepDrillerMk3Configuration.InternalBatteryCapacity}";

                if (_batteryFill != null)
                {
                    _batteryFill.color = _colorEmpty;
                    _batteryFill.fillAmount = 0f;
                }

                _batteryPercentage = batteryMeter?.FindChild("Percentage")?.GetComponent<Text>();



                #endregion

                #region Oil Meter

                var oilMeter = GameObjectHelpers.FindGameObject(homePage, "LubeMeter");
                _oilFill = oilMeter?.FindChild("Fill")?.GetComponent<Image>();
                _oilPercentage = oilMeter?.FindChild("Percentage")?.GetComponent<Text>();
                if (_oilFill != null)
                {
                    _oilFill.color = _colorEmpty;
                    _oilFill.fillAmount = 0f;
                }

                #endregion

                #region Items Per Day
                //_itemsPerDay = GameObjectHelpers.FindGameObject(homePage, "ItemsPerDayLBL")?.GetComponent<Text>();
                #endregion

                #region UnitID

                _unitID = InterfaceHelpers.FindGameObject(homePage, "UnitID").GetComponent<Text>();

                #endregion

                #region OresPerDay

                _oresPerDay = InterfaceHelpers.FindGameObject(homePage, "OresPerDayAmount").GetComponent<Text>();
                var oresPerDayLabel = InterfaceHelpers.FindGameObject(homePage, "OresPerDay").GetComponent<Text>();
                oresPerDayLabel.text = FCSDeepDrillerBuildable.OresPerDay();
                #endregion

                #region Power Consumption

                _powerUsage = InterfaceHelpers.FindGameObject(homePage, "PowerConsumptionAmount").GetComponent<Text>();
                var powerConsumptionLabel = InterfaceHelpers.FindGameObject(homePage, "PowerConsumption").GetComponent<Text>();
                powerConsumptionLabel.text = FCSDeepDrillerBuildable.PowerConsumption();

                #endregion

                #region Power Consumption

                var biome = InterfaceHelpers.FindGameObject(homePage, "Biome").GetComponent<Text>();
                biome.text = FCSDeepDrillerBuildable.BiomeFormat(_mono.CurrentBiome);

                #endregion

                //================= Inventory Page ================//

                #region Inventory Grid

                _inventoryGrid = _mono.gameObject.AddComponent<GridHelperPooled>();
                _inventoryGrid.OnLoadDisplay += OnLoadItemsGrid;
                _inventoryGrid.Setup(12, _pooler, inventoryPage, OnButtonClick);

                #endregion

                _itemCounter = GameObjectHelpers.FindGameObject(inventoryPage, "InventoryLabel")?.GetComponent<Text>();

                //================= Settings Page ================//

                #region Find Unit

                //_unitID = GameObjectHelpers.FindGameObject(homePage, "UnitID")?.GetComponent<Text>();

                #endregion

                #region Filter Button

                var filterBTN = InterfaceHelpers.FindGameObject(settingsPage, "FilterBTN");
                InterfaceHelpers.CreateButton(filterBTN, "FilterPageBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.FilterButton());

                #endregion

                #region Program Button

                var programBTN = GameObjectHelpers.FindGameObject(settingsPage, "ProgramBTN");
                InterfaceHelpers.CreateButton(programBTN, "ProgramBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.ProgrammingButton());

                #endregion

                #region Powercell Drain Button

                var powercellDrainBTN = InterfaceHelpers.FindGameObject(settingsPage, "PowercellDrainBTN");
                InterfaceHelpers.CreateButton(powercellDrainBTN, "PowercellDrainBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.PowercellDrainButton());

                #endregion

                #region Lube Refill Button

                var lubeRefillBTN = InterfaceHelpers.FindGameObject(settingsPage, "LubeRefillBTN");
                InterfaceHelpers.CreateButton(lubeRefillBTN, "LubeRefillBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.LubeRefillButton());

                #endregion

                #region ExStorage Button

                var exStorageBTN = InterfaceHelpers.FindGameObject(settingsPage, "ExStorageBTN");
                InterfaceHelpers.CreateButton(exStorageBTN, "ExStorageBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.AlterraStorageButton());

                #endregion

                #region Setting Back Button

                var backBTN = InterfaceHelpers.FindGameObject(settingsPage, "BackBTN");
                InterfaceHelpers.CreateButton(backBTN, "SettingsBackBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.GoToHome());

                #endregion

                //================= Filter Page ================//

                #region Filter Grid

                _filterGrid = _mono.gameObject.AddComponent<GridHelper>();
                _filterGrid.OnLoadDisplay += OnLoadFilterGrid;
                _filterGrid.Setup(6, ModelPrefab.DeepDrillerOreBTNPrefab, filterPage, _startColor, _hoverColor, OnButtonClick, 5, "PrevBTN", "NextBTN", "Grid", "Paginator", string.Empty);

                #endregion

                #region Filter Back Button

                var filterBackBTN = InterfaceHelpers.FindGameObject(filterPage, "BackBTN");
                InterfaceHelpers.CreateButton(filterBackBTN, "FilterBackBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.GoToSettings());

                #endregion

                #region Filter Toggle Button

                var filterToggleBTN = InterfaceHelpers.FindGameObject(filterPage, "ToggleFilterBTN");
                _filterToggle = filterToggleBTN.AddComponent<FCSToggleButton>();
                _filterToggle.ButtonMode = InterfaceButtonMode.Background;
                _filterToggle.STARTING_COLOR = _startColor;
                _filterToggle.HOVER_COLOR = _hoverColor;
                _filterToggle.BtnName = "ToggleFilterBTN";
                _filterToggle.TextLineOne = FCSDeepDrillerBuildable.FilterButton();
                _filterToggle.OnButtonClick = OnButtonClick;

                #endregion

                #region Filter Blacklist Toggle Button

                var filterBlackListToggleBTN = InterfaceHelpers.FindGameObject(filterPage, "ToggleBlackListBTN");
                _filterBlackListToggle = filterBlackListToggleBTN.AddComponent<FCSToggleButton>();
                _filterBlackListToggle.ButtonMode = InterfaceButtonMode.Background;
                _filterBlackListToggle.STARTING_COLOR = _startColor;
                _filterBlackListToggle.HOVER_COLOR = _hoverColor;
                _filterBlackListToggle.BtnName = "ToggleBlackListBTN";
                _filterBlackListToggle.Tag = new FilterBtnData { Toggle = _filterBlackListToggle };
                _filterBlackListToggle.TextLineOne = FCSDeepDrillerBuildable.BlackListToggle();
                _filterBlackListToggle.TextLineTwo = FCSDeepDrillerBuildable.BlackListToggleDesc();
                _filterBlackListToggle.OnButtonClick = OnButtonClick;

                #endregion

                //================= Alterra Storage Page ================//

                #region Alterra Storage Back Button

                var alterraStorageBackBTN = InterfaceHelpers.FindGameObject(alterraStoragePage, "BackBTN");
                InterfaceHelpers.CreateButton(alterraStorageBackBTN, "AlterraStorageBackBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.GoToSettings());

                #endregion

                #region Alterra Storage Toggle Button

                var alterraStorageToggleBTN = InterfaceHelpers.FindGameObject(alterraStoragePage, "ToggleAlterraStorageBTN");
                _alterraStorageToggle = alterraStorageToggleBTN.AddComponent<FCSToggleButton>();
                _alterraStorageToggle.ButtonMode = InterfaceButtonMode.Background;
                _alterraStorageToggle.STARTING_COLOR = _startColor;
                _alterraStorageToggle.HOVER_COLOR = _hoverColor;
                _alterraStorageToggle.BtnName = "ExportToggleBTN";
                _alterraStorageToggle.TextLineOne = FCSDeepDrillerBuildable.AlterraStorageToggle();
                _alterraStorageToggle.TextLineTwo = FCSDeepDrillerBuildable.AlterraStorageToggleDesc();
                _alterraStorageToggle.OnButtonClick = OnButtonClick;

                #endregion

                #region Alterra Storage Range Button

                var alterraRangeToggleBTN = InterfaceHelpers.FindGameObject(alterraStoragePage, "ToggleRangeBTN");
                _alterraRangeToggle = alterraRangeToggleBTN.AddComponent<FCSToggleButton>();
                _alterraRangeToggle.ButtonMode = InterfaceButtonMode.Background;
                _alterraRangeToggle.STARTING_COLOR = _startColor;
                _alterraRangeToggle.HOVER_COLOR = _hoverColor;
                _alterraRangeToggle.BtnName = "ToggleRangeBTN";
                _alterraRangeToggle.TextLineOne = FCSDeepDrillerBuildable.AlterraStorageRangeToggle();
                _alterraRangeToggle.TextLineTwo = FCSDeepDrillerBuildable.AlterraStorageRangeToggleDesc();
                _alterraRangeToggle.OnButtonClick = OnButtonClick;

                #endregion

                #region Alterra Storage Grid

                _alterraStorageGrid = _mono.gameObject.AddComponent<GridHelperPooled>();
                _alterraStorageGrid.OnLoadDisplay += OnLoadAlterraStorageGrid;
                _alterraStorageGrid.Setup(6, _pooler, alterraStoragePage, OnButtonClick, false);

                #endregion

                //================= Programming Page ================//

                #region Programming Grid

                _programmingGrid = _mono.gameObject.AddComponent<GridHelper>();
                _programmingGrid.OnLoadDisplay += OnLoadProgrammingGrid;
                _programmingGrid.Setup(4, ModelPrefab.DeepDrillerOverrideItemPrefab, programmingPage, _startColor, _hoverColor, OnButtonClick, 5, "PrevBTN", "NextBTN", "Grid", "Paginator", string.Empty);

                #endregion

                #region Programming Back Button

                var programmingBackBTN = InterfaceHelpers.FindGameObject(programmingPage, "BackBTN");
                InterfaceHelpers.CreateButton(programmingBackBTN, "ProgrammingBackBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.GoToSettings());

                #endregion

                #region Programming Add Button

                var programmingAddBTN = InterfaceHelpers.FindGameObject(programmingPage, "AddBTN");
                InterfaceHelpers.CreateButton(programmingAddBTN, "ProgrammingAddBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.AddProgramButton(), FCSDeepDrillerBuildable.AddProgramButtonDec());

                #endregion

                #region Programming Template Button
                var programmingTemplateBTN = InterfaceHelpers.FindGameObject(programmingPage, "TemplateBTN");
                InterfaceHelpers.CreateButton(programmingTemplateBTN, "ProgrammingTemplateBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.ProgrammingTemplateButton(), FCSDeepDrillerBuildable.ProgrammingTemplateButtonDesc());

                #endregion

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Has been caught:");
                QuickLogger.Error($"Message:\n {e.Message}");
                QuickLogger.Error($"StackTrace:\n {e.StackTrace}");
                return false;
            }

            return true;
        }

        private void OnLoadAlterraStorageGrid(DisplayDataPooled data)
        {
            data.Pool.Reset(AlterraStoragePoolTag);

            var grouped = _mono.TransferManager.GetTrackedAlterraStorage();

            if (data.EndPosition > grouped.Count)
            {
                data.EndPosition = grouped.Count;
            }

            QuickLogger.Debug($"Load Items: {grouped.Count} SP:{data.StartPosition} EP:{data.EndPosition}", true);

            for (int i = data.StartPosition; i < data.EndPosition; i++)
            {
                GameObject buttonPrefab = data.Pool.SpawnFromPool(AlterraStoragePoolTag, data.ItemsGrid);

                QuickLogger.Debug($"Button Prefab: {buttonPrefab}");

                if (buttonPrefab == null || data.ItemsGrid == null)
                {
                    return;
                }

                var item = buttonPrefab.EnsureComponent<InterfaceButton>();
                item.ButtonMode = InterfaceButtonMode.Background;
                item.TextLineOne = grouped[i].UnitID;
                item.STARTING_COLOR = Color.gray;
                item.HOVER_COLOR = Color.white;
                uGUI_Icon icon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").EnsureComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(Mod.AlterraStorageTechType());


            }
            _alterraStorageGrid.UpdaterPaginator(grouped.Count);
        }

        internal void RefreshAlterraStorageList()
        {
            _alterraStorageGrid.DrawPage();
        }

        private void OnUpgradeUpdate(UpgradeFunction obj)
        {
            QuickLogger.Debug("Refreshing the Upgrade Page", true);
            UpdateDisplayValues();
            _programmingGrid.DrawPage();

        }

        private void OnLoadProgrammingGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed) return;

                QuickLogger.Debug($"OnLoadProgrammingGrid : {data.ItemsGrid}", true);

                _programmingGrid.ClearPage();

                var grouped = _mono.UpgradeManager.Upgrades;

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {

                    GameObject buttonPrefab = Instantiate(data.ItemsPrefab);

                    if (buttonPrefab == null || data.ItemsGrid == null)
                    {
                        if (buttonPrefab != null)
                        {
                            Destroy(buttonPrefab);
                        }
                        return;
                    }

                    buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                    var upgradeText = buttonPrefab.GetComponentInChildren<Text>();
                    upgradeText.text = grouped.ElementAt(i).Format();

                    var deleteButton = GameObjectHelpers.FindGameObject(buttonPrefab, "DeleteBTN");
                    var deleteBTN = deleteButton.AddComponent<InterfaceButton>();
                    var function = grouped.ElementAt(i);
                    function.Label = upgradeText;
                    deleteBTN.OnButtonClick += (s, o) =>
                    {
                        _mono.UpgradeManager.DeleteFunction(function);
                    };

                    var activateButton = GameObjectHelpers.FindGameObject(buttonPrefab, "EnableToggleBTN");
                    var activateToggleBTN = activateButton.AddComponent<FCSToggleButton>();
                    activateToggleBTN.ButtonMode = InterfaceButtonMode.Background;
                    activateToggleBTN.STARTING_COLOR = _startColor;
                    activateToggleBTN.HOVER_COLOR = _hoverColor;
                    activateToggleBTN.TextLineOne = FCSDeepDrillerBuildable.FilterButton();
                    activateToggleBTN.OnButtonClick += (s, o) =>
                    {
                        function.ToggleUpdate();
                    };
                }

                _programmingGrid.UpdaterPaginator(grouped.Count);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnLoadFilterGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed) return;

                QuickLogger.Debug($"OnLoadFilterGrid : {data.ItemsGrid}");

                if (_trackedFilterState.Count <= 0)
                {
                    //Create all filters
                    var grouped = _mono.OreGenerator.AllowedOres;
                    foreach (TechType techType in grouped)
                    {
                        GameObject buttonPrefab = Instantiate(data.ItemsPrefab);
                        buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                        var itemBTN = buttonPrefab.AddComponent<FCSToggleButton>();
                        itemBTN.ButtonMode = InterfaceButtonMode.Background;
                        itemBTN.STARTING_COLOR = _startColor;
                        itemBTN.HOVER_COLOR = _hoverColor;
                        itemBTN.BtnName = "FilterToggleBTN";
                        itemBTN.TextLineOne = Language.main.Get(techType);
                        itemBTN.Tag = new FilterBtnData { TechType = techType, Toggle = itemBTN };
                        itemBTN.OnButtonClick = OnButtonClick;
                        uGUI_Icon icon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                        icon.sprite = SpriteManager.Get(techType);
                        buttonPrefab.gameObject.SetActive(false);
                        _trackedFilterState.Add(techType, itemBTN);
                    }
                }

                var allowedOres = _mono.OreGenerator.AllowedOres;

                if (data.EndPosition > allowedOres.Count)
                {
                    data.EndPosition = allowedOres.Count;
                }

                foreach (KeyValuePair<TechType, FCSToggleButton> toggle in _trackedFilterState)
                {
                    toggle.Value.SetVisible(false);
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _trackedFilterState.ElementAt(i).Value.SetVisible(true);
                }
                _filterGrid.UpdaterPaginator(allowedOres.Count);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnLoadItemsGrid(DisplayDataPooled data)
        {
            try
            {
                if (_isBeingDestroyed) return;

                var grouped = _mono.DeepDrillerContainer.GetItemsWithin();
                
                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }
                
                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    if(CheckIfButtonIsActive(grouped.ElementAt(i).Key)) {continue;}
                    
                    GameObject buttonPrefab = data.Pool.SpawnFromPool(InventoryPoolTag, data.ItemsGrid);
                    buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                    var itemBTN = buttonPrefab.EnsureComponent<DrillInventoryButton>();
                    itemBTN.ButtonMode = InterfaceButtonMode.Background;
                    itemBTN.STARTING_COLOR = _startColor;
                    itemBTN.HOVER_COLOR = _hoverColor;
                    itemBTN.BtnName = "ItemBTN";
                    itemBTN.TextLineOne = FCSDeepDrillerBuildable.TakeFormatted(Language.main.Get(grouped.ElementAt(i).Key));
                    itemBTN.Tag = grouped.ElementAt(i).Key;
                    itemBTN.RefreshIcon();
                    itemBTN.DrillStorage = _mono.DeepDrillerContainer;
                    itemBTN.OnButtonClick = OnButtonClick;
                    _trackedItems.Add(itemBTN);
                }
                _inventoryGrid.UpdaterPaginator(grouped.Count);
                RefreshStorageAmount();
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private bool CheckIfButtonIsActive(TechType techType)
        {
            foreach (DrillInventoryButton button in _trackedItems)
            {
                if (button.IsValidAndActive(techType))
                {
                    QuickLogger.Debug($"Button is valid: {techType} UpdatingButton",true);
                    button.UpdateAmount();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Updates the checked state of the focus items on the screen
        /// </summary>
        /// <param name="dataFocusOres"></param>
        internal void UpdateListItemsState(HashSet<TechType> dataFocusOres)
        {
            //for (int dataFocusOresIndex = 0; dataFocusOresIndex < dataFocusOres.Count; dataFocusOresIndex++)
            //{
            //    for (int trackedFilterItemsIndex = 0; trackedFilterItemsIndex < TrackedFilterItems.Count; trackedFilterItemsIndex++)
            //    {
            //        var filterData = (FilterBtnData)TrackedFilterItems.ElementAt(trackedFilterItemsIndex).Tag;
            //        if (filterData.TechType == dataFocusOres.ElementAt(dataFocusOresIndex))
            //        {
            //            filterData.Toggle.IsSelected = true;
            //        }
            //    }
            //}
        }

        internal void UpdateBatteryStatus(PowercellData data)
        {
            var charge = data.GetCharge() < 1 ? 0f : data.GetCharge();

            var percent = charge / data.GetCapacity();

            if (_batteryFill != null)
            {
                if (data.GetCharge() >= 0f)
                {
                    var value = (percent >= 0.5f) ? Color.Lerp(this._colorHalf, this._colorFull, 2f * percent - 1f) : Color.Lerp(this._colorEmpty, this._colorHalf, 2f * percent);
                    _batteryFill.color = value;
                    _batteryFill.fillAmount = percent;
                }
                else
                {
                    _batteryFill.color = _colorEmpty;
                    _batteryFill.fillAmount = 0f;
                }
            }

            _batteryPercentage.text = ((data.GetCharge() < 0f) ? Language.main.Get("ChargerSlotEmpty") : $"{Mathf.CeilToInt(percent * 100)}%");
            _batteryStatus.text = $"{Mathf.RoundToInt(data.GetCharge())}/{data.GetCapacity()}";

        }

        internal void UpdateOilLevel()
        {
            var percent = _mono.OilHandler.GetOilPercent();
            _oilFill.fillAmount = percent;
            Color value = (percent >= 0.5f) ? Color.Lerp(this._colorHalf, this._colorFull, 2f * percent - 1f) : Color.Lerp(this._colorEmpty, this._colorHalf, 2f * percent);
            _oilFill.color = value;
            _oilPercentage.text = $"{percent / 1 * 100}%";
        }

        internal void LoadFromSave(DeepDrillerSaveDataEntry save)
        {
            if (save.IsFocused)
            {
                _filterToggle.Select();
            }

            if (save.IsBlackListMode)
            {
                _filterToggle.Select();
            }

            if (save.AllowedToExport)
            {
                _alterraStorageToggle.Select();
            }

            if (save.IsRangeVisible)
            {
                _alterraRangeToggle.Select();
            }

            foreach (KeyValuePair<TechType, FCSToggleButton> toggleButton in _trackedFilterState)
            {
                if (_mono.OreGenerator.GetFocusedOres().Contains(toggleButton.Key))
                {
                    toggleButton.Value.Select();
                }
            }
        }

        internal void RefreshStorageAmount()
        {
            if (_itemCounter == null || _mono?.DeepDrillerContainer == null) return;
            _itemCounter.text = FCSDeepDrillerBuildable.InventoryStorageFormat(_mono.DeepDrillerContainer.GetContainerTotal(), QPatch.DeepDrillerMk3Configuration.StorageSize);
        }

        internal void UpdateStatus(string message)
        {
            _statusLabel.text = message;
        }

        public Text GetStatusField()
        {
            return _statusLabel;
        }
    }
}