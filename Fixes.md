<table>
    <tr>
        <td>Class</td>
        <td>Line</td>
        <td>Fix</td>
        <td>Reason</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>19</td>
        <td>.OrderByDescending(enemy => enemy.PushReloadTurns - asteroid.Steps(enemy) > 0)</td>
        <td>it is better to order by enemies who the asteroid can hit when they don't have a push</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>25</td>
        <td>.OrderBy(</td>
        <td>instead of orderbyDescending because it is better to hit the closest capsule that we can hit</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>31</td>
        <td>bestEnemy.Steps(asteroid) <= closestAsteroid.Steps(asteroid));</td>
        <td> put asteroid instead of pirate</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>40-56 (General)</td>
        <td>availableAsteroids.Remove(asteroid);</td>
        <td>we have to remove the asteroid from the available asteroids to make sure we don't push it more than once</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>63</td>
        <td>AsteroidHittingPirate(asteroid, pirate)</td>
        <td>we don't want to push it to the border if it is not hitting us</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>69</td>
        <td>if(AsteroidHittingPirate(asteroid, pirate))</td>
        <td>we don't want to push it randomly if it is not hitting us</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>121,13</td>
        <td>AsteroidHittingPirate</td>
        <td>Changed name to make it clearer</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>174</td>
        <td>int i = 1 </td>
        <td>changed starting steps to 1</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>178</td>
        <td>asteroid.Speed * (i-1)</td>
        <td>because the first turn the asteroid does not move it only gains the push distance</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>78</td>
        <td>!newAsteroidLocation.InMap()</td>
        <td>removed in map from inside the where</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>99</td>
        <td>.OrderBy(</td>
        <td> changed orderby to normal because we should target the closest capsule to the mothership that we can hit</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>154</td>
        <td>score</td>
        <td>changed the value to score since it already exists</td>
    </tr>
    <tr>
        <td>Asteroids.cs</td>
        <td>125</td>
        <td><</td>
        <td> changed <= to < because we want them to hit each other</td>
    </tr>
    <tr>
        <td>Bunkers.cs</td>
        <td></td>
        <td></td>
        <td></td>
    </tr>
</table>