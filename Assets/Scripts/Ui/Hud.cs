using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Hud : MonoBehaviour {
    [SerializeField] GameObject _fuel;
    [SerializeField] GameObject _sanity;
    [SerializeField] GameObject _lives;
    [SerializeField] TextMeshProUGUI _livesText;
    [SerializeField] GameObject _score;
    [SerializeField] TextMeshProUGUI _scoreText;

    [SerializeField] float _flashTime;
    [SerializeField] float _flashRate;

    Slider _fuelSlider, _sanitySlider;

    float _lastFuel, _lastMadness;
    int _lastLives;
    bool _wasFuelActive, _wasLivesActive, _wasSanityActive;

    void Start(){
        _fuelSlider = _fuel.GetComponentInChildren<Slider>();
        _sanitySlider = _sanity.GetComponentInChildren<Slider>();
        
        _wasFuelActive = HighwayManagement.UseFuel;
        _wasLivesActive = HighwayManagement.UseLives;
        _wasSanityActive = HighwayManagement.UseSanity;

        _fuel.SetActive(_wasFuelActive);
        _lives.SetActive(_wasLivesActive);
        _sanity.SetActive(_wasSanityActive);

        _scoreText.text = HighwayManagement.Score.ToString();
    }
    void Update() {
        if (Player.Instance.Fuel != _lastFuel){
            _fuelSlider.normalizedValue = _lastFuel = Player.Instance.Fuel;
        }
        if (HighwayManagement.Lives != _lastLives){
            _lastLives = HighwayManagement.Lives;
            _livesText.text = _lastLives.ToString();
        }

        // madness

        if (HighwayManagement.UseFuel && !_wasFuelActive){
            StartCoroutine(Flash(_fuel));
            _wasFuelActive = true;
        }
        if (HighwayManagement.UseLives && !_wasLivesActive){
            StartCoroutine(Flash(_lives));
            _wasLivesActive = true;
        }
        if (HighwayManagement.UseSanity && !_wasSanityActive){
            StartCoroutine(Flash(_sanity));
            _wasSanityActive = true;
        }

        IEnumerator Flash(GameObject go){
            float tFlash = 1f/_flashRate;
            int flashes = Mathf.RoundToInt(_flashRate*_flashTime);
            go.SetActive(true);
            for (int i=0; i<flashes; ++i){
                yield return new WaitForSeconds(tFlash);
                go.SetActive(!go.activeSelf);
            }
            go.SetActive(true);
        }
    }
}
