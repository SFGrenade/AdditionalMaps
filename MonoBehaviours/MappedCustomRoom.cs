using UnityEngine;

namespace AdditionalMaps.MonoBehaviours
{
    internal class MappedCustomRoom : Component
    {
        public bool fullSpriteDisplayed;
        private GameManager _gm;
        public PlayerData pd;

        private void Start()
        {
            _gm = GameManager.instance;
            pd = PlayerData.instance;
        }

        private void OnEnable()
        {
            if (_gm == null) _gm = GameManager.instance;
            if (fullSpriteDisplayed ||
                (!_gm.playerData.scenesMapped.Contains(transform.name) && !_gm.playerData.mapAllRooms)) return;
            for (var i = 0; i < transform.childCount; i++)
            {
                var t = transform.GetChild(i);
                t.gameObject.SetActive(true);
            }

            fullSpriteDisplayed = true;
        }
    }
}