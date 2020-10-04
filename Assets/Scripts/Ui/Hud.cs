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

    [SerializeField] AudioSource _livesIncreaseSound;

    [SerializeField] float _flashTime;
    [SerializeField] float _flashRate;

    Slider _fuelSlider, _sanitySlider;

    float _lastFuel, _lastSanity;
    int _lastLives;
    bool _wasFuelActive, _wasLivesActive, _wasSanityActive;

    IEnumerator Start(){
        _fuelSlider = _fuel.GetComponentInChildren<Slider>();
        _sanitySlider = _sanity.GetComponentInChildren<Slider>();
        
        _wasFuelActive = HighwayManagement.UseFuel;
        _wasLivesActive = HighwayManagement.UseLives;
        _wasSanityActive = HighwayManagement.UseSanity;

        _fuel.SetActive(_wasFuelActive);
        _lives.SetActive(_wasLivesActive);
        _sanity.SetActive(_wasSanityActive);

        _lastLives = HighwayManagement.Lives;
        _livesText.text = _lastLives.ToString();

        int score = HighwayManagement.Score;
        bool wait = false;
        if (score > 0){
            --score;
            wait = true;
        }
        _scoreText.text = score.ToString();

        if (wait){
            yield return new WaitForSeconds(1f);
            _scoreText.text = (score+1).ToString();
        }
    }
    void Update() {
        if (!Player.Instance) return;
        
        if (Player.Instance.Fuel != _lastFuel){
            _fuelSlider.normalizedValue = _lastFuel = Player.Instance.Fuel;
        }
        if (Player.Instance.Sanity != _lastSanity){
            _sanitySlider.normalizedValue = _lastSanity = Player.Instance.Sanity;
        }

        // madness

        if (HighwayManagement.UseFuel && !_wasFuelActive){
            StartCoroutine(_Flash(_fuel));
            _wasFuelActive = true;
        }
        if (HighwayManagement.UseLives && !_wasLivesActive){
            StartCoroutine(_Flash(_lives));
            _wasLivesActive = true;
        }
        if (HighwayManagement.UseSanity && !_wasSanityActive){
            StartCoroutine(_Flash(_sanity));
            _wasSanityActive = true;
        }

        if (HighwayManagement.Lives != _lastLives){
            if (HighwayManagement.Lives > _lastLives){
                _livesIncreaseSound.Play();
            }
            _lastLives = HighwayManagement.Lives;
            _livesText.text = _lastLives.ToString();
            StartCoroutine(_Flash(_lives));
        }

        IEnumerator _Flash(GameObject go){
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
