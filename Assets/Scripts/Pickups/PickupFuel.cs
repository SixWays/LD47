﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupFuel : Pickup<float> {
    protected override void OnPickedUp(Player p){
        p.AddFuel(_value);
    }
}
