using UnityEngine;
using UnityEngine.EventSystems;

public class SectorHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sector sector;
    [SerializeField] private EngineerSectorSelector selector;

    //Checks for a mouse pointer to determine which sector is being opened
    public void OnPointerEnter(PointerEventData eventData)
    {
        selector.SetSector(sector);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        selector.ClearSector();
    }
}