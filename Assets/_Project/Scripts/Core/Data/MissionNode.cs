using System.Collections.Generic;
using Project.Core.Missions;
using UnityEngine;

namespace Project.Core.Data
{
    /// <summary>
    /// Defines a mission node inside the cyber operations graph.
    /// </summary>
    public enum MissionRoleFocus
    {
        Red = 0,
        Blue = 1,
        Both = 2
    }

    [CreateAssetMenu(menuName = "Project/Missions/Mission Node", fileName = "MissionNode")]
    public class MissionNode : ScriptableObject
    {
        [SerializeField]
        private string id = "mission-node-id";

        [SerializeField]
        private string displayName = "New Mission";

        [SerializeField, TextArea]
        private string description = "Describe the mission fantasy and objectives.";

        [SerializeField]
        private string environmentTag = "NeonDistrict";

        [SerializeField]
        private MissionRoleFocus recommendedRole = MissionRoleFocus.Both;

        [SerializeField]
        private List<MissionNode> connectedNodes = new List<MissionNode>();

        [SerializeField, Range(0.1f, 10f)]
        private float difficultyRating = 1f;

        [SerializeField]
        private List<ObjectiveDefinition> objectives = new List<ObjectiveDefinition>();

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public string EnvironmentTag => environmentTag;
        public MissionRoleFocus RecommendedRole => recommendedRole;
        public IReadOnlyList<MissionNode> ConnectedNodes => connectedNodes;
        public float DifficultyRating => difficultyRating;
        public IReadOnlyList<ObjectiveDefinition> Objectives => objectives;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = name.Replace(" ", "_");
            }

            if (difficultyRating <= 0f)
            {
                difficultyRating = 0.1f;
            }

            if (connectedNodes == null)
            {
                connectedNodes = new List<MissionNode>();
            }

            for (int i = connectedNodes.Count - 1; i >= 0; i--)
            {
                var node = connectedNodes[i];
                if (node == null || node == this)
                {
                    connectedNodes.RemoveAt(i);
                }
            }

            var seenIds = new HashSet<string>();
            for (int i = connectedNodes.Count - 1; i >= 0; i--)
            {
                var node = connectedNodes[i];
                if (node == null)
                {
                    continue;
                }

                if (!seenIds.Add(node.Id))
                {
                    connectedNodes.RemoveAt(i);
                }
            }

            if (objectives == null)
            {
                objectives = new List<ObjectiveDefinition>();
            }

            foreach (var objective in objectives)
            {
                objective?.EnsureValid($"{id}_Objective");
            }
        }
#endif
    }
}
