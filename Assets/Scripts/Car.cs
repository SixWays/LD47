using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Car : MonoBehaviour {
    public float Speed;
    [SerializeField] protected float _entryOffsetRadians = 0.1f;

    protected Coroutine _transfer {get; private set;}
    protected float _theta;
    protected Road _currentRoad;
    protected Roundabout _currentRoundabout;

    protected abstract float CurrentRadius {get;}

    protected Vector3 FromPolar(float theta, float radius, Vector3 centre){
        return centre + (radius * new Vector3 (Mathf.Cos(theta), 0, Mathf.Sin(theta)));
    }
    protected float ToPolar(Vector3 position, Vector3 centre){
        Vector3 r = position-centre;
        float radius = r.magnitude;
        return Mathf.Atan2(r.z, r.x);
    }
    protected Vector3 PolarForward(float theta, float radius){
        return FromPolar(theta + 0.01f, radius, Vector3.zero) - FromPolar(theta, radius, Vector3.zero);
    }
    protected Vector3 _nearestEntry {
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

    protected void Transfer(Vector3 endPoint, Vector3 endFwd, System.Action onTransferred=null){
        _transfer = StartCoroutine(_Transfer());
        IEnumerator _Transfer(){
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
    void LateUpdate() {
        if (_transfer == null){
            float distance = Speed * Time.deltaTime;
            if (_currentRoad){
                transform.position += transform.forward * distance;
            } else if (_currentRoundabout) {
                // c = 2pr
                // l = theta r
                // theta = l/r
                float dTheta = distance / CurrentRadius;
                _theta += dTheta;
                _theta = Mathf.Repeat(_theta, Mathf.PI*2);
                transform.position = FromPolar(_theta, CurrentRadius, _currentRoundabout.transform.position);
                transform.forward = PolarForward(_theta, CurrentRadius);
            }
        }
    }

    protected virtual void OnTriggerEnter(Collider c){
        if (!_currentRoad && !_currentRoundabout){
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
        } else if (_currentRoad) {
            var r = c.GetComponent<Roundabout>();
            if (r){
                _currentRoundabout = r;
                _theta = ToPolar(transform.position, _currentRoundabout.transform.position);
                Vector3 target = FromPolar(_theta + _entryOffsetRadians, CurrentRadius, _currentRoundabout.transform.position);
                Vector3 fwd = PolarForward(_theta + _entryOffsetRadians, CurrentRadius);
                Transfer(target, fwd, ()=>{_currentRoad=null;});
            }
        }
    }
}
