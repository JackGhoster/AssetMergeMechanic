using DG.Tweening;
using Game.Managers;
using Game.Objects.Obstacle;
using System;
using System.Collections;
using UnityEngine;
using Utility;

namespace Game.AssetMerge {
    public class MergableAsset : MonoBehaviour {
        [field: SerializeField] public MergableAssetType MergableAssetType { get; set; }
        [field: SerializeField] public MergeConfig MergeConfig { get; set; }
        public float GhostedTime => MergeConfig ? MergeConfig.GhostedTime : 5f;
        public Obstacle Obstacle => gameObject.GetComponent<Obstacle>();//can't show
        public MergeManager MergeManager { get; set; } = null;
        public AppearanceObject AppearanceObject { get; set; }//can't show
        public Vector2 StartingPos { get; private set; }
        public Vector2Int GridStartingPos { get; private set; }
        public bool IsMoving { get; private set; } = false;
        public bool IsGhosting { get; private set; } = false;
        public bool IsGhosted { get; private set; } = false;
        public bool Cancelled { get; set; } = false;

        public event Action StartedMovement;
        public event Action StoppedMovement;

        private MergedTimer _createdTimer = null;

        private void Start() {
            StartingPos = Obstacle.transform.position;
            GridStartingPos = Obstacle.position;
        }
        private void Update() {
            if (!IsGhosting) OnPointerMove();
        }

        public void SetMoving(bool moving) {
            AppearanceObject = GetComponentInChildren<AppearanceObject>();
            if (AppearanceObject) MergableAssetType = AppearanceObject.AssetType;
            IsMoving = moving;

            if (moving) {
                if (AppearanceObject) AppearanceObject.SetSorting();
                Obstacle.DisableColliders();
            } else {
                if (AppearanceObject) AppearanceObject.ResetSorting();
                Obstacle.EnableColliders();
            }
        }


        void OnPointerMove() {
            if (!IsMoving) return;
            var input = (Vector2)Camera.main.ScreenToWorldPoint(Input.touches[0].position);
            var position = new Vector3(input.x, input.y, 10);
            Obstacle.transform.position = position;
        }

        public void OnPointerExit() {
            //merge logic
            var anotherMergable = MergeManager.GetAnotherMergableAtPosition(Obstacle.transform.position);
            if (anotherMergable != null && !Cancelled) {
                anotherMergable.CacheAppearanceType();
                //if its not the same object
                if (MergableAssetType != anotherMergable.MergableAssetType) ReturnToStartingPoint();
                else {
                    //successful merge
                    GenerateNew(anotherMergable.Obstacle.position, out var newObstacle);

                    anotherMergable.Obstacle.MergeDestroy();
                    newObstacle.Init(Obstacle.Level);
                    newObstacle.SetObjectOnGrid(true);

                    this.TurnGhost();
                }

                return;
            }

            //empty or unsuccessful merge
            ReturnToStartingPoint();
        }

        public bool TryChangeAppearanceFast() {
            if (TryGetComponent<AppearanceRandomizer>(out var randomizer)) {
                randomizer.HardRandomize(MergableAssetType);

                return true;
            }
            return false;
        }

        public void GenerateNew(Vector2Int position, out Obstacle newObstacle) {
            newObstacle = Obstacle.Level.SpawnManager.InstantiateOnGrid(
                prefab: MergeConfig.Assets[UnityEngine.Random.Range(0, MergeConfig.Assets.Count)].gameObject,
                tile: position,
                parent: transform.parent
            ).GetComponent<Obstacle>();
        }

        public void CacheAppearanceType() {
            AppearanceObject = GetComponentInChildren<AppearanceObject>();
            if (AppearanceObject) MergableAssetType = AppearanceObject.AssetType;
        }

        private void ReturnToStartingPoint() {
            SetMoving(true);
            IsGhosting = true;

            Obstacle.transform.DOMove(StartingPos, .5f).SetUpdate(true).OnComplete(() => {
                SetMoving(false);
                IsGhosting = false;
            });
            Cancelled = false;
        }

        private void TurnGhost() {
            SetMoving(false);

            this.IsGhosting = true;

            TryChangeAppearanceFast();
            AppearanceObject = GetComponentInChildren<AppearanceObject>();
            CacheAppearanceType();
            AppearanceObject.SetAlpha(0.3f);

            Obstacle.transform.position = StartingPos;

            SetTimer();

            StartCoroutine(GhostCoroutine(GhostedTime));
        }

        private void SetTimer() {
            _createdTimer = Instantiate(
                    MergeConfig.TimerPrefab,
                    this.transform.position,
                    Quaternion.identity,
                    this.transform);
            _createdTimer.StartFilling(GhostedTime);
        }


        private IEnumerator GhostCoroutine(float time) {
            IsGhosted = true;
            Obstacle.ChangeObstacleType(GridRefactor.ObstacleType.Hay);
            Obstacle.DisableColliders();
            yield return new WaitForSeconds(time);

            IsGhosted = false;
            AppearanceObject.SetAlpha(1);
            Obstacle.ChangeObstacleType(GridRefactor.ObstacleType.Stone);
            Obstacle.EnableColliders();
            IsGhosting = false;
            _createdTimer = null;
            Cancelled = false;
        }
    }
}
