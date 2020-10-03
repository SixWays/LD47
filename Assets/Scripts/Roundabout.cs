using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roundabout : MonoBehaviour {
    [SerializeField] float _radiusInner;
    [SerializeField] float _radiusOuter;
    [SerializeField] BoxCollider[] _boundaries;

    public float RadiusInner => _radiusInner;
    public float RadiusOuter => _radiusOuter;

    public void DeactivateBoundary(params int[] indices){
        foreach (var b in _boundaries){
            b.enabled = true;
        }
        foreach (var i in indices){
            _boundaries[i].enabled = false;
        }
    }

    void OnDrawGizmos(){
        var gc = Gizmos.color;
        Gizmos.color = Color.red;
        Vector3 up = Vector3.up * 0.01f;
        Gizmos.DrawWireSphere(transform.position + up, _radiusInner);
        Gizmos.DrawWireSphere(transform.position + up, _radiusOuter);
        Gizmos.color = gc;
    }
}
