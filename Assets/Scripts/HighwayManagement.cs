﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HighwayManagement : MonoBehaviour {
    static HighwayManagement _i;

    public static float SpeedScale => _i._speedScale;
    public static float SanityScale => _i._sanityScale;
    public static int Score => _i._score;
    public static int Lives => _i._lives;
    public static bool UseFuel => _i._activeFuel;
    public static bool UseLives => _i._activeLives;
    public static bool UseSanity => _i._activeSanity;
    public static float UiAppearTime => _i._uiAppearTime;

    [SerializeField] Roundabout[] _roundabouts;
    [SerializeField] Road[] _roads;
    [SerializeField] int _minSpaceBetweenRoads;
    [SerializeField, Range(0f,1f)] float _roadChance;
    [SerializeField] float _camMoveTime;
    [SerializeField] float _speedFactor;
    [SerializeField] float _sanityFactor;
    [SerializeField] Player _playerPrefab;
    [SerializeField] float _respawnTime=2;
    [SerializeField] float _uiAppearTime = 1;

    int _roundaboutIndex = 0;
    int _score = 0;
    int _lives = 0;
    bool _dead = false;

    bool _activeLives, _activeFuel, _activeSanity;

    float _speedScale = 1;
    float _sanityScale = 1;

    void Awake(){
        if (_i){
            Destroy(gameObject);
        } else {
            _i = this;
        }
    }
    void Update(){
        if (Input.GetKeyDown(KeyCode.Space) && Player.Instance.State == Player.PlayerState.Dying && _dead){
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    [ContextMenu("Spawn")]
    public static void SpawnRoundabout(){
        ++_i._score;
        _i._speedScale += _i._speedFactor;

        if (UseSanity){
            _i._sanityScale += _i._sanityFactor;
        }

        // Go sequentially through prefabs to allow tutorialisation
        var prefab = _i._roundabouts[_i._roundaboutIndex];
        ++_i._roundaboutIndex;
        // Final prefab is generic roundabout for the rest of the game
        if (_i._roundaboutIndex >= _i._roundabouts.Length){
            _i._roundaboutIndex = _i._roundabouts.Length-1;
        }

        var clone = Instantiate<Roundabout>(prefab);
        clone.AddToRoad(Road.Active);
        Road.Active.RoundaboutAdded(clone);

        int existingRoadIndex = clone.GetBoundaryIndex(-Road.Active.transform.forward);

        Dictionary<int,Road> roadIndices = new Dictionary<int,Road>();
        roadIndices.Add(existingRoadIndex, Road.Active);
        List<Road> spawnedRoads = new List<Road>();

        while (roadIndices.Count == 1){   // brute force to ensure there's at least one available road
            for (int i=0; i<clone.HardpointCount; ++i){
                // Check space
                bool hasSpace = true;
                for (int j=0; j<=_i._minSpaceBetweenRoads; ++j){
                    int prevRoadIndex = _Wrap(i-j);
                    int nextRoadIndex = _Wrap(i+j);
                    if (roadIndices.ContainsKey(prevRoadIndex) || roadIndices.ContainsKey(nextRoadIndex)){
                        hasSpace = false;
                        break;
                    }
                }
                if (!hasSpace) continue;

                if (Random.Range(0f,1f) < _i._roadChance){
                    roadIndices.Add(i, null);
                } 
            }
        }
        foreach (var a in roadIndices){
            if (!a.Value){
                var r = Instantiate<Road>(_i._roads[Random.Range(0, _i._roads.Length)]);
                float angle = a.Key * 360/clone.HardpointCount;
                r.AddToRoundabout(clone, angle);
                clone.AddRoad(r, angle);
                spawnedRoads.Add(r);
                // Don't set active on active road so as not to block
                r.SetActiveRoundabout(clone, false);
            }
        }

        _i.StartCoroutine(_MoveCamera());

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

        IEnumerator _MoveCamera(){
            var cam = Camera.main.transform;
            var camStart = cam.position;
            var camEnd = camStart;
            camEnd.x = clone.transform.position.x;
            camEnd.z = clone.transform.position.z;

            float t = 0;
            Vector3 slew = Vector3.zero;
            while (Vector3.Distance(cam.position, camEnd) > 0.01f){
                cam.position = Vector3.SmoothDamp(cam.position, camEnd, ref slew, _i._camMoveTime);
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            cam.position = camEnd;
        }
    }

    public static void OnPlayerDie(Player.DeathType type){
        --_i._lives;
        if (_i._lives < 0){
            _i._lives = 0;
            _i._dead = true;
        } else {
            _i.StartCoroutine(_Respawn());
        }

        IEnumerator _Respawn(){
            yield return new WaitForSeconds(_i._respawnTime);
            Destroy(Player.Instance.gameObject);
            var player = Instantiate<Player>(_i._playerPrefab);
            player.transform.position = Polar.FromPolar(0, Roundabout.Active.RadiusOuter, Roundabout.Active.transform.position);
            player.transform.forward = Polar.PolarForward(0, Roundabout.Active.RadiusOuter);
        }
    }

    public static void AddLives(int lives){
        if (UseLives){
            _i._lives += lives;
        }
    }

    public static void ActivateFuel(){
        if (!UseFuel){
            _i._activeFuel = true;
        }
    }
    public static void ActivateLives(){
        if (!UseLives){
            _i._activeLives = true;
        }
    }
    public static void ActivateSanity(){
        if (!UseSanity){
            _i._activeSanity = true;
        }
    }
}
