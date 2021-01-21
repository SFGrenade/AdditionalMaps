using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GlobalEnums;
using Modding;
using ModCommon;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Security.Cryptography;
using AdditionalMaps.Consts;
using AdditionalMaps.Utils;
using SFCore;
using TMPro;
using On;
using Logger = Modding.Logger;
using UnityEngine.SceneManagement;
using ModCommon.Util;
using UObject = UnityEngine.Object;

namespace AdditionalMaps.MonoBehaviours
{
    class MappedCustomRoom : Component
    {
        public PlayerData pd;
        private GameManager gm;
		public bool fullSpriteDisplayed;

		private void Start()
		{
			this.gm = GameManager.instance;
			this.pd = PlayerData.instance;
		}

		private void OnEnable()
		{
			if (this.gm == null)
			{
				this.gm = GameManager.instance;
			}
			if (!this.fullSpriteDisplayed && (this.gm.playerData.scenesMapped.Contains(base.transform.name) || this.gm.playerData.mapAllRooms))
			{
				for (int i = 0; i < this.transform.childCount; i++)
				{
					var t = this.transform.GetChild(i);
					t.gameObject.SetActive(true);
				}
				this.fullSpriteDisplayed = true;
			}
		}
	}
}
