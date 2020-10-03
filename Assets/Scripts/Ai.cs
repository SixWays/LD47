using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai : Car {
    [SerializeField, Range(0f,1f)] float _exitChance;    

    protected override float CurrentRadius => _currentRoundabout.RadiusOuter;

    protected override void OnTriggerEnter(Collider c){
        base.OnTriggerEnter(c);

        if (_currentRoundabout){
            var r = c.GetComponent<Road>();
            if (r){
                if (Random.Range(0f,1f) < _exitChance){
                    _currentRoad = r;
                    _currentRoundabout = null;
                    Transfer(_nearestEntry, _currentRoad.GetForward(_nearestEntry));
                }
            }
        }
    }
}
