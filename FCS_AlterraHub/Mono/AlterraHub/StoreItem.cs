﻿using System;
using FCS_AlterraHub.Enumerators;
using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class StoreItem : MonoBehaviour
    {
        private  float _price;

        internal void Initialize(string objectName,TechType techType, TechType receiveTechType, float cost, Action<TechType,TechType> callback,StoreCategory category)
        {
            _price = cost;
            var objectNameObj = GameObjectHelpers.FindGameObject(gameObject, "ObjectText").GetComponent<Text>();
            objectNameObj.text = objectName;

            var costObj = GameObjectHelpers.FindGameObject(gameObject, "CostText").GetComponent<Text>();
            costObj.text = cost.ToString("n0");

            var addToCartBTN = gameObject.GetComponentInChildren<Button>();
            addToCartBTN.onClick.AddListener(() =>
            {
                callback?.Invoke(techType,receiveTechType);
            });

            var icon = GameObjectHelpers.FindGameObject(gameObject, "Icon");
            var uGUIIcon = icon.AddComponent<uGUI_Icon>();
            uGUIIcon.sprite = SpriteManager.Get(techType);
        }

        internal float GetPrice()
        {
            return _price;
        }
    }
}