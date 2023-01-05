using JetBrains.Annotations;
using UnityEngine;

namespace AdditionalMaps.MonoBehaviours;

internal class MappedCustomRoom : MonoBehaviour
{
    [UsedImplicitly]
    private void Awake()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            t.gameObject.SetActive(true);
        }
    }
}