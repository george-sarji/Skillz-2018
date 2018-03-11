<table>
    <tr>
        <th>Class</th>
        <th>Line</th>
        <th>Fix</th>
        <th>Instead Of</th>
        <th>Reason</th>
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
    <tr>
        <td>Capsules.cs</td>
        <td>63</td>
        <td>+</td>
        <td>-</td>
        <td>Added taking the worst case scenario</td>
    </tr>
    <tr>
        <td>Capsules.cs</td>
        <td>89</td>
        <td>var bestWormhole = GetBestWormhole(myPirateWithCapsule, destination.Location);</td>
        <td>---</td>
        <td>we want the best wormhole to ensure that we push it to the best wormhole</td>
    </tr>
    <tr>
        <td>Capsules.cs</td>
        <td>89</td>
        <td>Location locationOfPush = null;
            if (bestWormhole != null)
            {
                var distance = DistanceThroughWormhole(myPirateWithCapsule.Location, destination, bestWormhole, game.GetActiveWormholes().Where(wormhole => wormhole != bestWormhole), 0, myPirateWithCapsule.MaxSpeed);
                locationOfPush = (distance > myPirateWithCapsule.Distance(destination)) ? destination.Location : bestWormhole.Location ;
            }
            else
            {
                locationOfPush = destination.Location;
            }</td>
        <td>var locationOfPush = myPirateWithCapsule.Location.Towards(destination, pusherPirate.PushDistance);</td>
        <td>in order to make sure that we push it towards the best possible location (wormhole or mothership)</td>
    </tr>
    <tr>
        <td>Capsules.cs</td>
        <td>90</td>
        <td>!IsWorthPushing(myPirateWithCapsule, pusherPirate)</td>
        <td>!IsWorthPushing(myPirateWithCapsule, pusherPirate, locationOfPush, destination.Location)</td>
        <td>removed unused parameters</td>
    </tr>
    <tr>
        <td>Capsule.cs</td>
        <td>103</td>
        <td>private bool IsWorthPushing(Pirate myPirateWithCapsule, Pirate piratePusher)</td>
        <td>private bool IsWorthPushing(Pirate myPirateWithCapsule, Pirate piratePusher, Location locationOfPush, Location destination)</td>
        <td>Reomved unused parameters</td>
    </tr>
    <tr>
        <td>Capsule.cs</td>
        <td>108</td>
        <td>-1</td>
        <td>---</td>
        <td>add -1 so we don't over push the capsule</td>
    </tr>
    <tr>
    <td>Priorities.cs</td>
        <td>45-55</td>
        <td>---</td>
        <td>if (LocationType.MyPirate == Type)
                {
                    var bestMothership = game.GetMyMotherships().OrderBy(mothership => pirate.Steps(mothership) / (int) ((double) mothership.ValueMultiplier).Sqrt()).FirstOrDefault();;
                    if (!CanCatchUpAndPush(pirate, (Pirate) TargetLocationObject, bestMothership.Location))
                    {
                        score += MAX_PRIORITY;
                        return score;
                    }
                    score += this.Priority;
                    return score;
                }</td>
        <td>it is already done inside handle if pirate can reach</td>
    </tr>
    <tr>
    <td>Priorities.cs </td>
    <td>34 </td>
    <td>Location intercept = bot.GetPirateOptimalInterception(pirate, destinationPirate, destination); </td>
    <td>Location intercept = Interception(destinationPirate.Location, destination, pirate.Location); </td>
    <td>Added PirateInterception which considers pirate states </td>
    </tr>
    <tr>
    <td>Utilities.cs </td>
    <td>80 </td>
    <td>if (capsuleHolder.InRange(mothership, mothership.UnloadRange + capsuleHolder.MaxSpeed) </td>
    <td>if (capsuleHolder.InRange(mothership, mothership.UnloadRange * 3) </td>
    <td>Now represents the capsuleHolder's capability of unloading in the next turn </td>
    </tr>
	<tr>
		<td>Pirates.cs</td>
		<td>16</td>
		<td>game.GetEnemyLivingPirates().Count(enemyPirate => enemy != enemyPirate && enemy.Distance(enemyPirate) <= game.StickyBombExplosionRange) > 2</td>
		<td>game.GetEnemyLivingPirates().Where(enemyPirate => enemy != enemyPirate && enemy.Distance(enemyPirate) < game.StickyBombExplosionRange).Count() > 2</td>
		<td>Used to include the tip of sticky bomb range, attack more pirates (minimum 2) and call less functions.</td>
	</tr>
	<tr>
		<td>Pirates.cs</td>
		<td>36</td>
		<td>var bestMothership = GetEnemyBestMothershipThroughWormholes(capsuleHolder);
                if (capsuleHolder.StickyBombs.Any() && capsuleHolder.StickyBombs.OrderBy(bomb => bomb.Countdown).First().Countdown < capsuleHolder.Steps(bestMothership))</td>
		<td>if (capsuleHolder.StickyBombs.Any())</td>
		<td>This is to attack pirates if they wont blow up before reaching the mothership. Needs to be tested.</td>
	</tr>
	<tr>
		<td>Pirates.cs</td>
		<td>44-45</td>
		<td>.Where(enemy => !enemy.StickyBombs.Any() &&
                    GetEnemiesInBombRange(enemy).Count() >= 2)</td>
		<td>.Where(enemy => GetEnemiesInBombRange(enemy).Count() >= 2 &&
                    !enemy.StickyBombs.Any())</td>
		<td>Switch order of functions to use less time.</td>
	</tr>
	<tr>
		<td>Pirates.cs</td>
		<td>61</td>
		<td>var bestMothership = GetMyBestMothershipThroughWormholes(pirate);</td>
		<td>var bestMothership = game.GetMyMotherships().OrderBy(mothership => pirate.Steps(mothership) / (int) ((double) mothership.ValueMultiplier).Sqrt()).FirstOrDefault();</td>
		<td>Updated to new function.</td>
	</tr>
	<tr>
    <td>Utilities.cs</td>
        <td>48</td>
        <td>return availablePirates.Count(p => p.CanPush(pirate));</td>
        <td>return availablePirates.Where(p => p.CanPush(pirate)).Count();</td>
        <td>Less called functions.</td>
    </tr>
    <tr>
        <td>Utilities.cs</td>
        <td>64</td>
        <td>return game.GetEnemyLivingPirates().Count(p => IsOnTheWay(myPirate.Location, b, p.Location, p.MaxSpeed) && myPirate.Steps(p) < p.PushReloadTurns);</td>
        <td>return game.GetEnemyLivingPirates().Where(p => IsOnTheWay(myPirate.Location, b, p.Location, p.MaxSpeed) && myPirate.Steps(p) < p.PushReloadTurns).ToList().Count;</td>
        <td>Less called functions</td>
    </tr>
    <tr>
        <td>Utilities.cs</td>
        <td>69</td>
        <td>return game.GetEnemyLivingPirates().Count(enemy => enemy.CanPush(pirate));</td>
        <td>return game.GetEnemyLivingPirates().Where(enemy => enemy.CanPush(pirate)).Count();</td>
        <td>Less called functions</td>
    </tr>
    <tr>
        <td>Utilities.cs</td>
        <td>74</td>
        <td>return game.GetEnemyLivingPirates().Count(enemy => enemy.InRange(location, enemy.PushRange) && enemy.PushReloadTurns != 0);</td>
        <td>return game.GetEnemyLivingPirates().Where(enemy => enemy.InRange(location, enemy.PushRange) && enemy.PushReloadTurns != 0).Count();</td>
        <td>Less called functions</td>
    </tr>
	<tr>
		<td>Utilities.cs</td>
		<td>85</td>
		<td>-</td>
		<td>availablePirates.Remove(capsuleHolder);</td>
		<td>Review this and check if it doesnt cause any ignored actions.</td>
	</tr>
    <tr>
        <td>Utilities.cs</td>
        <td>94</td>
        <td>return game.GetEnemyLivingPirates().Count(enemy => enemy.Distance(pirate) < game.PushRange) > game.NumPushesForCapsuleLoss;</td>
        <td>return game.GetEnemyLivingPirates().Where(enemy => enemy.Distance(pirate) < game.PushRange).Count() > game.NumPushesForCapsuleLoss;</td>
        <td>Less called functions</td>
    </tr>
    <tr>
        <td>Utilities.cs</td>
        <td>98</td>
        <td>var numOfNearbyEnemyPushers = game.GetEnemyLivingPirates().Count(enemy => enemy.InRange(pirate, enemy.PushRange + game.PirateMaxSpeed) && enemy.PushReloadTurns <= 2);</td>
        <td>int numOfNearbyEnemyPushers = game.GetEnemyLivingPirates().Where(enemy => enemy.InRange(pirate, enemy.PushRange + game.PirateMaxSpeed) && enemy.PushReloadTurns <= 2).Count();</td>
        <td>Less called functions</td>
    </tr>
    <tr>
        <td>Initializer.cs</td>
        <td>54-58</td>
        <td>myPiratesWithCapsulePushes = game.GetMyLivingPirates().Where(p => p.HasCapsule()).ToDictionary(pirate => pirate, pirate => 0);</td>
        <td>myPiratesWithCapsulePushes = new Dictionary<Pirate, int>();
            foreach (Pirate pirate in game.GetMyLivingPirates().Where(p => p.HasCapsule()))
            {
                myPiratesWithCapsulePushes.Add(pirate, 0);
            }</td>
        <td>Less time used</td>
    </tr>
    <tr>
		<td>Pirates.cs</td>
		<td>57</td>
		<td>var PiratesWithCapsule = game.GetMyLivingPirates().Where(p => p.HasCapsule());</td>
		<td>List<\Pirate\> PiratesWithCapsule = game.GetMyLivingPirates().Where(p => p.HasCapsule()).ToList();</td>
		<td>Used less functions (removed copying to list)</td>
	</tr>
    <tr>
		<td>Pirates.cs</td>
		<td>89-124</td>
		<td>IEnumerable + it's methods</td>
		<td>List + it's methods</td>
		<td>Change everything from lists to IEnumerable and make the .AddRange into .Concat</td>
	</tr>
</table>
