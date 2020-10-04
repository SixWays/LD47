using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Roundabout : RoadBase {
    static List<Roundabout> _all = new List<Roundabout>();
    public static ReadOnlyCollection<Roundabout> All => _all.AsReadOnly();
    public static Roundabout Active {get; private set;}

    [SerializeField] float _radiusInner;
    [SerializeField] float _radiusOuter;
    [SerializeField] BoxCollider[] _boundaries;
    [SerializeField] Ai[] _aiPrefabs;
    [SerializeField] Vector2Int _minMaxAiSpawns;
    [SerializeField] bool _offsetCamera = false;

    [SerializeField] UiFade _instructions;

    public float RadiusInner => _radiusInner;
    public float RadiusOuter => _radiusOuter;
    public bool OffsetCamera => _offsetCamera;

    List<Road> _roads = new List<Road>();
    public ReadOnlyCollection<Road> Roads => _roads.AsReadOnly();

    public int HardpointCount => _boundaries.Length;

    void Awake(){
        if (!Active && _all.Count == 0){
            Activate();
        }
        _all.Add(this);
    }
    protected override void OnDestroy(){
        _all.Remove(this);
        base.OnDestroy();
    }

    public void FadeOutInstructions(){
        if (_instructions){
            _instructions.FadeOut(1);
        }
    }

    public void AddToRoad(Road r){
        float angle = 90-r.transform.eulerAngles.y;
        angle = Mathf.Repeat(angle, 360);
        Vector3 joinPoint = r.Joint2;
        Vector3 offset = Polar.FromPolar(Mathf.Deg2Rad * angle, RadiusOuter, Vector3.zero);
        Vector3 pos = joinPoint + offset;
        pos.y = 0;
        transform.position = pos;
        DisableBoundary(-r.transform.forward, false);
        _roads.Add(r);

        // Spawn AIs
        int spawns = Random.Range(_minMaxAiSpawns.x, _minMaxAiSpawns.y) + HighwayManagement.SpawnIncrement;
        for (int i=0; i<spawns; ++i){
            var ai = Instantiate<Ai>(_aiPrefabs[Random.Range(0, _aiPrefabs.Length)]);
            float theta = Random.Range(0,Mathf.PI*2);
            ai.transform.position = Polar.FromPolar(theta, RadiusOuter, transform.position);
            ai.transform.forward = Polar.PolarForward(theta, RadiusOuter);
        }
    }
    public void AddRoad(Road r, float angleDeg){
        DisableBoundary(r.transform.forward, true);
        _roads.Add(r);
    }
    void DisableBoundary(Vector3 fwd, bool gameObject){
        foreach (var b in _boundaries){
            var bFwd = b.transform.forward;
            if (Vector3.Angle(bFwd, fwd) < 5){
                if (gameObject){
                    b.gameObject.SetActive(false);
                } else {
                    b.enabled = false;
                }
            }
        }
    }
    public int GetBoundaryIndex(Vector3 forward){
        for (int i=0; i<HardpointCount; ++i){
            if (Vector3.Angle(forward, _boundaries[i].transform.forward) < 5){
                return i;
            }
        }
        return -1;
    }
    public void Activate(){
        if (Active != this){
            Active = this;
            foreach (var r in All){
                if (r != this){
                    Destroy(r.gameObject);
                }
            }
            foreach (var r in Road.All){
                if (!_roads.Contains(r)){
                    Destroy(r.gameObject);
                }
            }
        }
    }

    void OnDrawGizmos(){
        var gc = Gizmos.color;
        Gizmos.color = Color.red;
        Vector3 up = Vector3.up * 0.01f;
        Gizmos.DrawWireSphere(transform.position + up, _radiusInner);
        Gizmos.DrawWireSphere(transform.position + up, _radiusOuter);
        Gizmos.color = gc;
    }
}
