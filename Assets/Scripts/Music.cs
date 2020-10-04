using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour {
    [SerializeField] AudioSource _main;
    [SerializeField] AudioSource _loop;
    [SerializeField] float _minLoopTime;
    [SerializeField] float _loopSpeedupTime;

    IEnumerator Start(){
        if (!_loop.clip){
            Debug.Log("NO MUSIC");
            yield break;
        }

        _main.Play();
        while (_main.isPlaying){
            yield return null;
        }
        _loop.loop = false;
        _loop.Play();
        float wait = _loop.clip.length;
        float t = 0;
        while (true){
            yield return new WaitForSeconds(wait);
            t += wait;
            _loop.Stop();
            _loop.Play();
            float p = Mathf.Clamp01(t/_loopSpeedupTime);
            p = 1f-p;
            wait = Mathf.Lerp(_minLoopTime, _loop.clip.length, p);
        }
    }
}
