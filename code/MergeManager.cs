using Game.Objects.Obstacle;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.AssetMerge {
    public class MergeManager : MonoBehaviour {
        [SerializeField] private InGameManager _inGameManager;
        public InGameManager InGameManager => _inGameManager;
        [field: SerializeField] public MergeConfig MergeConfig { get; private set; }
        public MergableAsset CurrentMergable { get; private set; } = null;

        private bool _mergeAllowed = false;
        public event Action Ended;


        private void Start() {
            if (_inGameManager == null) _inGameManager = GetComponent<InGameManager>();
        }

        private void Update() {
            if (!_mergeAllowed) return;
            if (Input.touchCount != 0) {
                if (Input.GetTouch(0).phase == TouchPhase.Began) OnPointerEnter();
                if (Input.GetTouch(0).phase == TouchPhase.Canceled || Input.GetTouch(0).phase == TouchPhase.Ended) {
                    OnPointerExit();
                }
            }
        }

        public void EnterMerge() {
            _mergeAllowed = true;
            _inGameManager.MergeStarted(timeScale: 0.1f, seconds: MergeConfig.MergeModeTime);
        }

        public void ExitMerge() {
            _mergeAllowed = false;
            if (CurrentMergable != null) CurrentMergable.Cancelled = true;
            OnPointerExit();
            Ended?.Invoke();
        }


        public MergableAsset GetMergableAtPosition(Vector2 pos, bool anotherMergable = true) {
            //get colliders at the point
            Collider2D[] colliders = Physics2D.OverlapPointAll(pos);
            if (colliders.Length == 0) return null;

            //find another
            foreach (Collider2D collider in colliders) {
                if (collider.gameObject.TryGetComponent<MergableAsset>(out var mergable)) {
                    if(anotherMergable) if (mergable == CurrentMergable) continue;
                    return mergable;
                }
            }
            return null;
        }

        private void OnPointerEnter() {
            var worldPos = (Vector2)Camera.main.ScreenToWorldPoint(Input.touches[0].position);

            var mergable = GetMergableAtPosition(pos: worldPos, anotherMergable: false);
            CurrentMergable = mergable;
            CurrentMergable.SetMoving(true);
            CurrentMergable.StartedMovement += OnPointerExit;
        }


        private void OnPointerExit() {
            if (CurrentMergable == null) return;
            CurrentMergable.MergeManager = this;
            CurrentMergable.OnPointerExit();
            CurrentMergable.StartedMovement -= OnPointerExit;
            CurrentMergable = null;
        }
    }
}
