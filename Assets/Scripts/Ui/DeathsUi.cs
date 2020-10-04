using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathsUi : MonoBehaviour {
    static DeathsUi _i;
    public static void OnDeath(Player.DeathType type){
        switch (type){
            case Player.DeathType.Boundary:
                DoDeath(_i._deathsBoundary);
                break;
            case Player.DeathType.Vehicle:
                DoDeath(_i._deathsVehicle);
                break;
            case Player.DeathType.Fuel:
                DoDeath(_i._deathsFuel);
                break;
            case Player.DeathType.WrongWay:
                DoDeath(_i._deathsWrongWay);
                break;
            case Player.DeathType.Sanity:
                DoDeath(_i._deathsSanity);
                break;
        }
    }
    static void DoDeath(UiFade[] deaths){
        _i.StartCoroutine(_Fade());
        IEnumerator _Fade(){
            _i._wrapper.Fade();
            yield return new WaitForSeconds(_i._messageDelay);
            deaths[Random.Range(0, deaths.Length)].Fade();
        }
    }

    [SerializeField] UiFade _wrapper;
    [SerializeField] float _messageDelay = 0.5f;
    [SerializeField] UiFade[] _deathsVehicle;
    [SerializeField] UiFade[] _deathsBoundary;
    [SerializeField] UiFade[] _deathsWrongWay;
    [SerializeField] UiFade[] _deathsFuel;
    [SerializeField] UiFade[] _deathsSanity;

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
