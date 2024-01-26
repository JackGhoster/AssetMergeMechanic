using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Configs.GameManagers;
using DG.Tweening;
using Game.AssetMerge;


//I made this script shorter and left only related to merge parts
//
namespace Game {
    public class InGameManager : MonoBehaviour {
        //above
        //some unrealated properties and fields
        [field: SerializeField] public MergeManager MergeManager { get; private set; }
        private float _time = 1;
        private bool IsSlowRunning;
        public event Action GameOver;
        //some unrealated properties and fields
        //bellow


        //above
        //some code that isnt important in this case

        public IEnumerator MergeSlow(float timeScale, float seconds) {
            var delay = 0.1f;
            Time.timeScale = timeScale;
            for (float time = timeScale; time < 1f; time += delay) {
                if (!IsSlowRunning) {
                    _time = 1;
                    Time.timeScale = 1;
                    yield break;
                }
                _time = time;
                yield return new WaitForSecondsRealtime(seconds * 0.1f);
                Time.timeScale = Math.Clamp(_time, timeScale, 1f);
            }
            if (MergeManager.MergeConfig.AutoClose) MergeEnded();
        }
        public void MergeStarted(float timeScale, float seconds) {
            GameOver += MergeEnded;
            IsSlowRunning = true;
            StartCoroutine(MergeSlow(timeScale, seconds));
        }
        public void MergeEnded() {
            GameOver -= MergeEnded;
            IsSlowRunning = false;

            MergeManager.ExitMerge();
        }

        //some code that isnt important in this case
        //bellow
    }
}