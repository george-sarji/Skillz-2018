# Skillz 2018
This repository contains the code for the bot for the Skill 2018 cyber competition.

## Bot documentation:
<table>
<tr>
    <td>Class</td>
    <td>Name</td>
    <td>Parameters</td>
    <td>Purpose</td>
</tr>
<tr>
    <td>AggressiveBot</td>
    <td>SendCapsuleCaptures</td>
    <td>-</td>
    <td>Sends closest pirates to capsules to capture them.</td>
</tr>
<tr>
    <td>AggressiveBot</td>
    <td>PushAsteroidsNearby</td>
    <td>-</td>
    <td>Makes pirates push asteroids that are nearby to avoid being destroyed.</td>
</tr>
<tr>
    <td>AggressiveBot</td>
    <td>MoveToIntersection</td>
    <td>-</td>
    <td>Moves the capsule holders to an intersection and then to the best mothership.</td>
</tr>
<tr>
    <td>AggressiveBot</td>
    <td>PushAsteroids</td>
    <td>-</td>
    <td>Sends pirates to push the asteroids towards either the enemy capsule or closest enemy grouping</td>
</tr>
<tr>
    <td>AggressiveBot</td>
    <td>CheckIfCapsuleCanReach</td>
    <td>Pirate CapsuleHolder, Mothership mothership</td>
    <td>Checks if the given pirate can reach the given mothership without losing the capsule. If it can, it assigns the mothership as the destination.</td>
</tr>
<tr>
    <td>AggressiveBot</td>
    <td>GroupPair</td>
    <td>Pirate first, Pirate second, Location destination</td>
    <td>Groups the two given pirates at the best intersection and then sends them towards the given location.</td>
</tr>
<tr>
    <td>AggressiveBot</td>
    <td>AttackEnemies</td>
    <td>-</td>
    <td>Sends our pirates to attack enemy pirates.</td>
</tr>

<tr>
    <td>DefensiveBot</td>
    <td>BuildDefensiveBunker</td>
    <td>-</td>
    <td>Builds a dual setup bunker at the best mothership for each capsule and attacks the capsule if it can. If the capsule is in PushRange * 2, the pirate sails towards it to attack it. It also checks if there is a nearby wormhole that reaches towards the capsule and pushes it away from the mothership.</td>
</tr>
<tr>
    <td>DefensiveBot</td>
    <td>BuildBunkerForDefence</td>
    <td>-</td>
    <td>This bunker is used on defence mode, building a regular bunker that only pushes the nearby capsule and does not sail towards it if it's in PushRange *2</td>
</tr>
<tr>
    <td>TryPush</td>
    <td>NumOfPushesAvailable</td>
    <td>Pirate enemy</td>
    <td>Returns how many of our enemies can push the given pirate.</td>
</tr>
<tr>
    <td>TryPush</td>
    <td>PushDistanceAvailable</td>
    <td>Pirate enemy</td>
    <td>Returns how much distance the given pirate can be pushed by our pirates.</td>
</tr>
<tr>
    <td>TryPush</td>
    <td>TryPushEnemyCapsule</td>
    <td>Pirate pirate, Pirate capsuleHolder</td>
    <td>Attempts to push the enemy capsule towards the other direction from the capsule's initial location. If can't push but in PushRange * 2, sails towards the capsule to attack it.</td>
</tr>
<tr>
    <td>TryPush</td>
    <td>TryPushAsteroid</td>
    <td>Pirate pirate, Asteroid asteroid</td>
    <td>Pushes the given asteroid towards the closest enemy grouping or the closest asteroid to destroy it.</td>
</tr>
<tr>
    <td>TryPush</td>
    <td>TryPushAsteroidTowardsCapsule</td>
    <td>Pirate pirate, Asteroid asteroid</td>
    <td>Attempts to push the give asteroid towards the closest capsule's interception line.</td>
</tr>
<tr>
    <td>TryPush</td>
    <td>TryPushEnemy</td>
    <td>Pirate pirate, Pirate enemy</td>
    <td>Attempts to push the given enemy towards the closest border.</td>
</tr>
<tr>
    <td>TryPush</td>
    <td>GetGroupingNumber</td>
    <td>Pirate pirate</td>
    <td>Returns how many pirates are around the given pirate in the range of PushRange * 2</td>
<tr>
<tr>
    <td>TryPush</td>
    <td>TryPushMyCapsule</td>
    <td>Pirate myPirateWithCapsule</td>
    <td>To be added.</td>
</tr>
<tr>
    <td>TryPush</td>
    <td>PushAlliesToEnemy</td>
    <td>Pirate target</td>
    <td>To be added.</td>
</tr>
<tr>
    <td>TryPush</td>
    <td>PushWormholes</td>
    <td>-</td>
    <td>Sends pirates to push wormholes to the best location according to the priorities class.</td>
</tr>
<tr>
    <td>TryPush</td>
    <td>TryPushWormhole</td>
    <td>Pirate pirate, Wormhole wormhole</td>
    <td>Makes the given pirate attempt to push the wormhole to the best location according to the priorities class.</td>
</tr>
<tr>
    <td>TryPush</td>
    <td>PushEachOther</td>
    <td>-</td>
    <td>Makes the pairs of capsule holders push each other into the mothership if needed.</td>
</tr>
<tr>
    <td>TryPush</td>
    <td>PushPair</td>
    <td>Pirate pirate1, Pirate pirate2, Location destination</td>
    <td>Makes the given pirates push each other into the given location.</td>
</tr>
<tr>
    <td>TryPush</td>
    <td>TryPushEnemyCapsuleDefensively</td>
    <td>Pirate pirate, Pirate capsuleHolder</td>
    <td>Makes the given pirate attempt to push the given capsule holder towards the negative direction of the capsule's initial location, <b>without</b> sailing towards it if it is in PushRange * 2.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>Steps</td>
    <td>this Pirate pirate, MapObject mapObject</td>
    <td>Returns how much turns until given pirate reaches the map object.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>Sqrt</td>
    <td>this double num</td>
    <td>Returns the square root of the given double.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>Max</td>
    <td>params int[] numbers</td>
    <td>Returns the maximum number from the given params array.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>Min</td>
    <td>params int[] numbers</td>
    <td>Returns the minimum number from the given params array.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>Power</td>
    <td>this int num, int power</td>
    <td>Returns the num to the power of power.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>Power</td>
    <td>this double num, int power</td>
    <td>Returns the num to the power of power.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>Print</td>
    <td>this string s</td>
    <td>Debugs the given string if the debug is enabled.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>IntersectionPoint</td>
    <td>Location enemyLoc, Location myLoc, Location destLoc, int S1, int S2</td>
    <td>Returns the intersection point of line enemyLoc=>destLoc and myLoc according to the given speeds.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>Interception</td>
    <td>Location a, Location b, Location c</td>
    <td>Returns the interception of the line a=>b with the point c.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>IsOnTheWay</td>
    <td>Location a, Location b, Location c, int buffer</td>
    <td>Returns if the given c is on the way of the line a=>b in the range of the buffer.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>DistanceLP</td>
    <td>Location a, Location b, Location c</td>
    <td>Returns the distance between the line a=>b and point c.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>NumOfAvailableEnemyPushers</td>
    <td>Pirate pirate</td>
    <td>Returns the number of enemy pirates that can push the given pirate.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>NumberOfPushersAtLocation</td>
    <td>Location location</td>
    <td>Returns the number of enemy pirates that can push at the given location.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>MidPoint</td>
    <td>Pirate pirate1, Pirate pirate2</td>
    <td>Returns the middle point between the two pirates.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>DistanceThroughWormhole</td>
    <td>Location from, MapObject to, Wormhole wormhole, IEnumerable<Wormhole> wormholes</td>
    <td>Returns the distance through the wormhole from the current location towards the destination.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>WormholePossibleLocationDistance</td>
    <td>Location from, Location to, Location wormhole, Location partner</td>
    <td>To be added.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>ClosestDistance</td>
    <td>Location from, MapObject to, IEnumerable<Wormhole> wormholes</td>
    <td>Returns the closest distance from the from to the to.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>GetBestWormhole</td>
    <td>Location destination, Pirate pirate</td>
    <td>Returns the best wormhole for the pirate to take to get to the destination the fastest.</td>
</tr>
<tr>
    <td>Extensions</td>
    <td>GetBestMothershipThroughWormholes</td>
    <td>Pirate pirate</td>
    <td>Returns the best mothership through the wormhole distance and the value multiplier.</td>
</tr>
</table>

## Bot stats:

### Week 1 (Asteroids):
<table>
<tr>
    <td>Bot name</td>
    <td>Level</td>
    <td>Result</td>
    <td>Score</td>
    <td>Turns</td>
    <td>Explanation</td>
</tr>
<tr>
    <td>LargeRocks</td>
    <td>1</td>
    <td>Win</td>
    <td>8/3</td>
    <td>230</td>
    <td>-</td>
</tr>
<tr>
    <td>OneManArmy</td>
    <td>1</td>
    <td>Win</td>
    <td>1/0</td>
    <td>400</td>
    <td>-</td>
</tr>
<tr>
    <td>Steroids</td>
    <td>1</td>
    <td>Win</td>
    <td>8/0</td>
    <td>225</td>
    <td>-</td>
</tr>
<tr>
    <td>GeorgePushSenior</td>
    <td>2</td>
    <td>Win</td>
    <td>1/0</td>
    <td>750</td>
    <td>We have un-used pirates as we removed the AttackEnemies. We need to use them for something else.</td>
</tr>
<tr>
    <td>YouShallNotPass</td>
    <td>2</td>
    <td>Win</td>
    <td>8/7</td>
    <td>366</td>
    <td>-</td>
</tr>
<tr>
    <td>Meteorite</td>
    <td>3</td>
    <td>Win</td>
    <td>8/0</td>
    <td>155</td>
    <td>-</td>
</tr>

<tr>
    <td>Leptopn</td>
    <td>3</td>
    <td>Win</td>
    <td>8/4</td>
    <td>286</td>
    <td>-</td>
</tr>
<tr>
    <td>Voyager1</td>
    <td>4</td>
    <td>Win</td>
    <td>8/4</td>
    <td>259</td>
    <td>-</td>
</tr>
<tr>
    <td>Pullpullon</td>
    <td>4</td>
    <td>Win</td>
    <td>8/2</td>
    <td>218</td>
    <td>-</td>
</tr>
<tr>
    <td>Overall results:</td>
    <td cellspan="2">9/9</td>
    <td cellspan="3">Pass</td>
</tr>
</table>

### Week 2 (Wormholes):
<table>
<tr>
    <td>Bot name</td>
    <td>Level</td>
    <td>Result</td>
    <td>Score</td>
    <td>Turns</td>
    <td>Explanation</td>
</tr>
<tr>
    <td>SpaceTime</td>
    <td>1</td>
    <td>Win</td>
    <td>8/7</td>
    <td>269</td>
    <td>-</td>
</tr>
<tr>
    <td>Momentum</td>
    <td>1</td>
    <td>Win</td>
    <td>8/0</td>
    <td>349</td>
    <td>-</td>
</tr>
<tr>
    <td>GravityWaves</td>
    <td>2</td>
    <td>Win</td>
    <td>8/1</td>
    <td>245</td>
    <td>-</td>
</tr>
<tr>
    <td>Graviton</td>
    <td>2</td>
    <td>Win</td>
    <td>3/2</td>
    <td>38</td>
    <td>-</td>
</tr>
<tr>
    <td>Spaghettification</td>
    <td>2</td>
    <td>Win</td>
    <td>8/7</td>
    <td>247</td>
    <td>-</td>
</tr>
<tr>
    <td>OutOfSpace</td>
    <td>3</td>
    <td>Win</td>
    <td>8/1</td>
    <td>362</td>
    <td>-</td>
</tr>
<tr>
    <td>Pushti</td>
    <td>3</td>
    <td>Win</td>
    <td>8/4</td>
    <td>729</td>
    <td>-</td>
</tr>
<tr>
    <td>William</td>
    <td>4</td>
    <td>Win</td>
    <td>9/7</td>
    <td>600</td>
    <td>-</td>
</tr>
<tr>
    <td>Pullpushon</td>
    <td>4</td>
    <td><b>Loss</b></td>
    <td>8/1</td>
    <td>218</td>
    <td>-</td>
</tr>
<tr>
    <td>Overall results:</td>
    <td cellspan="2">8/9</td>
    <td cellspan="3">Pass</td>
</tr>
</table>

### Week 3 (Heavy pirates):
<table>
<tr>
    <td>Bot name</td>
    <td>Level</td>
    <td>Result</td>
    <td>Score</td>
    <td>Turns</td>
    <td>Explanation</td>
</tr>
<tr>
    <td>Deathstar</td>
    <td>1</td>
    <td>Win</td>
    <td>12/4</td>
    <td>246</td>
    <td>-</td>
</tr>
<tr>
    <td>StaticVoid</td>
    <td>1</td>
    <td>Win</td>
    <td>8/1</td>
    <td>408</td>
    <td>-</td>
</tr>
<tr>
    <td>StateMachine</td>
    <td>2</td>
    <td><b>Loss</b></td>
    <td>3/2</td>
    <td>400</td>
    <td>Bot bunkers up our capsule spawn with 40 pirates. Need to implement pushes into capsule capturers.</td>
</tr>
<tr>
    <td>SpaceRace</td>
    <td>2</td>
    <td><b>Loss</b></td>
    <td>1/0</td>
    <td>21</td>
    <td>This bot acts sort of as a race, we need to protect our capsule and make it a normal pirate.</td>
</tr>
<tr>
    <td>DarkInvasion</td>
    <td>3</td>
    <td>Win</td>
    <td>2/0</td>
    <td>400</td>
    <td>-</td>
</tr>
<tr>
    <td>HeavyLifting</td>
    <td>3</td>
    <td>Win</td>
    <td>8/4</td>
    <td>258</td>
    <td>We need to make our capsule holders become heavy pirates upon being in danger and implement the push enemy into it.</td>
</tr>
<tr>
    <td>NeutronStar</td>
    <td>3</td>
    <td>Win</td>
    <td>8/2</td>
    <td>396</td>
    <td>We need to utilize heavy pirates and make them more useable with capsule holders and bunkers (add pushes till border)</td>
</tr>
<tr>
    <td>Pushiner</td>
    <td>4</td>
    <td><b>Loss</b></td>
    <td>8/4</td>
    <td>210</td>
    <td>Make bunker pirates switch to normal if heavy then once in the bunker switch them back to heavy.</td>
</tr>
<tr>
    <td>GravitySlingshot</td>
    <td>4</td>
    <td><b>Loss</b></td>
    <td>4/1</td>
    <td>47</td>
    <td>This pirate boosts it's capsule holder towards the mothership. We need to counter that.</td>
</tr>
<tr>
    <td>Overall results:</td>
    <td cellspan="2">5/9</td>
    <td cellspan="3">Didn't pass</td>
</tr>
</table>



###### Last updated: 16/FEB/2018 21:04
