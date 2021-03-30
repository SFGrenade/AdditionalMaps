using UnityEngine;

namespace AdditionalMaps.MonoBehaviours
{
    internal class MappedCustomRoom : Component
    {
        public bool fullSpriteDisplayed;
        private GameManager gm;
        public PlayerData pd;

        private void Start()
        {
            gm = GameManager.instance;
            pd = PlayerData.instance;
        }

        private void OnEnable()
        {
            if (gm == null) gm = GameManager.instance;
            if (!fullSpriteDisplayed &&
                (gm.playerData.scenesMapped.Contains(transform.name) || gm.playerData.mapAllRooms))
            {
                for (var i = 0; i < transform.childCount; i++)
                {
                    var t = transform.GetChild(i);
                    t.gameObject.SetActive(true);
                }

                fullSpriteDisplayed = true;
            }
        }
    }
}