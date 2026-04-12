using Fusion;
using Pilot.Ship;
using UnityEngine;
using UnityEngine.UI;

public class EngineerShipStatSliders : MonoBehaviour
{
    [SerializeField] private EngineerItemSender sender;

    private ShipStatsNetwork network;

    [SerializeField] private Slider fuelSlider;
    [SerializeField] private Slider ammoSlider;
    [SerializeField] private Slider hullSlider;

    void Update()
    {
        if (network == null)
        {
            if (sender == null || sender.PilotObject == null) return;

            network = sender.PilotObject.GetComponentInChildren<ShipStatsNetwork>();
            if (network == null) return;
        }

        fuelSlider.value = network.Fuel / 100f;
        ammoSlider.value = network.Ammo / 100f;
        hullSlider.value = network.Hull / 100f;
    }
}