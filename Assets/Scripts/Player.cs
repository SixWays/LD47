using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Car {
    enum PlayerState {
        Travelling,
        Turning,
        Transferring,
        Dying
    }
    PlayerState _state = PlayerState.Travelling;

    [SerializeField] float _turnRadius;

    protected override float CurrentRadius => _currentRoundabout.RadiusInner;

    void Update(){
        switch (_state){
            case PlayerState.Travelling:
                if (_currentRoundabout){
                    if (Input.GetKeyDown(KeyCode.Space)){
                        // Start turning
                        _state = PlayerState.Turning;
                        _currentRoad = null;
                        _currentRoundabout = null;
                    }
                }
                break;
            case PlayerState.Turning:
                // d = st
                float turnArcLength = Speed * Time.deltaTime;
                // arc = theta r
                // theta = arc / r
                float dTheta = turnArcLength / _turnRadius;
                float dDeg = Mathf.Rad2Deg * dTheta;

                var ea = transform.eulerAngles;
                ea.y += dDeg;
                transform.eulerAngles = ea;
                
                transform.position += transform.forward * Speed * Time.deltaTime;
                break;
        }
    }
    protected override void OnTriggerEnter(Collider c){
        if (_state == PlayerState.Turning){
            var r = c.GetComponentInParent<Road>();
            if (r && c.CompareTag("Entryu")){
                _state = PlayerState.Transferring;
                Transfer(c.transform.position, r.GetForward(c.transform.position), ()=>{
                    _state = PlayerState.Travelling;
                    _currentRoad = r;
                });
            }
        } else if (_state != PlayerState.Dying) {
            base.OnTriggerEnter(c);
        }
    }
    void OnCollisionEnter(Collision c){
        if (_state != PlayerState.Dying && _state != PlayerState.Transferring && !c.collider.CompareTag("Floor")){
            Debug.LogFormat(c.collider, "DIE: {0}", c.collider.name);
            var rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.None;
            _state = PlayerState.Dying;
            enabled = false;
        }
    }

    void OnDrawGizmos(){
        Gizmos.DrawWireSphere(transform.position + transform.right * _turnRadius, _turnRadius);
    }
}
