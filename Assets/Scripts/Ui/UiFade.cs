using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class UiFade : MonoBehaviour {
    [SerializeField] AnimationCurve _fadeCurve;
    [SerializeField] bool _startVisible;
    [SerializeField] bool _fadeOnStart;

    [System.Serializable]
    struct FadeAction {
        [SerializeField] bool _fadeIn;
        public bool FadeIn => _fadeIn;
        [SerializeField] float _fadeTime;
        public float FadeTime => _fadeTime;
        [SerializeField] float _delayTime;
        public float DelayTime => _delayTime;
        [SerializeField] AnimationCurve _curve;
        public AnimationCurve Curve => _curve;
        [SerializeField] bool _useCurve;
        public bool UseCurve => _useCurve;
        [SerializeField] UnityEvent _onStart;
        public UnityEvent OnStart => _onStart;
        [SerializeField] UnityEvent _onEnd;
        public UnityEvent OnEnd => _onEnd;
    }

    [SerializeField] List<FadeAction> _fadeQueue = new List<FadeAction>();

    CanvasGroup _cg;
    Coroutine _fade;

    void Awake(){
        _cg = GetComponent<CanvasGroup>();
        _cg.alpha = _startVisible ? 1 : 0;
    }
    void Start(){
        if (_fadeOnStart){
            Fade();
        }
    }
    public void Fade(){
        if (_fade != null){
            StopCoroutine(_fade);
        }
        _fade = StartCoroutine(_Fade());

        IEnumerator _Fade(){
            for (int i=0; i<_fadeQueue.Count; ++i){
                var f = _fadeQueue[i];
                float dir = f.FadeIn ? 1 : -1;
                AnimationCurve curve = f.UseCurve ? f.Curve : _fadeCurve;
                yield return new WaitForSeconds(f.DelayTime);

                f.OnStart.Invoke();

                float t=0;
                while (t<f.FadeTime){
                    float p = t/f.FadeTime;
                    if (!f.FadeIn){
                        p = 1f-p;
                    }
                    _cg.alpha = curve.Evaluate(p);
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }
                _cg.alpha = curve.Evaluate(f.FadeIn ? 1 : 0);

                f.OnEnd.Invoke();
            }

            _fade = null;
        }
    }
    [ContextMenu("Fade Out")]
    public void FadeOut(){
        float time = 1;
        if (_fade != null){
            StopCoroutine(_fade);
        }
        _fade = StartCoroutine(_FadeOut());

        IEnumerator _FadeOut(){
            float a = _cg.alpha;
            float t = 0;
            while (t < time){
                _cg.alpha = Mathf.Lerp(0, a, 1f-(t/time));
                t += Time.deltaTime;
                yield return null;
            }
            _cg.alpha = 0;
        }
    }
}
