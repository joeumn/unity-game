using System;
using UnityEngine;

namespace Project.Core.Missions
{
    /// <summary>
    /// Serializable data that describes an objective in a mission node.
    /// </summary>
    [Serializable]
    public class ObjectiveDefinition
    {
        [SerializeField]
        private string id = "objective-id";

        [SerializeField]
        private string displayName = "New Objective";

        [SerializeField]
        private ObjectiveType type = ObjectiveType.HackConsole;

        [SerializeField]
        private string targetId = "TARGET";

        [SerializeField]
        private float requiredValue = 1f;

        public string Id => id;
        public string DisplayName => displayName;
        public ObjectiveType Type => type;
        public string TargetId => targetId;
        public float RequiredValue => Mathf.Max(0.01f, requiredValue);

        public void EnsureValid(string fallbackId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = fallbackId;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = id;
            }

            if (string.IsNullOrWhiteSpace(targetId))
            {
                targetId = id;
            }

            if (requiredValue <= 0f)
            {
                requiredValue = 1f;
            }
        }
    }
}
