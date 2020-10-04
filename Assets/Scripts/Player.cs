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
    public enum DeathType {
        Vehicle,
        Boundary,
        WrongWay,
        Fuel,
        Sanity
    }
    PlayerState _state = PlayerState.Travelling;
    public PlayerState State => _state;

    public static Player Instance {get; private set;}

    public float Fuel {get; private set;} = 1;
    public float Sanity {get; private set;} = 1;

    [SerializeField] float _speed;
    [SerializeField] float _roadSpeed;
    protected override float speed {
        get {
            if (_currentRoad && !_currentRoundabout){
                return _speed * _roadSpeed;
            }
            return _speed;
        }
    }
    [SerializeField] float _turnRadius;
    [SerializeField] float _invulnTime=0.5f;
    [SerializeField] float _fuelTime = 30f;
    [SerializeField] float _sanityTime = 60f;

    [SerializeField] UnityEngine.Events.UnityEvent _onDie;

    protected override float CurrentRadius => _currentRoundabout.RadiusInner;

    float _timeVulnerable;

    void Start(){
        if (Instance){
            Destroy(gameObject);
        } else {
            Instance = this;
            _timeVulnerable = Time.time + _invulnTime;
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

                    if (HighwayManagement.UseFuel){
                        Fuel -= (Time.deltaTime / _fuelTime);
                        if (Fuel <= 0){
                            Fuel = 0;
                            Die(DeathType.Fuel);
                        }
                    }
                    if (HighwayManagement.UseSanity){
                        Sanity -= HighwayManagement.SanityScale * (Time.deltaTime / _sanityTime);
                        if (Sanity <= 0){
                            Sanity = 0;
                            Die(DeathType.Sanity);
                        }
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
    public void AddFuel(float f){
        Fuel += f;
        Fuel = Mathf.Clamp01(Fuel);
    }
    void Die(DeathType type){
        var rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        _state = PlayerState.Dying;
        enabled = false;
        Debug.LogFormat(this, "Death: {0}", type);
        HighwayManagement.OnPlayerDie(type);
        _onDie.Invoke();
    }
    protected override void OnTriggerEnter(Collider c){
        var p = c.GetComponent<PickupBase>();
        if (p){
            p.OnPickup(this);
            return;
        }

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
                _timeVulnerable = Time.time + _invulnTime;
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
        if (Time.time > _timeVulnerable && !_currentRoad && _state != PlayerState.Dying && _state != PlayerState.Transferring && !c.collider.CompareTag("Floor")){
            Debug.LogFormat(c.collider, "DIE: {0}", c.collider.name);
            if (c.collider.GetComponentInParent<Ai>()){
                Die(DeathType.Vehicle);
            } else if (c.collider.CompareTag("Block")){
                Die(DeathType.WrongWay);
            } else {
                Die(DeathType.Boundary);
            }
        }
    }

    void OnDrawGizmos(){
        Gizmos.DrawWireSphere(transform.position + transform.right * _turnRadius, _turnRadius);
    }
}
