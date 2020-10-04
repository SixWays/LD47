using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RoadBase : MonoBehaviour {
    List<Ai> _ais = new List<Ai>();

    public void RegisterAi(Ai a){
        if (!_ais.Contains(a)){
            _ais.Add(a);
        }
    }
    public void UnregisterAi(Ai a){
        _ais.Remove(a);
    }

    protected virtual void OnDestroy(){
        foreach (var a in _ais){
            if (a){
                Destroy(a.gameObject);
            }
        }
    }
}
