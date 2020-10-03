using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighwayManagement : MonoBehaviour {
    static HighwayManagement _i;

    [SerializeField] Roundabout[] _roundabouts;
    [SerializeField] Road[] _roads;
    [SerializeField] int _minSpaceBetweenRoads;
    [SerializeField, Range(0f,1f)] float _roadChance;

    void Awake(){
        if (_i){
            Destroy(gameObject);
        } else {
            _i = this;
        }
    }

    [ContextMenu("Spawn")]
    public static void SpawnRoundabout(){
        var prefab = _i._roundabouts[Random.Range(0,_i._roundabouts.Length)];
        var clone = Instantiate<Roundabout>(prefab);
        clone.AddToRoad(Road.Active);
        Road.Active.RoundaboutAdded(clone);

        int existingRoadIndex = clone.GetBoundaryIndex(-Road.Active.transform.forward);

        Dictionary<int,Road> roads = new Dictionary<int,Road>();
        roads.Add(existingRoadIndex, Road.Active);

        for (int i=0; i<clone.HardpointCount; ++i){
            // Check space
            bool hasSpace = true;
            for (int j=0; j<=_i._minSpaceBetweenRoads; ++j){
                int prevRoadIndex = _Wrap(i-j);
                int nextRoadIndex = _Wrap(i+j);
                if (roads.ContainsKey(prevRoadIndex) || roads.ContainsKey(nextRoadIndex)){
                    hasSpace = false;
                    break;
                }
            }
            if (!hasSpace) continue;

            if (Random.Range(0f,1f) < _i._roadChance){
                var r = Instantiate<Road>(_i._roads[Random.Range(0, _i._roads.Length)]);
                float angle = i * 360/clone.HardpointCount;
                r.AddToRoundabout(clone, angle);
                clone.AddRoad(r, angle);
                roads.Add(i, r);
            }
        }

        var camPos = Camera.main.transform.position;
        camPos.x = clone.transform.position.x;
        camPos.z = clone.transform.position.z;
        Camera.main.transform.position = camPos;

        int _Wrap(int i){
            int max = clone.HardpointCount;
            if (i >= max){
                i -= max;
            }
            if (i < 0){
                i += max;
            }
            return i;
        }
    }
}
