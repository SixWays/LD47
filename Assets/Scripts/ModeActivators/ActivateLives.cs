using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateLives : MonoBehaviour {
    IEnumerator Start(){
        yield return new WaitForSeconds(HighwayManagement.UiAppearTime);
        HighwayManagement.ActivateLives();
    }
}
