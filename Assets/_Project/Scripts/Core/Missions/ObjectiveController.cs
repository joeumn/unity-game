using System;
using System.Collections.Generic;
using Project.Gameplay.Interaction;
using UnityEngine;

namespace Project.Core.Missions
{
    /// <summary>
    /// Tracks mission objectives and responds to gadget/trigger events.
    /// </summary>
    public class ObjectiveController : MonoBehaviour
    {
        [SerializeField]
        private HackGadget hackGadget;

        [SerializeField]
        private List<ObjectiveDefinition> defaultObjectives = new();

        private readonly List<ObjectiveRuntimeState> runtimeObjectives = new();
        private bool completionRaised;

        public IReadOnlyList<ObjectiveRuntimeState> Objectives => runtimeObjectives;

        public event Action<ObjectiveRuntimeState> OnObjectiveChanged;
        public event Action OnAllObjectivesCompleted;

        private void Awake()
        {
            BuildRuntimeObjectives(defaultObjectives);
        }

        private void OnEnable()
        {
            AttachHackGadget();
        }

        private void OnDisable()
        {
            DetachHackGadget();
        }

        private void Update()
        {
            bool anyTicked = false;
            float deltaTime = Time.deltaTime;

            foreach (var objective in runtimeObjectives)
            {
                if (objective.Tick(deltaTime))
                {
                    anyTicked = true;
                    OnObjectiveChanged?.Invoke(objective);
                }
            }

            if (anyTicked)
            {
                CheckForCompletion();
            }
        }

        /// <summary>
        /// Replaces the runtime objective list using mission data.
        /// </summary>
        public void InitializeObjectives(IEnumerable<ObjectiveDefinition> definitions)
        {
            BuildRuntimeObjectives(definitions ?? defaultObjectives);
        }

        public void NotifyLocationReached(string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return;
            }

            foreach (var objective in runtimeObjectives)
            {
                if (objective.Definition.Type == ObjectiveType.ReachLocation &&
                    string.Equals(objective.Definition.TargetId, targetId, StringComparison.OrdinalIgnoreCase))
                {
                    if (objective.MarkComplete())
                    {
                        OnObjectiveChanged?.Invoke(objective);
                        CheckForCompletion();
                    }
                }
            }
        }

        public void StartDefendObjective(string targetId)
        {
            var objective = FindObjective(ObjectiveType.DefendForDuration, targetId);
            if (objective != null && objective.StartTimer())
            {
                OnObjectiveChanged?.Invoke(objective);
            }
        }

        public void StopDefendObjective(string targetId)
        {
            var objective = FindObjective(ObjectiveType.DefendForDuration, targetId);
            if (objective != null && objective.StopTimer())
            {
                OnObjectiveChanged?.Invoke(objective);
            }
        }

        private void AttachHackGadget()
        {
            if (hackGadget == null)
            {
                hackGadget = FindObjectOfType<HackGadget>();
            }

            if (hackGadget != null)
            {
                hackGadget.OnHackPerformed += HandleHackPerformed;
            }
        }

        private void DetachHackGadget()
        {
            if (hackGadget != null)
            {
                hackGadget.OnHackPerformed -= HandleHackPerformed;
            }
        }

        private void HandleHackPerformed(IInteractable interactable)
        {
            if (interactable == null || string.IsNullOrEmpty(interactable.InteractionId))
            {
                return;
            }

            foreach (var objective in runtimeObjectives)
            {
                if (objective.Definition.Type == ObjectiveType.HackConsole &&
                    string.Equals(objective.Definition.TargetId, interactable.InteractionId, StringComparison.OrdinalIgnoreCase))
                {
                    if (objective.MarkComplete())
                    {
                        OnObjectiveChanged?.Invoke(objective);
                        CheckForCompletion();
                    }
                }
            }
        }

        private ObjectiveRuntimeState FindObjective(ObjectiveType type, string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return null;
            }

            foreach (var objective in runtimeObjectives)
            {
                if (objective.Definition.Type == type &&
                    string.Equals(objective.Definition.TargetId, targetId, StringComparison.OrdinalIgnoreCase))
                {
                    return objective;
                }
            }

            return null;
        }

        private void BuildRuntimeObjectives(IEnumerable<ObjectiveDefinition> definitions)
        {
            runtimeObjectives.Clear();
            completionRaised = false;

            if (definitions == null)
            {
                return;
            }

            foreach (var definition in definitions)
            {
                if (definition == null)
                {
                    continue;
                }

                var runtime = new ObjectiveRuntimeState(definition);
                runtimeObjectives.Add(runtime);
            }
        }

        private void CheckForCompletion()
        {
            if (completionRaised || runtimeObjectives.Count == 0)
            {
                return;
            }

            foreach (var objective in runtimeObjectives)
            {
                if (!objective.IsComplete)
                {
                    return;
                }
            }

            completionRaised = true;
            OnAllObjectivesCompleted?.Invoke();
        }

        [Serializable]
        public sealed class ObjectiveRuntimeState
        {
            public ObjectiveDefinition Definition { get; }
            public float Progress { get; private set; }
            public bool IsComplete { get; private set; }
            public bool IsTimerActive { get; private set; }

            public ObjectiveRuntimeState(ObjectiveDefinition definition)
            {
                Definition = definition;
                Progress = 0f;
                IsComplete = false;
                IsTimerActive = false;
            }

            public bool MarkComplete()
            {
                if (IsComplete)
                {
                    return false;
                }

                Progress = Definition.RequiredValue;
                IsComplete = true;
                IsTimerActive = false;
                return true;
            }

            public bool StartTimer()
            {
                if (IsComplete || IsTimerActive)
                {
                    return false;
                }

                IsTimerActive = true;
                return true;
            }

            public bool StopTimer()
            {
                if (!IsTimerActive)
                {
                    return false;
                }

                IsTimerActive = false;
                return true;
            }

            /// <summary>
            /// Advances the defend timer; returns true when progress changed.
            /// </summary>
            public bool Tick(float deltaTime)
            {
                if (!IsTimerActive || IsComplete)
                {
                    return false;
                }

                Progress = Mathf.Min(Definition.RequiredValue, Progress + deltaTime);
                if (Progress >= Definition.RequiredValue)
                {
                    IsComplete = true;
                    IsTimerActive = false;
                }

                return true;
            }
        }
    }
}
