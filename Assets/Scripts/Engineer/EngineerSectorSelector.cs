using UnityEngine;


public class EngineerSectorSelector : MonoBehaviour
{
    public Sector CurrentSector { get; private set; } = Sector.None;

    [SerializeField] private AudioSource audioSelectSector;

    //Checks the currently selected sector when in the Engine Room
    public void SetSector(Sector sector)
    {
        CurrentSector = sector;
        Debug.Log("Hovering sector: " + sector);
        audioSelectSector.Play();
    }

    public void ClearSector()
    {
        CurrentSector = Sector.None;
    }
}