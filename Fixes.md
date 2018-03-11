<table>
    <tr>
        <td>Class</td>
        <td>Line</td>
        <td>Fix</td>
        <td>Instead Of</td>
        <td>Reason</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>19</td>
        <td>.OrderByDescending(enemy => enemy.PushReloadTurns - asteroid.Steps(enemy) > 0)</td>
        <td>enemy => enemy.PushReloadTurns > 0</td>
        <td>it is better to order by enemies who the asteroid can hit when they don't have a push</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>25</td>
        <td>.OrderBy(</td>
        <td>.OrderByDescending(</td>
        <td>instead of orderbyDescending because it is better to hit the closest capsule that we can hit</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>29</td>
        <td>var oppositeDirection = asteroid.Direction.Multiply(-1);</td>
        <td>var oppositeDirection = new Location(asteroid.Location.Row * (-1), asteroid.Location.Col * (-1));</td>
        <td>Better calculation using API</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>31</td>
        <td>bestEnemy.Steps(asteroid) <= closestAsteroid.Steps(asteroid));</td>
        <td>bestEnemy.Distance(pirate) <= closestAsteroid.Distance(pirate)</td>
        <td> put asteroid instead of pirate</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>40-56 (General)</td>
        <td>availableAsteroids.Remove(asteroid);</td>
        <td> ----- </td>
        <td>we have to remove the asteroid from the available asteroids to make sure we don't push it more than once</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>63</td>
        <td>AsteroidHittingPirate(asteroid, pirate)</td>
        <td> ----- </td>
        <td>we don't want to push it to the border if it is not hitting us</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>69</td>
        <td>if(AsteroidHittingPirate(asteroid, pirate))</td>
        <td> ----- </td>
        <td>we don't want to push it randomly if it is not hitting us</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>78</td>
        <td>if(!newAsteroidLocation.InMap() &&</td>
        <td>.Where(p => !newAsteroidLocation.InMap() ? false</td>
        <td>removed in map from inside the where</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>99</td>
        <td>.OrderBy(</td>
        <td>.OrderByDescending(</td>
        <td> changed orderby to normal because we should target the closest capsule to the mothership that we can hit</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>121,13</td>
        <td>AsteroidHittingPirate</td>
        <td>AsteroidHeadingTowardsPirate</td>
        <td>Changed name to make it clearer</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>125</td>
        <td><</td>
        <td><=</td>
        <td> changed <= to < because we want them to hit each other</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>154</td>
        <td>score</td>
        <td> bestScore = asteroid.Steps(bestCapsuleForAsteroid) + ...</td>
        <td>changed the value to score since it already exists</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>174</td>
        <td>int i = 1 </td>
        <td>int i = 0</td>
        <td>changed starting steps to 1</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>178</td>
        <td>asteroid.Speed * (i-1)</td>
        <td>asteroid.Speed * i</td>
        <td>because the first turn the asteroid does not move it only gains the push distance</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>178</td>
        <td>asteroid.Size</td>
        <td> (int) (asteroid.Size * 0.8)</td>
        <td> the problem of the asteroid not killing the pirate was caused by another reason</td>
    </tr>
    <tr>
        <td>Bunkers.cs</td>
        <td>23</td>
        <td>.Where(p => capsule.Holder.Steps(mothership) > p.PushReloadTurns)</td>
        <td>.Where(p => p.Steps(mothership) > p.PushReloadTurns)</td>
        <td>add the wait turns until we get to the enemy pirates instead of the p.Steps(mothership)</td>
    </tr>
    <tr>
        <td>Bunkers.cs</td>
        <td>44</td>
        <td>if (requiredPiratesCount == count)</td>
        <td>---</td>
        <td>Deleted the if because we want to sort them in any case</td>
    </tr>
    <tr>
        <td>Bunkers.cs</td>
        <td>53</td>
        <td> var friendlyStepsNeeded = pirate.Steps(bestWormhole.Partner) * 2 + 1 + pirate.PushReloadTurns + game.PushMaxReloadTurns;</td>
        <td>System.Math.Max(pirate.Steps(bestWormhole.Partner.Location.Towards(pirate, pirate.PushRange)), pirate.PushReloadTurns) +
                                System.Math.Max(pirate.Steps(bestWormhole.Partner.Location.Towards(pirate, pirate.PushRange)), game.PushMaxReloadTurns);</td>
        <td>the previous calculation was faulty</td>
    </tr>
    <tr>
        <td>Bunkers.cs</td>
        <td>55</td>
        <td>capsule.Holder.Steps(bestWormhole) + (bestWormhole.Partner.Distance(capsule) / capsule.Holder.MaxSpeed);</td>
        <td>var enemyStepsNeeded = capsule.Holder.Steps(mothership);</td>
        <td>the previous calculation was faulty</td>
    </tr>
    <tr>
        <td>Bunkers.cs</td>
        <td>67</td>
        <td>bunkerCount[mothership] * game.PushDistance /2</td>
        <td>bunkerCount[mothership].Power(2) * game.PushRange</td>
        <td>a better calculation to take into consideration the heavy pirates' push</td>
    </tr>
    <tr>
        <td>Bunkers.cs</td>
        <td>69-72</td>
        <td>Make sure to review the * 2</td>
        <td></td>
        <td></td>
    </tr>
    <tr>
        <td>SmartSailing.cs</td>
        <td>21</td>
        <td>if (option.InMap() && !IsInDanger(option, destination.GetLocation(), pirate))</td>
        <td>if (!IsInDanger(option, destination.GetLocation(), pirate) && option.InMap())</td>
        <td>Provides more run time for the bot</td>
    <tr>
    <tr>
        <td>SmartSailing.cs</td>
        <td>42</td>
        <td>-</td>
        <td>bool hitting = false;</td>
        <td>Removed to improve timing. Look changes below</td>
    </tr>
    <tr>
        <td>SmartSailing.cs</td>
        <td>46</td>
        <td>return true;</td>
        <td>hitting = true;</td>
        <td>Improving timing of operation.</td>
    </tr>
    <tr>
        <td>SmartSailing.cs</td>
        <td>47</td>
        <td>return false;</td>
        <td>return hitting;</td>
        <td>Improving timing of operation.</td>
    </tr>
    <tr>
        <td>SmartSailing.cs</td>
        <td>64-65</td>
        <td>(!myPirate.HasCapsule()) ? false : game.GetEnemyLivingPirates().Count(enemy => enemy.InRange(loc, enemy.PushRange + enemy.MaxSpeed) &&
                enemy.PushReloadTurns<enemy.Steps(loc))>= myPirate.NumPushesForCapsuleLoss;</td>
        <td>return game.GetEnemyLivingPirates().Count(enemy => enemy.InRange(loc, enemy.PushRange + enemy.MaxSpeed) &&
                enemy.PushReloadTurns<enemy.Steps(loc))>= myPirate.NumPushesForCapsuleLoss;</td>
        <td>Added to check if the pirate has capsule to show if in enemy danger</td>
    </tr>
</table>