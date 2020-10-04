using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Car : MonoBehaviour {
    protected abstract float speed {get;}
    public float Speed => speed * HighwayManagement.SpeedScale;
    [SerializeField] protected float _entryOffsetRadians = 0.1f;

    protected Coroutine _transfer {get; private set;}
    protected float _theta;
    protected Road _currentRoad;
    protected Roundabout _currentRoundabout;

    protected abstract float CurrentRadius {get;}
    
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

    Vector2 XZ(Vector3 v){return new Vector2(v.x, v.z);}
    protected void Transfer(Vector3 endPoint, Vector3 endFwd, System.Action onTransferred=null){
        _transfer = StartCoroutine(_Transfer());
        IEnumerator _Transfer(){
            Vector3 startFwd = transform.forward;
            Vector3 startPoint = transform.position;

            endPoint += endFwd * (Vector3.Distance(startPoint, endPoint)/2);

            // Get circle
            Vector3 startRight = transform.right;
            Vector3 endRight = Vector3.Cross(endFwd, Vector3.up);
            Vector2 ctr2 = Vector2.zero;
            Vector3 p0 = startPoint;
            Vector3 p1 = startPoint + (startRight * 100);
            Vector3 p2 = endPoint;
            Vector3 p3 = endPoint - (endRight * 100);
            LineIntersection(XZ(p0), XZ(p1), XZ(p2), XZ(p3), ref ctr2);
            Vector3 centre = new Vector3(ctr2.x, 0, ctr2.y);
            float radius = Vector3.Distance(startPoint, centre);

            float dTheta = Vector3.SignedAngle(startFwd, endFwd, -Vector3.up) * Mathf.Deg2Rad;
            float arcLength = Mathf.Abs(dTheta * radius);
            float startTheta = Mathf.Repeat(Polar.ToPolar(transform.position, centre), Mathf.PI*2);
            float endTheta = startTheta + dTheta;

            float time = arcLength / Speed;

            Debug.DrawLine(p0, p1, Color.magenta, 3f);
            Debug.DrawLine(p2, p3, Color.magenta, 3f);
            Debug.DrawRay(startPoint, startFwd*100, Color.yellow, 3f);
            Debug.DrawRay(endPoint, endFwd*100, Color.yellow, 3f);
            Debug.DrawLine(startPoint, centre, Color.green, 3f);

            float t = 0;
            while (t < time){
                float p = t/time;
                float theta = Mathf.Lerp(startTheta, endTheta, p);
                theta = Mathf.Repeat(theta, Mathf.PI * 2);
                Vector3 circlePos = Polar.FromPolar(theta, radius, centre);
                transform.position = Vector3.Lerp(circlePos, Vector3.Lerp(startPoint,endPoint,p), Mathf.Sqrt(p));
                transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(startFwd, Vector3.up), Quaternion.LookRotation(endFwd, Vector3.up), p);
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
                transform.position = Polar.FromPolar(_theta, CurrentRadius, _currentRoundabout.transform.position);
                transform.forward = Polar.PolarForward(_theta, CurrentRadius);
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
                _theta = Polar.ToPolar(transform.position, _currentRoundabout.transform.position);
                transform.position = Polar.FromPolar(_theta, _currentRoundabout.RadiusOuter, _currentRoundabout.transform.position);
                transform.forward = Polar.PolarForward(_theta, _currentRoundabout.RadiusOuter);
            }
        } else if (_currentRoad) {
            var r = c.GetComponent<Roundabout>();
            if (r){
                _currentRoundabout = r;
                _theta = Polar.ToPolar(transform.position, _currentRoundabout.transform.position);
                Vector3 target = Polar.FromPolar(_theta + _entryOffsetRadians, CurrentRadius, _currentRoundabout.transform.position);
                Vector3 fwd = Polar.PolarForward(_theta + _entryOffsetRadians, CurrentRadius);
                Transfer(target, fwd, ()=>{
                    _currentRoad=null;
                    _theta = _theta + _entryOffsetRadians;
                });
                OnEnteredRoundabout(r);
            }
        }
    }
    protected virtual void OnEnteredRoundabout(Roundabout r){}

    public static bool LineIntersection( Vector2 p1,Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection )
    {
        float Ax,Bx,Cx,Ay,By,Cy,d,e,f,num,offset;
        float x1lo,x1hi,y1lo,y1hi;
    
        Ax = p2.x-p1.x;
        Bx = p3.x-p4.x;
    
        // X bound box test/
        if(Ax<0) {
            x1lo=p2.x; x1hi=p1.x;
        } else {
            x1hi=p2.x; x1lo=p1.x;
        }
    
        if(Bx>0) {
            if(x1hi < p4.x || p3.x < x1lo) return false;
        } else {
            if(x1hi < p3.x || p4.x < x1lo) return false;
        }
    
        Ay = p2.y-p1.y;
        By = p3.y-p4.y;
    
        // Y bound box test//
        if(Ay<0) {                 
            y1lo=p2.y; y1hi=p1.y;
        } else {
            y1hi=p2.y; y1lo=p1.y;
        }
    
        if(By>0) {
            if(y1hi < p4.y || p3.y < y1lo) return false;
        } else {
            if(y1hi < p3.y || p4.y < y1lo) return false;
        }
    
        Cx = p1.x-p3.x;
        Cy = p1.y-p3.y;
        d = By*Cx - Bx*Cy;  // alpha numerator//
        f = Ay*Bx - Ax*By;  // both denominator//
    
        // alpha tests//
        if(f>0) {
            if(d<0 || d>f) return false;
        } else {
            if(d>0 || d<f) return false;
        }
    
        e = Ax*Cy - Ay*Cx;  // beta numerator//
    
        // beta tests //
        if(f>0) {                          
        if(e<0 || e>f) return false;
        } else {
        if(e>0 || e<f) return false;
        }
    
        // check if they are parallel
        if(f==0) return false;
    
        // compute intersection coordinates //
        num = d*Ax; // numerator //
        offset = SameSign(num,f) ? f*0.5f : -f*0.5f;   // round direction //
        intersection.x = p1.x + (num+offset) / f;
    
        num = d*Ay;
        offset = SameSign(num,f) ? f*0.5f : -f*0.5f;
        intersection.y = p1.y + (num+offset) / f;
    
        return true;
    }
    
    private static bool SameSign( float a, float b ){
        return ( ( a * b ) >= 0f );
    }

}
