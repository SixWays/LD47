using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flash : MonoBehaviour {
    [SerializeField] float _rate;
    MeshRenderer _mr;
    IEnumerator Start() {
        _mr = GetComponent<MeshRenderer>();
        var wfs = new WaitForSeconds(1f/_rate);
        while (true){
            yield return wfs;
            _mr.enabled = !_mr.enabled;
        }
    }
    void OnDisable(){
        _mr.enabled = true;
    }
}
