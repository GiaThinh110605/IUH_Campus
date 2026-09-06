using UnityEngine;

namespace IUHCampus.Interaction
{
    public interface IInteractable
    {
        bool CanInteract();
        void Interact(GameObject source);
        string GetInteractionText();
    }
}
