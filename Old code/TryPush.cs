using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Bot
{
    partial class SSJS12Bot : IPirateBot
    {
        public int NumOfPushesAvailable(Pirate enemy)
        {
            return myPirates.Where(p => p.CanPush(enemy)).Count();
        }

        public int PushDistanceAvailable(Pirate enemy)
        {
            int total = 0;
            foreach (Pirate pirate in game.GetMyLivingPirates())
            {
                if (pirate.CanPush(enemy))
                    total += pirate.PushDistance;
            }
            return total;
        }

        public bool TryPushEnemyCapsule(Pirate pirate, Pirate capsuleHolder)
        {
            var bestMothership = game.GetEnemyMotherships().OrderBy(mothership => mothership.Distance(capsuleHolder)).FirstOrDefault();
            if (pirate.CanPush(capsuleHolder))
            {
                // Check how much other pirates can push it.
                var numOfPushers = NumOfPushesAvailable(capsuleHolder);
                // Check if we can either make the pirate lose his capsule or get pushed outside the border.
                var pushesToBorder = capsuleHolder.Distance(GetClosestToBorder(capsuleHolder.Location)) / pirate.PushDistance;
                if ((numOfPushers >= pushesToBorder && enemyCapsulesPushes[capsuleHolder.Capsule] < pushesToBorder) || (numOfPushers >= capsuleHolder.NumPushesForCapsuleLoss && enemyCapsulesPushes[capsuleHolder.Capsule] < capsuleHolder.NumPushesForCapsuleLoss))
                {
                    // Push the pirate towards the border!
                    pirate.Push(capsuleHolder, capsuleHolder.Location.Towards(capsuleHolder.Capsule.InitialLocation, -pirate.PushDistance * numOfPushers));
                    enemyCapsulesPushes[capsuleHolder.Capsule]++;
                    return true;
                }
            }
            else if (capsuleHolder.InRange(pirate, pirate.PushRange * 2) && pirate.InRange(bestMothership, (int)(game.PushDistance * 2)))
            {
                // Send the pirate towards the capsule where it can push.
                if (pirateDestinations.ContainsKey(pirate))
                    pirateDestinations[pirate] = capsuleHolder.Location.Towards(pirate, (int)(pirate.PushRange * 0.9));
                else
                    pirateDestinations.Add(pirate, capsuleHolder.Location.Towards(pirate, (int)(pirate.PushRange * 0.9)));
                return true;
            }
            return false;
        }

        public bool TryPushAsteroid(Pirate pirate, Asteroid asteroid)
        {
            if (pirate.CanPush(asteroid) && !asteroids[asteroid])
            {
                var closestEnemy = game.GetEnemyLivingPirates().OrderBy(enemy => enemy.Distance(asteroid)).OrderByDescending(enemy => GetGroupingNumber(enemy)).FirstOrDefault();
                var pushDestination = game.GetLivingAsteroids().OrderBy(ast => ast.Distance(asteroid)).Where(ast => ast != asteroid).FirstOrDefault();
                if (closestEnemy != null && pushDestination != null)
                {
                    // Check which one is closest to the pirate.
                    if (closestEnemy.Distance(pirate) < pushDestination.Distance(pirate))
                    {
                        // Push the asteroid towards the enemies.
                        pirate.Push(asteroid, closestEnemy);
                        ("Pirate " + pirate.ToString() + " pushes asteroid " + asteroid.ToString() + " towards " + closestEnemy.ToString()).Print();
                    }
                    else
                    {
                        pirate.Push(asteroid, pushDestination);
                        ("Pirate " + pirate.ToString() + " pushes asteroid " + asteroid.ToString() + " towards " + pushDestination.ToString()).Print();
                    }
                    asteroids[asteroid] = true;
                    return true;
                }
                else if (closestEnemy != null)
                {
                    // astroids.OrderBy(asteroid => asteroid.Distance(destination)).OrderBy(asteroid => asteroid.Distance(closestAsteroid)).Where(asteroid => asteroid != closestAsteroid).FirstOrDefault()
                    // Push the asteroid towards it.

                    pirate.Push(asteroid, closestEnemy);
                    ("Pirate " + pirate.ToString() + " pushes asteroid " + asteroid.ToString() + " towards " + closestEnemy.ToString()).Print();
                    asteroids[asteroid] = true;
                    return true;
                }
                else if (pushDestination != null)
                {
                    pirate.Push(asteroid, pushDestination);
                    ("Pirate " + pirate.ToString() + " pushes asteroid " + asteroid.ToString() + " towards " + pushDestination.ToString()).Print();
                    asteroids[asteroid] = true;
                    return true;
                }
            }
            return false;
        }

        public bool TryPushAsteroidTowardsCapsule(Pirate pirate, Asteroid asteroid)
        {
            if (pirate.CanPush(asteroid) && !asteroids[asteroid])
            {
                // Check if there is a capsule holder.
                var enemyCapsuleHolders = game.GetEnemyLivingPirates().Where(p => p.HasCapsule());
                if (enemyCapsuleHolders.Any())
                {
                    enemyCapsuleHolders = enemyCapsuleHolders.OrderBy(p => p.Distance(asteroid));
                    var closestHolder = enemyCapsuleHolders.FirstOrDefault();
                    var closestMothership = game.GetEnemyMotherships().OrderBy(m => m.Distance(closestHolder)).FirstOrDefault();
                    if (closestMothership != null)
                    {
                        // Intercept the capsule with the asteroid.
                        var interception = IntersectionPoint(closestHolder.Location, asteroid.Location, closestMothership.Location, closestHolder.MaxSpeed, asteroid.Speed);
                        if (interception != null)
                        {
                            // Push the asteroid.
                            pirate.Push(asteroid, interception);
                            asteroids[asteroid] = true;
                            ("Pirate " + pirate.ToString() + " pushes asteroid " + asteroid.ToString() + " to intercept " + closestHolder.ToString() + " at " + interception).Print();
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool TryPushEnemy(Pirate pirate, Pirate enemy)
        {
            if (pirate.CanPush(enemy))
            {
                pirate.Push(enemy, GetClosestToBorder(enemy.Location));
                ("Pirate " + pirate.ToString() + " pushes enemy " + enemy.ToString() + " towards " + GetClosestToBorder(enemy.Location)).Print();
                return true;
            }
            return false;
        }

        public int GetGroupingNumber(Pirate pirate)
        {
            return game.GetEnemyLivingPirates().Where(p => p.InRange(pirate, game.PushRange * 2)).Count();
        }

        public Location TryPushMyCapsule(Pirate myPirateWithCapsule)
        { // Get all my pirates with capsule
            var usedPirates = new List<Pirate>();
            PushAlliesToEnemy(myPirateWithCapsule);
            Location locationOfPush = null;

            int count = myPirates.Where(pirate => pirate.CanPush(myPirateWithCapsule)).Count(); // Number of my living pirate who can push enemy pirates
            int numberOfPushesNeeded = myPirateWithCapsule.NumPushesForCapsuleLoss;
            int numberOfEnemiesAroundMyCapsule = game.GetEnemyLivingPirates().Where(enemy => enemy.CanPush(myPirateWithCapsule)).Count();
            foreach (Pirate myPirate in myPirates.Where(pirate => pirate.CanPush(myPirateWithCapsule) && !pirate.Equals(myPirateWithCapsule))) // We push until we drop it
            {
                var destination = game.GetMyMotherships().OrderBy(mothership => mothership.Distance(myPirateWithCapsule))
                    .FirstOrDefault();
                locationOfPush = myPirateWithCapsule.Location.Towards(destination, myPirate.PushDistance);
                if (!IsWorthPushing(myPirateWithCapsule, locationOfPush, destination.Location))
                {
                    continue;
                }
                if ((IsInDanger(myPirate.GetLocation(), locationOfPush, myPirate) || ((numberOfPushesNeeded - numberOfEnemiesAroundMyCapsule == 1) || !myPirates.Contains(myPirate))))
                {
                    ("Breaking for loop, not enough pushes.").Print();
                    ((!myPirates.Contains(myPirate)).ToString()).Print();
                    break;
                }
                if ((!IsInDanger(myPirate.GetLocation(), locationOfPush, myPirate)))
                {
                    if (myPirate.HasCapsule())
                    {
                        PushPair(myPirateWithCapsule, myPirate, destination.Location);
                        continue;
                    }
                    myPirate.Push(
                        myPirateWithCapsule,
                        game.GetMyMotherships().OrderBy(mothership => mothership.Distance(myPirateWithCapsule))
                        .FirstOrDefault());
                    FinishedTurn[myPirate] = true;
                    // myPirates.Remove(myPirate);
                    usedPirates.Add(myPirate);
                    numberOfPushesNeeded--;
                }
            }
            myPirates = myPirates.Except(usedPirates).ToList();
            return locationOfPush;
        }

        public bool IsWorthPushing(Pirate pirate, Location locationOfPush, Location destination)
        {
            int count = game.GetEnemyLivingPirates()
                        .Where(enemy => enemy.HasCapsule() && pirate.Distance(destination) < enemy.Distance(
                        game.GetEnemyMotherships().OrderBy(mothership => mothership.Distance(enemy)).FirstOrDefault())).ToList().Count;
            if (count < (game.GetEnemyLivingPirates()
                        .Where(enemy => enemy.HasCapsule() && locationOfPush.Distance(destination) < enemy.Distance(
                        game.GetEnemyMotherships().OrderBy(mothership => mothership.Distance(enemy)).FirstOrDefault())).ToList().Count))
            {
                return true;
            }
            return false;
        }

        public bool PushAlliesToEnemy(Pirate target) //will document soon
        {
            int count = game.GetMyLivingPirates().Where(pirate => pirate.CanPush(target) && !pirate.HasCapsule()).Count();
            if (!(count >= target.NumPushesForCapsuleLoss))
            {
                var myPiratesNotInRange = game.GetMyLivingPirates().Where(pirate => !pirate.CanPush(target) && !pirate.HasCapsule());
                Dictionary<Pirate, int> PiratesPush = new Dictionary<Pirate, int>();
                foreach (Pirate pirate in myPiratesNotInRange)
                {
                    int PiratesCanPush = myPiratesNotInRange.Where(myPirate => pirate != myPirate && pirate.CanPush(myPirate)).Count();
                    int PushesNeeded = pirate.Distance(target) / (game.PushDistance + 1);
                    if (PiratesCanPush >= PushesNeeded)
                    {
                        PiratesPush[pirate] = pirate.Distance(target) / (game.PushDistance + 1);
                    }
                }
                var PushingPirate = PiratesPush.OrderBy(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value).FirstOrDefault();
                int PushesLeft = PushingPirate.Value;
                if (PushingPirate.Key != null)
                {
                    foreach (Pirate pirate in myPiratesNotInRange)
                    {
                        if (pirate.CanPush(PushingPirate.Key) && pirate != PushingPirate.Key && PushesLeft > 0)
                        {
                            pirate.Push(PushingPirate.Key, target);
                            myPirates.Remove(pirate);
                            PushesLeft--;
                        }
                    }
                    return true;
                }
                return false;
            }
            return true;
        }

        public void PushWormholes()
        {
            List<Wormhole> usedWormholes = new List<Wormhole>();
            foreach (Wormhole wormhole in game.GetAllWormholes())
            {
                if (usedWormholes.Contains(wormhole.Partner))
                    continue;
                var PiratePush = PushWormhole(wormhole, myPirates, true);
                usedWormholes.Add(wormhole);
            }
        }

        public bool TryPushWormhole(Pirate pirate, Wormhole wormhole)
        {
            List<Pirate> AvailablePirates = new List<Pirate>();
            AvailablePirates.Add(pirate);
            if (pirate.CanPush(wormhole))
                PushWormhole(wormhole, AvailablePirates, false);
            return AvailablePirates.Count == 0;
        }

        public void PushEachOther()
        {
            var PiratesWithCapsuleCanPushOthers = new Dictionary<Pirate, List<Pirate>>();
            var UsedPirates = new List<Pirate>();
            var myPiratesWithCapsule = game.GetMyLivingPirates().Where(pirate => pirate.HasCapsule());
            foreach (Pirate pirateWithCapsule in myPiratesWithCapsule) //go over all my pirate with capsule and make a list of who they can push
            {
                var heading = game.GetMyMotherships().OrderBy(mothership => mothership.Distance(pirateWithCapsule)).FirstOrDefault();
                var PiratesWhoCanPush = myPiratesWithCapsule
                    .Where(pirate => pirate.CanPush(pirateWithCapsule) //All pirate who can push the current pirate with capsule
                        &&
                        pirateWithCapsule != pirate &&
                        pirate.Distance(heading) - ((Mothership)heading).UnloadRange < pirate.PushDistance).ToList();
                if (PiratesWhoCanPush.Count == 0)
                {
                    continue;
                }
                PiratesWithCapsuleCanPushOthers[pirateWithCapsule] = PiratesWhoCanPush;
            }
            foreach (var pirate in PiratesWithCapsuleCanPushOthers
                    .OrderBy(item => item.Value.Count)
                    .ToDictionary(pair => pair.Key, pair => pair.Value).Keys.ToList()) //go over all my Pirates with capsule in the dictionary but in order of the length of the list
            {
                var heading = game.GetMyMotherships().OrderBy(mothership => mothership.Distance(pirate)).FirstOrDefault();
                if (myPirates.Contains(pirate))
                {
                    Pirate OtherPushingPirate = PiratesWithCapsuleCanPushOthers[pirate].First(); //Get FirstPirate who can push the current pirate with capsule
                    PushPair(pirate, OtherPushingPirate, heading.Location); //Push them
                    myPirates.Remove(pirate); //Remove both from list
                    myPirates.Remove(OtherPushingPirate);
                    FinishedTurn[pirate] = true; //Set turns to finished
                    FinishedTurn[OtherPushingPirate] = true;
                }
            }
        }

        public void PushPair(Pirate pirate1, Pirate pirate2, Location destination) //Take two pirates and a destination and lets them push eachother towards the destination
        {
            pirate1.Push(pirate2, destination);
            pirate2.Push(pirate1, destination);
        }

        public bool TryPushEnemyCapsuleDefensively(Pirate pirate, Pirate capsuleHolder)
        {
            var bestMothership = game.GetEnemyMotherships().OrderBy(mothership => mothership.Distance(capsuleHolder)).FirstOrDefault(); ;
            if (pirate.CanPush(capsuleHolder))
            {
                // Check how much other pirates can push it.
                var numOfPushers = NumOfPushesAvailable(capsuleHolder);
                // Check if we can either make the pirate lose his capsule or get pushed outside the border.
                var pushesToBorder = capsuleHolder.Distance(GetClosestToBorder(capsuleHolder.Location)) / pirate.PushDistance;
                if ((numOfPushers >= pushesToBorder && enemyCapsulesPushes[capsuleHolder.Capsule] < pushesToBorder) || (numOfPushers >= capsuleHolder.NumPushesForCapsuleLoss && enemyCapsulesPushes[capsuleHolder.Capsule] < capsuleHolder.NumPushesForCapsuleLoss))
                {
                    // Push the pirate towards the border!
                    pirate.Push(capsuleHolder, capsuleHolder.Location.Towards(capsuleHolder.Capsule.InitialLocation, -numOfPushers * pirate.PushDistance));
                    enemyCapsulesPushes[capsuleHolder.Capsule]++;
                    return true;
                }
            }
            return false;
        }

        public bool TryPushInterceptedEnemyCapsule(Pirate pirate, Pirate capsuleHolder)
        {
            var bestMothership = game.GetEnemyMotherships().OrderBy(mothership => mothership.Distance(capsuleHolder)).FirstOrDefault();
            if (pirate.CanPush(capsuleHolder))
            {
                // Check how much other pirates can push it.
                var numOfPushers = NumOfPushesAvailable(capsuleHolder);
                // Check if we can either make the pirate lose his capsule or get pushed outside the border.
                var pushesToBorder = capsuleHolder.Distance(GetClosestToBorder(capsuleHolder.Location)) / pirate.PushDistance;
                if ((numOfPushers >= pushesToBorder && enemyCapsulesPushes[capsuleHolder.Capsule] < pushesToBorder) || (numOfPushers >= capsuleHolder.NumPushesForCapsuleLoss && enemyCapsulesPushes[capsuleHolder.Capsule] < capsuleHolder.NumPushesForCapsuleLoss))
                {
                    // Push the pirate towards the border!
                    pirate.Push(capsuleHolder, GetClosestToBorder(capsuleHolder.Location));
                    enemyCapsulesPushes[capsuleHolder.Capsule]++;
                    return true;
                }
            }
            else if (capsuleHolder.InRange(pirate, pirate.PushRange * 2) && pirate.InRange(bestMothership, (int)(game.PushDistance * 2)))
            {
                // Send the pirate towards the capsule where it can push.
                if (pirateDestinations.ContainsKey(pirate))
                    pirateDestinations[pirate] = capsuleHolder.Location.Towards(pirate, (int)(pirate.PushRange * 0.9));
                else
                    pirateDestinations.Add(pirate, capsuleHolder.Location.Towards(pirate, (int)(pirate.PushRange * 0.9)));
                return true;
            }
            return false;
        }
    }
}