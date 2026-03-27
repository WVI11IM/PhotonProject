using Fusion;
using System;

[Serializable]
public class NetworkPlayerData
{
    public string playerName;
    public Role playerRole;
    public PlayerRef playerRef;

    public NetworkPlayerData(string name, Role role, PlayerRef refPlayer)
    {
        playerName = name;
        playerRole = role;
        playerRef = refPlayer;
    }
}