using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public static class Polar {
    public static Vector3 PolarForward(float theta, float radius, float sign=1){
        return FromPolar(theta + (sign*0.01f), radius, Vector3.zero) - FromPolar(theta, radius, Vector3.zero);
    }
    public static Vector3 FromPolar(float theta, float radius, Vector3 centre){
        return centre + (radius * new Vector3 (Mathf.Cos(theta), 0, Mathf.Sin(theta)));
    }
    public static float ToPolar(Vector3 position, Vector3 centre){
        Vector3 r = position-centre;
        float radius = r.magnitude;
        return Mathf.Atan2(r.z, r.x);
    }
}
public class Road : RoadBase {
    static List<Road> _all = new List<Road>();
    public static ReadOnlyCollection<Road> All => _all.AsReadOnly();
    public static Road Active {get; private set;}

    [SerializeField] Transform _entry1;
    [SerializeField] Transform _entry2;
    [SerializeField] Transform _exit1;
    [SerializeField] Transform _exit2;

    [SerializeField] Transform _joint1;
    [SerializeField] Transform _joint2;

    [SerializeField] GameObject _block1;
    [SerializeField] GameObject _block2;

    [SerializeField] Despawner _despawn1;
    [SerializeField] Despawner _despawn2;

    [SerializeField] Ai[] _aiPrefabs;

    [SerializeField] Vector2 _minMaxSpawnTime;

    [SerializeField] Roundabout _r1;
    [SerializeField] Roundabout _r2;

    public Vector3 Entry1 => _entry1.position;
    public Vector3 Entry2 => _entry2.position;
    public Vector3 Exit1 => _exit1.position;
    public Vector3 Exit2 => _exit2.position;

    public Vector3 Joint1 => _joint1.position;
    public Vector3 Joint2 => _joint2.position;

    Transform _activeSpawnPoint = null;
    float _timeNextSpawn;

    void Awake(){
        _block1.SetActive(false);
        _block2.SetActive(false);
        _all.Add(this);
        SetNextSpawnTime();
    }
    protected override void OnDestroy(){
        _all.Remove(this);
        base.OnDestroy();
    }
    void Update(){
        if (Time.time > _timeNextSpawn){
            SetNextSpawnTime();
            if (_activeSpawnPoint){
                if (Active == this) return;
                Instantiate<Ai>(_aiPrefabs[Random.Range(0,_aiPrefabs.Length)], _activeSpawnPoint.position, _activeSpawnPoint.rotation);
            }
        }
    }
    void SetNextSpawnTime(){
        _timeNextSpawn = Time.time + Random.Range(_minMaxSpawnTime.x, _minMaxSpawnTime.y);
    }

    public void AddToRoundabout(Roundabout r, float angleDeg){
        angleDeg = Mathf.Repeat(angleDeg, 360);
        transform.eulerAngles = new Vector3(0, angleDeg, 0);
        Vector3 target = Polar.FromPolar(angleDeg * Mathf.Deg2Rad, r.RadiusOuter, r.transform.position);
        transform.position = r.transform.position;
        _r1 = r;
    }
    public void RoundaboutAdded(Roundabout r){
        _r2 = r;
    }
    public void SetActiveRoundabout(Roundabout r, bool block){
        if (r == _r1){
            _activeSpawnPoint = _entry2;
            if (block){
                _block1.SetActive(true);
            }
        } else if (r == _r2){
            _activeSpawnPoint = _entry1;
            if (block){
                _block2.SetActive(true);
            }
        }
    }
    public void PlayerEnteredRoad(){
        if (Active != this){
            Active = this;
            HighwayManagement.SpawnRoundabout();
            _activeSpawnPoint = null;
            _despawn2.gameObject.SetActive(false);
        }
    }
    public void PlayerExitedRoad(){
        if (Active == this){
            Active = null;
            _despawn1.gameObject.SetActive(true);
        }
    }

    public Vector3 GetForward(Vector3 position){
        float dEntry1 = Vector3.Distance(position, Entry1);
        float dEntry2 = Vector3.Distance(position, Entry2);

        float min = Mathf.Min(dEntry1, dEntry2);
        return (Mathf.Approximately(min, dEntry1)) ? _Fwd(Exit1) : _Fwd(Exit2);

        Vector3 _Fwd(Vector3 target){
            return (target - position).normalized;
        }
    }

    void OnDrawGizmos(){
        var gc = Gizmos.color;
        Gizmos.color = Color.green;
        Vector3 up = Vector3.up * 0.01f;
        if (_entry1 && _exit1){
            Gizmos.DrawLine(Entry1+up, Exit1+up);
        }
        if (_exit2 && _exit2){
            Gizmos.DrawLine(Entry2+up, Exit2+up);
        }
        Gizmos.color = Color.blue;
        if (_joint1 && _joint2){
            Gizmos.DrawLine(Joint1+up, Joint2+up);
        }
        Gizmos.color = gc;
    }
}
