using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai : MonoBehaviour {
    public float Speed;
    [SerializeField, Range(0f,1f)] float _exitChance;
    [SerializeField] float _entryOffsetRadians = 0.1f;

    public Road _currentRoad;
    public Roundabout _currentRoundabout;
    float _theta;

    Coroutine _transfer;

    void Update() {
        if (_transfer == null){
            float distance = Speed * Time.deltaTime;
            if (_currentRoad){
                transform.position += transform.forward * distance;
            } else {
                // c = 2pr
                // l = theta r
                // theta = l/r
                float dTheta = distance / _currentRoundabout.RadiusOuter;
                _theta += dTheta;
                _theta = Mathf.Repeat(_theta, Mathf.PI*2);
                transform.position = FromPolar(_theta, _currentRoundabout.RadiusOuter, _currentRoundabout.transform.position);
                transform.forward = PolarForward(_theta, _currentRoundabout.RadiusOuter);
            }
        }
    }
    Vector3 FromPolar(float theta, float radius, Vector3 centre){
        return centre + (radius * new Vector3 (Mathf.Cos(theta), 0, Mathf.Sin(theta)));
    }
    float ToPolar(Vector3 position, Vector3 centre){
        Vector3 r = position-centre;
        float radius = r.magnitude;
        return Mathf.Atan2(r.z, r.x);
    }
    Vector3 PolarForward(float theta, float radius){
        return FromPolar(theta + 0.01f, radius, Vector3.zero) - FromPolar(theta, radius, Vector3.zero);
    }
    Vector3 _nearestEntry {
        get {
            Vector3 entry = Vector3.zero;
            if (Vector3.Distance(transform.position, _currentRoad.Entry1) < Vector3.Distance(transform.position, _currentRoad.Entry2)){
                entry = _currentRoad.Entry1;
            } else {
                entry = _currentRoad.Entry2;
            }
            return entry;
        }
    }
    void OnTriggerEnter(Collider c){
        if (_currentRoundabout){
            var r = c.GetComponent<Road>();
            if (r){
                _currentRoad = r;
                _currentRoundabout = null;
                if (Random.Range(0f,1f) < _exitChance){
                    _transfer = StartCoroutine(_Transfer(_nearestEntry, _currentRoad.GetForward(_nearestEntry)));
                }
            }
        } else if (_currentRoad) {
            var r = c.GetComponent<Roundabout>();
            if (r){
                _currentRoundabout = r;
                _theta = ToPolar(transform.position, _currentRoundabout.transform.position);
                Vector3 target = FromPolar(_theta + _entryOffsetRadians, _currentRoundabout.RadiusOuter, _currentRoundabout.transform.position);
                Vector3 fwd = PolarForward(_theta + _entryOffsetRadians, _currentRoundabout.RadiusOuter);
                _transfer = StartCoroutine(_Transfer(target, fwd, ()=>{_currentRoad=null;}));
            }
        } else {
            // Just spawned
            _currentRoad = c.GetComponent<Road>();
            _currentRoundabout = c.GetComponent<Roundabout>();
            if (_currentRoad){
                transform.position = _nearestEntry;
                transform.forward = _currentRoad.GetForward(transform.position);
            } else if (_currentRoundabout){
                _theta = ToPolar(transform.position, _currentRoundabout.transform.position);
                transform.position = FromPolar(_theta, _currentRoundabout.RadiusOuter, _currentRoundabout.transform.position);
                transform.forward = PolarForward(_theta, _currentRoundabout.RadiusOuter);
            }
        }

        IEnumerator _Transfer(Vector3 endPoint, Vector3 endFwd, System.Action onTransferred=null){
            Vector3 startPoint = transform.position;
            float time = Vector3.Distance(startPoint, endPoint) / Speed;
            float t = 0;
            while (t < time){
                transform.position = Vector3.Lerp(startPoint, endPoint, t/time);
                t += Time.deltaTime;
                yield return null;
            }
            transform.position = endPoint;
            transform.forward = endFwd;
            onTransferred?.Invoke();
            _transfer = null;
        }
    }
}
