using UnityEngine;

namespace Project.Gameplay.Interaction
{
    /// <summary>
    /// Contract that allows the hack gadget or other systems to trigger world interactions.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Unique identifier that links runtime interactables to mission objectives.
        /// </summary>
        string InteractionId { get; }

        /// <summary>
        /// Friendly label for prompts and debug output.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Whether the supplied interactor may currently use this object.
        /// </summary>
        bool CanInteract(GameObject interactor);

        /// <summary>
        /// Executes the interaction payload.
        /// </summary>
        void Interact(GameObject interactor);

        /// <summary>
        /// Optional highlight hook for reticles/HUD feedback.
        /// </summary>
        void Highlight(bool isHighlighted);
    }
}
