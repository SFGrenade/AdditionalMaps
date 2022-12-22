using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace AdditionalMaps.MonoBehaviours;

internal class MappedCustomRoom : MonoBehaviour
{
    private PlayerData _pd;

    [UsedImplicitly]
    private void Start()
    {
        _pd = PlayerData.instance;
    }

    [UsedImplicitly]
    private void OnEnable()
    {
        bool ret = true;
        if (_pd == null) _pd = PlayerData.instance;
        if (!_pd.GetVariable<List<string>>(nameof(PlayerData.scenesMapped)).Contains(transform.name))
        {
            if (!_pd.GetBool(nameof(PlayerData.mapAllRooms))) ret = false;
        }
        if (!_pd.GetVariable<List<string>>(nameof(PlayerData.scenesVisited)).Contains(transform.name))
        {
            if (!_pd.GetBool(nameof(PlayerData.mapAllRooms))) ret = false;
        }
        for (var i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            t.gameObject.SetActive(ret);
        }
    }
}