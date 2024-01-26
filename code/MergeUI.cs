using DG.Tweening;
using Game.Managers.Tutorial;
using Managers.Saves;
using System.Collections;
using UI.Common;
using UI.InGame;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.AssetMerge {
    public class MergeUI : MenuWindow {
        [field: SerializeField] public InGameUIManager UIManager { get; private set; } //main manager of all the ingame ui
        [field: SerializeField] public Button MergeButton { get; private set; }
        [field: SerializeField] public Button ExitButton { get; private set; }
        [field: SerializeField] public Image TimerImage { get; private set; }

        [SerializeField] private TutorialWindow _mergeTutorial;
        [SerializeField] private TutorialTellerController _tellerController;

        private Vector2 _mergeButtonPos;
        private Vector3 _mergeButtonScale;

        private bool _inMergeMode = false;
        private bool _inCooldownMode = false;
        private bool _inTutorial = false;


        private float _moveInTime = 0.5f;
        private float _moveOutTime = 0.5f;

        private float _fadeAmount = 0.5f;
        private float _fadeInTime = 0.1f;
        private float _fadeOutTime = 0.5f;

        private Vector3 ScaleDownAmount => _mergeButtonScale / 2;
        private float _scaleTime = 0.5f;
        

        public override void Init(bool startOpened = false) {
            base.Init(startOpened);
            _mergeTutorial.Init(false);


            MergeButton.onClick.AddListener(EnterMergeMode);
            ExitButton.onClick.AddListener(() => {
                UIManager.MergeManager.InGameManager.MergeEnded();
            });

            _mergeButtonPos = MergeButton.transform.position;
            _mergeButtonScale = MergeButton.transform.localScale;
        }
        private void Update() {
            //timer fill
            if (_inMergeMode && UIManager.MergeManager != null && UIManager.MergeManager.MergeConfig.AutoClose)
                TimerImage.fillAmount += Time.unscaledDeltaTime / (UIManager.MergeManager.MergeConfig.MergeModeTime - 1f);
            if (_inCooldownMode && UIManager.MergeManager != null)
                TimerImage.fillAmount += Time.unscaledDeltaTime / (UIManager.MergeManager.MergeConfig.MergeModeCooldown);
        }

        private void EnterMergeMode() {
            if (SaveManager.Preferences.seenAssetMergeTutorial == false) {
                ShowTutorial();
                return;
            }
            MergeButton.interactable = false;
            TimerImage.fillAmount = 0;
            _inMergeMode = true;
            UIManager.SetAllUI(false);
            ExitButton.gameObject.SetActive(true);
            UIManager.MergeManager.EnterMerge();
            _mergeButtonPos = MergeButton.transform.position;
            MergeButton.image.DOFade(_fadeAmount, _fadeInTime);
            MergeButton.transform.DOMoveX(UIManager.placeBombButton.transform.position.x, _moveInTime).SetUpdate(true);
            UIManager.MergeManager.Ended += ExitMergeMode;
        }

        private void ExitMergeMode() {
            UIManager.MergeManager.Ended -= ExitMergeMode;
            _inMergeMode = false;
            TimerImage.fillAmount = 0;
            ExitButton.gameObject.SetActive(false);
            UIManager.SetAllUI(true);
            UIManager.MergeManager.ExitMerge();
            MergeButton.transform.DOMoveX(_mergeButtonPos.x, _moveOutTime).SetUpdate(true).OnComplete(() => {
                EnterCooldownMode();
            });
        }

        private void EnterCooldownMode() {
            _inCooldownMode = true;
            MergeButton.interactable = false;
            TimerImage.fillAmount = 0;
            MergeButton.transform.DOScale(ScaleDownAmount, _scaleTime).SetUpdate(true).OnComplete(() => {
                StartCoroutine(CooldownCoroutine());
            });

        }

        IEnumerator CooldownCoroutine() {
            yield return new WaitUntil(() => TimerImage.fillAmount == 1);
            ExitCooldownMode();
        }

        private void ExitCooldownMode() {
            _inCooldownMode = false;
            TimerImage.fillAmount = 0;
            MergeButton.image.DOFade(1f, _fadeOutTime);
            MergeButton.transform.DOScale(_mergeButtonScale, _scaleTime).SetUpdate(true).OnComplete(() => {
                MergeButton.interactable = true;
            });
        }

        private void ShowTutorial() {
            _inTutorial = true;
            UIManager.MergeManager.InGameManager.FreezeGame();
            StartCoroutine(TutorialCoroutine());
            _mergeTutorial.Open();
            _tellerController.ShowLeftTeller();
            _mergeTutorial.OnClick += CloseTutorial;
        }

        private void CloseTutorial(PointerEventData pointer) {
            _mergeTutorial.Close();
            _inTutorial = false;
            SaveManager.Preferences.SetSeenAssetMergeTutorial(true);
            _tellerController.HideActiveTeller();
            UIManager.MergeManager.InGameManager.UnfreezeGame();
        }

        private IEnumerator TutorialCoroutine() {
            yield return new WaitUntil(() => !_inTutorial);
            EnterMergeMode();
        }
        
        private void OnEnable() {
            if (_inCooldownMode) StartCoroutine(CooldownCoroutine());
        }
    }
}