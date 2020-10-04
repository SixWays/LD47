using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai : Car {
    protected override bool _canStartOnRoad => true;

    [SerializeField] Vector2 _minMaxSpeed;
    float _speed;
    protected override float speed => _speed;
    [SerializeField, Range(0f,1f)] float _exitChance;

    protected override float CurrentRadius => _currentRoundabout.RadiusOuter;
    bool _hasHadRoad = false;

    void Start(){
        _speed = Random.Range(_minMaxSpeed.x, _minMaxSpeed.y);
    }

    protected override void OnTriggerEnter(Collider c){
        base.OnTriggerEnter(c);

        if (_currentRoundabout){
            var r = c.GetComponent<Road>();
            if (r){
                if (Random.Range(0f,1f) < _exitChance){
                    _currentRoad = r;
                    _currentRoad.RegisterAi(this);
                    _currentRoundabout.UnregisterAi(this);
                    _currentRoundabout = null;
                    _currentRoadFwd = _currentRoad.GetForward(_nearestEntry);
                    Transfer(_nearestEntry, _currentRoadFwd);
                }
            }
        } else {
            // Despawner
            var d = c.GetComponent<Despawner>();
            if (d){
                Destroy(gameObject);
            }
        }

        _hasHadRoad = _currentRoad || _currentRoundabout;
    }
    void Update(){
        if (_hasHadRoad && !_currentRoad && !_currentRoundabout){
            Destroy(gameObject);
        }
    }
    protected override void OnEnteredRoundabout(Roundabout r){
        _currentRoundabout.UnregisterAi(this);
        r.RegisterAi(this);
        _currentRoadFwd = Vector3.zero;
    }    
}
