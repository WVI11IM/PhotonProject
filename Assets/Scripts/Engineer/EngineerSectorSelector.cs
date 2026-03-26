using UnityEngine;


public class EngineerSectorSelector : MonoBehaviour
{
    public Sector CurrentSector { get; private set; } = Sector.None;

    //Checks the currently selected sector when in the Engine Room
    public void SetSector(Sector sector)
    {
        CurrentSector = sector;
        Debug.Log("Hovering sector: " + sector);
    }

    public void ClearSector()
    {
        CurrentSector = Sector.None;
    }
}