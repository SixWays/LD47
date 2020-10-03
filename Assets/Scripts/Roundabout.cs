using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roundabout : MonoBehaviour {
    [SerializeField] float _radiusInner;
    [SerializeField] float _radiusOuter;

    public float RadiusInner => _radiusInner;
    public float RadiusOuter => _radiusOuter;

    void OnDrawGizmos(){
        var gc = Gizmos.color;
        Gizmos.color = Color.red;
        Vector3 up = Vector3.up * 0.01f;
        Gizmos.DrawWireSphere(transform.position + up, _radiusInner);
        Gizmos.DrawWireSphere(transform.position + up, _radiusOuter);
        Gizmos.color = gc;
    }
}
