using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickupBase : MonoBehaviour {
    public void OnPickup(){
        OnPickedUp();
        Destroy(gameObject);
    }
    protected abstract void OnPickedUp();
}

[RequireComponent(typeof(SphereCollider))]
public abstract class Pickup<T> : PickupBase {
    [SerializeField] GameObject _wrapper;
    [SerializeField] float _flashTime;
    [SerializeField] float _flashRate;
    [SerializeField] float _spinRate;
    [SerializeField] protected T _value;

    IEnumerator Start(){
        GetComponent<SphereCollider>().isTrigger = true;
        float tFlash = 1f/_flashRate;
        int flashes = Mathf.RoundToInt(_flashRate*_flashTime);
        _wrapper.SetActive(true);
        for (int i=0; i<flashes; ++i){
            yield return new WaitForSeconds(tFlash);
            _wrapper.SetActive(!_wrapper.activeSelf);
        }
        _wrapper.SetActive(true);
    }
    void Update(){
        var lea = transform.localEulerAngles;
        lea.y += _spinRate * Time.deltaTime;
        transform.localEulerAngles = lea;
    }
}
