using UnityEngine;
using UnityEngine.EventSystems;

public class SectorHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sector sector;
    [SerializeField] private EngineerSectorSelector selector;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    //Checks for a mouse pointer to determine which sector is being opened
    public void OnPointerEnter(PointerEventData eventData)
    {
        selector.SetSector(sector);
        if (animator != null)
        {
            animator.SetBool("isSelected", true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        selector.ClearSector();
        if (animator != null)
        {
            animator.SetBool("isSelected", false);
        }
    }
}