using JetBrains.Annotations;
using UnityEngine;

namespace AdditionalMaps.MonoBehaviours;

internal class MappedCustomRoom : MonoBehaviour
{
    private GameManager _gm;

    [UsedImplicitly]
    private void Start()
    {
        _gm = GameManager.instance;
    }

    [UsedImplicitly]
    private void OnEnable()
    {
        bool ret = true;
        if (_gm == null) _gm = GameManager.instance;
        if (!_gm.playerData.scenesMapped.Contains(transform.name))
        {
            if (!_gm.playerData.mapAllRooms) ret = false;
        }
        if (!_gm.playerData.scenesVisited.Contains(transform.name))
        {
            if (!_gm.playerData.mapAllRooms) ret = false;
        }
        for (var i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            t.gameObject.SetActive(ret);
        }
    }
}