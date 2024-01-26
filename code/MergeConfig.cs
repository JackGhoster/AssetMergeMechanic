using Game.Objects.Obstacle;
using System.Collections.Generic;
using UnityEngine;

namespace Game.AssetMerge {
    [CreateAssetMenu(fileName = "MergeConfig", menuName = "Configs/MergeConfig")]
    public class MergeConfig : ScriptableObject {
        [field: SerializeField] public List<Obstacle> Assets { get; private set; } = new List<Obstacle>();
        [field: SerializeField] public float GhostedTime { get; private set; } = 5f;
        [field: SerializeField] public MergedTimer TimerPrefab { get; private set; }
        [field: SerializeField] public float MergeModeTime { get; private set; }
        [field: SerializeField] public float MergeModeCooldown { get; private set; }
        [field: SerializeField] public bool AutoClose { get; private set; } = true;
    }
}
