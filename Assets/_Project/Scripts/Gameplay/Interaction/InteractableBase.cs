using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Gameplay.Interaction
{
    /// <summary>
    /// Default implementation that handles interaction gating and emissive highlighting.
    /// </summary>
    [DisallowMultipleComponent]
    public class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Identity")]
        [SerializeField]
        private string interactionId = "INTERACTABLE_ID";

        [SerializeField]
        private string label = "Interact";

        [SerializeField]
        private bool isInteractable = true;

        [Header("Events")]
        [SerializeField]
        private UnityEvent onInteract;

        [Header("Highlight")]
        [SerializeField]
        private Renderer[] highlightRenderers;

        [SerializeField]
        private Color highlightColor = new Color(0.2f, 0.85f, 1f);

        [SerializeField, Range(0f, 5f)]
        private float highlightEmission = 1.5f;

        private readonly List<RendererMaterialCache> rendererCache = new();
        private bool highlighted;

        public string InteractionId => interactionId;
        public string Label => label;

        protected virtual void Awake()
        {
            CacheRendererMaterials();
        }

        public virtual bool CanInteract(GameObject interactor)
        {
            return isInteractable;
        }

        public virtual void Interact(GameObject interactor)
        {
            if (!isInteractable)
            {
                return;
            }

            onInteract?.Invoke();
        }

        public virtual void Highlight(bool isHighlighted)
        {
            if (highlighted == isHighlighted)
            {
                return;
            }

            highlighted = isHighlighted;
            ApplyHighlight(isHighlighted);
        }

        /// <summary>
        /// Allows inheritors to toggle whether the object accepts interactions.
        /// </summary>
        /// <param name="allow">True if the object should be interactable.</param>
        protected void SetInteractable(bool allow)
        {
            isInteractable = allow;
            if (!allow && highlighted)
            {
                Highlight(false);
            }
        }

        private void CacheRendererMaterials()
        {
            rendererCache.Clear();

            if (highlightRenderers == null || highlightRenderers.Length == 0)
            {
                return;
            }

            foreach (var renderer in highlightRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                foreach (var material in renderer.materials)
                {
                    var cache = new RendererMaterialCache
                    {
                        Material = material,
                        BaseEmission = material.HasProperty("_EmissionColor")
                            ? material.GetColor("_EmissionColor")
                            : Color.black
                    };

                    rendererCache.Add(cache);
                }
            }
        }

        private void ApplyHighlight(bool enable)
        {
            if (rendererCache.Count == 0)
            {
                return;
            }

            foreach (var cache in rendererCache)
            {
                if (cache.Material == null)
                {
                    continue;
                }

                if (enable)
                {
                    cache.Material.EnableKeyword("_EMISSION");
                    cache.Material.SetColor("_EmissionColor", highlightColor * highlightEmission);
                }
                else
                {
                    cache.Material.SetColor("_EmissionColor", cache.BaseEmission);
                }
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(interactionId))
            {
                interactionId = name;
            }

            if (highlightRenderers == null || highlightRenderers.Length == 0)
            {
                var renderer = GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    highlightRenderers = new[] { renderer };
                }
            }
        }

        private struct RendererMaterialCache
        {
            public Material Material;
            public Color BaseEmission;
        }
    }
}
