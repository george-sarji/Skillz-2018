PushBombCarriers():
    foreach (bomb that's going to explode in <=2 turns) {
        var pushers = myPirates.Where(CanPush(bomb));
        if (pushers.Any()) {
            push towards enemy capsule / concentration
        }
    }

If you're a normal pirate and there's a bomb nearby, consider switching states to heavy so that you can push the bomb as far as possible.
I.e., make a change in wantToBeHeavy?
Downsides: If there's another heavy pirate nearby, he might push it instead (swap for nothing).
Also, after swapping the bomb might not be in the push range anymore so you might explode with it.

Possible additions to the game:

Blackholes - when a pirate is in its range, it sucks in the pirate

Powerups: Speed, Shield, Push (distance / range / forCapsuleLoss)
if enemy has powerup, need to treat him differently

Value Multiplier: 

Neutral Cities: 