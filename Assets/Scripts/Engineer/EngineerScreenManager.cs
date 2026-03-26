using UnityEngine;

public enum EngineerScreen
{
    CargoHold,
    EngineRoom
}

public class EngineerScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject cargoHoldScreen;
    [SerializeField] private GameObject engineRoomScreen;
    [SerializeField] private EngineerSectorSelector sectorSelector;
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSwitchRoom;
    [SerializeField] private AudioSource audioEject;

    public EngineerScreen CurrentScreen { get; private set; } = EngineerScreen.CargoHold;

    public void ShowCargoHoldScreen()
    {
        cargoHoldScreen.SetActive(true);
        engineRoomScreen.SetActive(false);

        CurrentScreen = EngineerScreen.CargoHold;

        //Clears the selected sector from Engine Room
        if (sectorSelector != null)
            sectorSelector.ClearSector();

        //Deletes entire inventories from all scanned cards
        if (inventoryManager != null)
            inventoryManager.ClearAllInventories();

        audioEject.Play();
    }

    public void ShowEngineRoomScreen()
    {
        cargoHoldScreen.SetActive(false);
        engineRoomScreen.SetActive(true);

        CurrentScreen = EngineerScreen.EngineRoom;
    }
    public void SwitchScreen(EngineerScreen screen)
    {
        if (screen == EngineerScreen.CargoHold)
            ShowCargoHoldScreen();
        else
            ShowEngineRoomScreen();

        audioSwitchRoom.Play();
    }
}