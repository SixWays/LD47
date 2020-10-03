using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour {
    [SerializeField] Transform _entry1;
    [SerializeField] Transform _entry2;
    [SerializeField] Transform _exit1;
    [SerializeField] Transform _exit2;

    [SerializeField] Transform _joint1;
    [SerializeField] Transform _joint2;

    public Vector3 Entry1 => _entry1.position;
    public Vector3 Entry2 => _entry2.position;
    public Vector3 Exit1 => _exit1.position;
    public Vector3 Exit2 => _exit2.position;

    public Vector3 Joint1 => _joint1.position;
    public Vector3 Joint2 => _joint2.position;

    public Vector3 GetForward(Vector3 position){
        float dEntry1 = Vector3.Distance(position, Entry1);
        float dEntry2 = Vector3.Distance(position, Entry2);

        float min = Mathf.Min(dEntry1, dEntry2);
        return (Mathf.Approximately(min, dEntry1)) ? _Fwd(Exit1) : _Fwd(Exit2);

        Vector3 _Fwd(Vector3 target){
            return (target - position).normalized;
        }
    }

    void OnDrawGizmos(){
        var gc = Gizmos.color;
        Gizmos.color = Color.green;
        Vector3 up = Vector3.up * 0.01f;
        if (_entry1 && _exit1){
            Gizmos.DrawLine(Entry1+up, Exit1+up);
        }
        if (_exit2 && _exit2){
            Gizmos.DrawLine(Entry2+up, Exit2+up);
        }
        Gizmos.color = Color.blue;
        if (_joint1 && _joint2){
            Gizmos.DrawLine(Joint1+up, Joint2+up);
        }
        Gizmos.color = gc;
    }
}
