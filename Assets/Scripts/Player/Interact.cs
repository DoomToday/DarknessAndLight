using TMPro;
using UnityEngine;

public class Interact : MonoBehaviour
{
    [SerializeField] private InputController input;
    [SerializeField] private TextMeshProUGUI interactHint;
    [SerializeField] private float interactRadius = 2.5f;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private LayerMask interactableMask;

    private void OnEnable()
    {
        input.OnInteractInput += HandleInteraction;
    }
    private void OnDisable()
    {
        input.OnInteractInput -= HandleInteraction;
    }
    private void HandleInteraction()
    {
        Collider[] hits = Physics.OverlapSphere(interactionPoint.position, interactRadius, interactableMask);
        foreach (Collider hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract)
            {
                interactable.Interact();
                break; 
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(interactionPoint.position, interactRadius);
    }
}
