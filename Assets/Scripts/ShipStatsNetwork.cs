using Fusion;
using Pilot;
using Pilot.Ship;
using UnityEngine;

public class ShipStatsNetwork : NetworkBehaviour
{
    [SerializeField] private ShipStats stats;
    public float Fuel => FuelCurrent;
    public float Ammo => AmmoCurrent;
    public float Hull => HullCurrent;
    [Networked] private float FuelCurrent { get; set; }
    [Networked] private float AmmoCurrent { get; set; }
    [Networked] private float HullCurrent { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            stats.Fuel.Initialize();
            stats.Ammo.Initialize();
            stats.Hull.Initialize();

            //Sync initial values into network state
            FuelCurrent = stats.Fuel.Current;
            AmmoCurrent = stats.Ammo.Current;
            HullCurrent = stats.Hull.Current;
        }
    }

    public void PushFromStats(ItemType type)
    {
        var res = stats.TypeToResource(type);

        switch (type)
        {
            case ItemType.Fuel: FuelCurrent = res.Current; break;
            case ItemType.Ammo: AmmoCurrent = res.Current; break;
            case ItemType.Metal: HullCurrent = res.Current; break;
        }
    }

    public override void FixedUpdateNetwork()
    {
        //Continuously push the authoritative stat values into network state
        if (Object.HasStateAuthority)
        {
            FuelCurrent = stats.Fuel.Current;
            AmmoCurrent = stats.Ammo.Current;
            HullCurrent = stats.Hull.Current;
        }
    }

    public void Replenish(ItemType type)
    {
        if (!Object.HasStateAuthority)
        {
            // Forward request to state authority
            RPC_Replenish(type);
            return;
        }

        var res = stats.TypeToResource(type);

        //Apply change to gameplay
        res.Replenish();
        //Sync to network
        Push(type, res.Current);
    }
    public void Penalty(ItemType type)
    {
        // Forward request to state authority
        if (!Object.HasStateAuthority)
        {
            RPC_Penalty(type);
            return;
        }

        var res = stats.TypeToResource(type);

        //Apply change to gameplay
        res.Penalty();
        //Sync to network
        Push(type, res.Current);
    }

    void Push(ItemType type, float value)
    {
        switch (type)
        {
            case ItemType.Fuel: FuelCurrent = value; break;
            case ItemType.Ammo: AmmoCurrent = value; break;
            case ItemType.Metal: HullCurrent = value; break;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_Replenish(ItemType type) => Replenish(type);

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_Penalty(ItemType type) => Penalty(type);

}