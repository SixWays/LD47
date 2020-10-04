using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai : Car {
    [SerializeField] Vector2 _minMaxSpeed;
    float _speed;
    protected override float speed => _speed;
    [SerializeField, Range(0f,1f)] float _exitChance;

    protected override float CurrentRadius => _currentRoundabout.RadiusOuter;

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
                    Transfer(_nearestEntry, _currentRoad.GetForward(_nearestEntry));
                }
            }
        } else {
            // Despawner
            var d = c.GetComponent<Despawner>();
            if (d){
                Destroy(gameObject);
            }
        }
    }
    protected override void OnEnteredRoundabout(Roundabout r){
        _currentRoundabout.UnregisterAi(this);
        r.RegisterAi(this);
    }
}
