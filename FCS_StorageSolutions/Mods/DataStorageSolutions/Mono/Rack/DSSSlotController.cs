﻿using System;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Server;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack
{
    internal class DSSSlotController : MonoBehaviour, IFCSDumpContainer, IHandTarget,ISlotController
    {
        private DumpContainerSimplified _dumpContainer;
        private InventoryItem _inventoryItem;
        private IDSSRack _controller;
        private string _slotName;
        private BoxCollider _collider;
        private FCSStorage _storage;
        private DSSServerController _mountedServer;
        private Image _preloader;
        private Text _preloaderPercentage;
        public bool IsOccupied => _mountedServer != null;
        public bool IsFull => _mountedServer?.IsFull() ?? true;

        private void Start()
        {
            _storage?.CleanUpDuplicatedStorageNoneRoutine();
        }

        private void Update()
        {
            if (_collider == null || _controller == null || _preloader == null || _preloaderPercentage == null) return;
            _collider.isTrigger = !_controller.IsOpen;

            if (_mountedServer != null)
            {
                _preloader.fillAmount = _mountedServer.GetPercentage();
                _preloaderPercentage.text = $"{_mountedServer.GetPercentage():P0}";
            }
            
        }

        internal void Initialize(string slotName, IDSSRack controller,GameObject preloader)
        {
            _preloader = preloader.GetComponent<Image>();
            _preloaderPercentage = preloader.GetComponentInChildren<Text>();
            _collider = gameObject.GetComponent<BoxCollider>();
            _controller = controller;
            _slotName = slotName;
            if (_dumpContainer == null)
            {
                _dumpContainer = gameObject.EnsureComponent<DumpContainerSimplified>();
                _dumpContainer.Initialize(transform,$"Add server to {_slotName}",this,1,1,gameObject.name);
            }


            if (_storage == null)
            {
                _storage = gameObject.AddComponent<FCSStorage>();
                _storage.Initialize(1,gameObject);
                _storage.ItemsContainer.onAddItem += ItemsContainerOnOnAddItem;
                _storage.ItemsContainer.onRemoveItem += ItemsContainerOnRemoveItem;
            }
        }

        private void ItemsContainerOnRemoveItem(InventoryItem item)
        {
            var server = item.item.gameObject.EnsureComponent<DSSServerController>();
            if (server == null)
            {
                QuickLogger.DebugError($"Server controller returned null on ItemsContainerOnOnAddItem. Object {item.item.gameObject.name}");
                return;
            }
            server.UnDockServer();
            _controller.Manager.RemoveServerFromBase(_mountedServer);
            _mountedServer.GetStorage().ItemsContainer.onAddItem -= OnMountedServerUpdate;
            _mountedServer.GetStorage().ItemsContainer.onRemoveItem -= OnMountedServerUpdate;
            _mountedServer = null;
            _inventoryItem = null;
        }

        private void ItemsContainerOnOnAddItem(InventoryItem item)
        {
            if(item== null || item.item ==null) return;
            ModelPrefab.ApplyShaders(item.item.gameObject);
            var result = AddItemToContainer(item);
            if(result)
            {
                _mountedServer.GetStorage().ItemsContainer.onAddItem += OnMountedServerUpdate;
                _mountedServer.GetStorage().ItemsContainer.onRemoveItem += OnMountedServerUpdate;
            }
        }

        private void OnMountedServerUpdate(InventoryItem item)
        {
            _controller.UpdateStorageCount();
        }

        public void OnHandHover(GUIHand hand)
        {
            if(_controller.IsOpen)
            {
                HandReticle main = HandReticle.main;
                main.SetIcon(HandReticle.IconType.Hand);
                main.SetInteractText(IsOccupied ? $"Remove server from {_slotName}" : $"Add server to {_slotName}");
            }
        }
        
        public void OnHandClick(GUIHand hand)
        {
            if (!_controller.IsOpen) return;

            FindServer();

            if (IsOccupied)
            {
                PlayerInteractionHelper.GivePlayerItem(_inventoryItem);
                _mountedServer?.UnDockServer();
                _mountedServer = null;
                _inventoryItem = null;
                _preloader.fillAmount =0;
                _preloaderPercentage.text = $"{0:P0}";
                _controller.UpdateStorageCount();
            }
            else
            {
                _dumpContainer.OpenStorage();
            }
        }

        /// <summary>
        /// Event when the DumpContainer has an item added
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool AddItemToContainer(InventoryItem item)
        {
            try
            {
                if (item.item.GetTechType() != Mod.GetDSSServerTechType()) return false;
                _inventoryItem = item;
                _mountedServer = item.item.gameObject.GetComponentInChildren<DSSServerController>();
                _controller?.Manager?.RegisterServerInBase(_mountedServer);
                _mountedServer.DockServer(this, _controller);
                _controller.UpdateStorageCount();
            }
            catch (Exception e)
            {
                
                QuickLogger.Error("Please contact FCStudios about this fail",true);
                QuickLogger.DebugError(e.Message);
                QuickLogger.DebugError(e.StackTrace);
                PlayerInteractionHelper.GivePlayerItem(item);
                return false;
            }

            return true;
        }

        public bool AddItemMountedItem(InventoryItem item)
        {
            return _mountedServer.AddItemToContainer(item);
        }

        /// <summary>
        /// Dump Container calls this method to see if this item can be added to the container.
        /// </summary>
        /// <param name="pickupable"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return pickupable.GetTechType() == Mod.GetDSSServerTechType();
        }

        public string GetSlotName()
        {
            return _slotName;
        }

        public byte[] Save(ProtobufSerializer serializer)
        {
            return _storage.Save(serializer);
        }

        public void RestoreItems(ProtobufSerializer serializer,byte[] data)
        {
            _storage.RestoreItems(serializer,data);
        }

        internal void SetIsVisible(bool value)
        {
            FindServer();

            if (_mountedServer != null)
            {
                _mountedServer.IsVisible = value;
            }
        }

        public int GetStorageAmount()
        {
            FindServer();

            if (_mountedServer == null)
            {
                return 0;
            }

            return _mountedServer.GetCount();
        }

        private void FindServer()
        {
            if (_mountedServer == null)
            {
                if (_inventoryItem != null)
                {
                    _mountedServer = _inventoryItem.item.gameObject.GetComponentInChildren<DSSServerController>();
                }
            }
        }

        public bool HasSpace(int amount)
        {
            return _mountedServer.HasSpace(amount);
        }

        public int GetFreeSpace()
        {
            if (_mountedServer == null) return 0;

            return _mountedServer.GetFreeSpace();
        }

        public bool IsTechTypeAllowed(TechType techType)
        {
            if (_mountedServer == null) return false;
            return _mountedServer.IsTechTypeAllowed(techType);
        }

        public int GetItemCount(TechType techType)
        {
            return _mountedServer.GetItemCount(techType);
        }

        public bool HasItem(TechType techType)
        {
            if (_mountedServer == null) return false;
            return _mountedServer.HasItem(techType);
        }

        public Pickupable RemoveItemFromServer(TechType techType)
        {
            if (_mountedServer == null) return null;
            return _mountedServer.RemoveItemFromContainer(techType,1);
        }

        public FcsDevice GetServer()
        {
            return _mountedServer;
        }
    }
}