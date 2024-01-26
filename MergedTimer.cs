using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.AssetMerge {
    public class MergedTimer : MonoBehaviour {
        [SerializeField] private Image _timerImage;
        private float _time;
        private bool _isRunning = false;
        private float _fadeTime = 0.5f;


        public void StartFilling(float time) {
            gameObject.SetActive(true);
            _time = time;
            _isRunning = true;
            StartCoroutine(OnFillCoroutine());
        }

        private void Update() {
            if(_isRunning) _timerImage.fillAmount += Time.deltaTime / _time;
        }

        public void Reset() {
            _timerImage.fillAmount = 0;
            gameObject.SetActive(false);
        }

        IEnumerator OnFillCoroutine() {
            yield return new WaitUntil(() => _timerImage.fillAmount == 1);
            _isRunning = false;
            _timerImage.DOFade(0, _fadeTime).OnComplete(() => { 
                Reset();
                Destroy(this.gameObject);
            });            
        }

    }
}