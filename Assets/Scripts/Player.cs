using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Car {
    public enum PlayerState {
        Travelling,
        Turning,
        Transferring,
        Dying
    }
    PlayerState _state = PlayerState.Travelling;
    public PlayerState State => _state;

    public static Player Instance {get; private set;}

    [SerializeField] float _speed;
    protected override float speed => _speed;
    [SerializeField] float _turnRadius;

    protected override float CurrentRadius => _currentRoundabout.RadiusInner;

    void Start(){
        if (Instance){
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }
    void OnDestroy(){
        if (Instance == this){
            Instance = null;
        }
    }

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
                    r.PlayerEnteredRoad();
                });
            }
        } else if (_state != PlayerState.Dying) {
            base.OnTriggerEnter(c);
            // Transitioned to new roundabout
            if (_currentRoundabout && _currentRoad && Road.Active == _currentRoad){
                _currentRoad.PlayerExitedRoad();
                _currentRoundabout.Activate();
                StartCoroutine(_BlockRoad(_currentRoad, _currentRoundabout));
                _currentRoad = null;
            }
        }

        IEnumerator _BlockRoad(Road r, Roundabout rab){
            yield return new WaitForSeconds(0.25f);
            r.SetActiveRoundabout(rab, true);
        }
    }
    void OnCollisionEnter(Collision c){
        if (!_currentRoad && _state != PlayerState.Dying && _state != PlayerState.Transferring && !c.collider.CompareTag("Floor")){
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
