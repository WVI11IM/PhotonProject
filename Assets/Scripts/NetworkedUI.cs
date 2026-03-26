using Fusion;
using UnityEngine;

public class NetworkedUI : NetworkBehaviour
{
    [SerializeField] private GameObject[] uiElements;

    //On spawn, only activates ui elements if it has input authority
    public override void Spawned()
    {
        foreach (var element in uiElements)
        {
            element.SetActive(Object.HasInputAuthority);
        }
    }
}