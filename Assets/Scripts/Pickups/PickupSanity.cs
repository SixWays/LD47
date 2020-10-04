using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSanity : Pickup<float> {
    protected override void OnPickedUp(){
        Player.Instance.AddFuel(_value);
    }
}
