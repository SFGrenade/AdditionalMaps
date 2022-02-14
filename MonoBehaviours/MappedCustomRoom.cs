using JetBrains.Annotations;
using UnityEngine;

namespace AdditionalMaps.MonoBehaviours;

internal class MappedCustomRoom : Component
{
    private bool _fullSpriteDisplayed;
    private GameManager _gm;

    [UsedImplicitly]
    private void Start()
    {
        _gm = GameManager.instance;
    }

    [UsedImplicitly]
    private void OnEnable()
    {
        if (_gm == null) _gm = GameManager.instance;
        if (_fullSpriteDisplayed || (!_gm.playerData.scenesMapped.Contains(transform.name) && !_gm.playerData.mapAllRooms)) return;
        for (var i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            t.gameObject.SetActive(true);
        }

        _fullSpriteDisplayed = true;
    }
}