using Fusion;
using System.Collections;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [Networked] public string playerName { get; set; }
    [Networked] public Role playerRole { get; set; }

    public override void Spawned()
    {
        //Only the local client sets its information
        if (Object.HasInputAuthority)
        {
            StartCoroutine(RegisterLocalPlayerNextFrame());
        }
    }
    private IEnumerator RegisterLocalPlayerNextFrame()
    {
        //Wait one frame
        yield return null;

        if (NetworkRunnerHandler.Instance != null)
        {
            NetworkRunnerHandler.Instance.SetLocalPlayerInfo(playerName, playerRole);
        }
    }
}
