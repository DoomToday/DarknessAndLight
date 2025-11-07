using UnityEngine;

public class ElectricalPanel : MonoBehaviour, IInteractable
{
    public bool CanInteract { get; private set; } = true;
    public void Interact() 
    { 
        Debug.Log("Electrical Panel has been interacted with.");
        SoundManager.MakeSound(transform.position);
        CanInteract = false;
    }
    
}
