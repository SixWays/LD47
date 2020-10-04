using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathsUi : MonoBehaviour {
    static DeathsUi _i;
    public static void OnDeath(Player.DeathType type, bool dead){
        switch (type){
            case Player.DeathType.Boundary:
                DoDeath(_i._deathsBoundary, _i._quickDeathBoundary, dead);
                break;
            case Player.DeathType.Vehicle:
                DoDeath(_i._deathsVehicle, _i._quickDeathVehicle, dead);
                break;
            case Player.DeathType.Fuel:
                DoDeath(_i._deathsFuel, _i._quickDeathFuel, dead);
                break;
            case Player.DeathType.WrongWay:
                DoDeath(_i._deathsWrongWay, _i._quickDeathWrongWay, dead);
                break;
            case Player.DeathType.Sanity:
                DoDeath(_i._deathsSanity, _i._quickDeathSanity, dead);
                break;
        }
    }
    static void DoDeath(UiFade[] deaths, UiFade death, bool random){
        if (random){
            DoDeath(deaths[Random.Range(0, deaths.Length)], true);
            _i._deathSound.Play();
        } else {
            DoDeath(death, false);
            _i._quickDeathSound.Play();
        }
    }
    static void DoDeath(UiFade death, bool overlay){
        _i.StartCoroutine(_Fade());
        IEnumerator _Fade(){
            if (overlay){
                if (Roundabout.Active){
                    Roundabout.Active.FadeOutInstructions();
                }
                _i._wrapper.Fade();
                yield return new WaitForSeconds(_i._messageDelay);
            }
            death.Fade();
        }
    }

    [SerializeField] UiFade _wrapper;
    [SerializeField] float _messageDelay = 0.5f;
    [SerializeField] UiFade[] _deathsVehicle;
    [SerializeField] UiFade[] _deathsBoundary;
    [SerializeField] UiFade[] _deathsWrongWay;
    [SerializeField] UiFade[] _deathsFuel;
    [SerializeField] UiFade[] _deathsSanity;

    [SerializeField] UiFade _quickDeathVehicle;
    [SerializeField] UiFade _quickDeathBoundary;
    [SerializeField] UiFade _quickDeathWrongWay;
    [SerializeField] UiFade _quickDeathFuel;
    [SerializeField] UiFade _quickDeathSanity;

    [SerializeField] AudioSource _quickDeathSound;
    [SerializeField] AudioSource _deathSound;

    void Awake(){
        if (_i){
            Destroy(gameObject);
        } else {
            _i = this;
        }
    }
    void OnDestroy(){
        if (_i == this){
            _i = null;
        }
    }
}
