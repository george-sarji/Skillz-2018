using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Bot
{
    partial class SSJS12Bot : IPirateBot
    {
        public void GenerateGeneralPriority()
        {
            foreach (MapObject mapobject in GeneralPriority.Keys)
            {
                GeneratePriority(mapobject);
            }
        }

        public bool HandleSwitchPirates()
        {
            var wantToBeHeavy = new List<Pirate>();
            var wantToBeNormal = new List<Pirate>();
            var willingToBeHeavy = new List<Pirate>();
            var willingToBeNormal = new List<Pirate>();


            foreach (Pirate pirate in game.GetMyLivingPirates())
            {
                if (pirate.HasCapsule())
                {
                    if (CapsuleHolderInDanger(pirate) && pirate.StateName == "normal") {
                        wantToBeHeavy.Add(pirate);
                        game.Debug("capsule holder wants to be heavy");
                    }
                    if (!CapsuleHolderInDanger(pirate) && pirate.StateName == "heavy"){
                        wantToBeNormal.Add(pirate);
                        game.Debug("capsule holder wants to be normal");
                    }
                }
                else
                {
                    if (pirate.StateName == "normal")
                    {
                        // if(bunkeringPirates.Contains(pirate))
                        //     wantToBeHeavy.Add(pirate);
                        // else willingToBeHeavy.Add(pirate);
                        willingToBeHeavy.Add(pirate);
                    }
                    if (pirate.StateName == "heavy")
                    willingToBeNormal.Add(pirate);
                }
            }

            return TrySwitchPirates(wantToBeHeavy, wantToBeNormal) ||
                TrySwitchPirates(wantToBeHeavy, willingToBeNormal) ||
                TrySwitchPirates(wantToBeNormal, willingToBeHeavy);
        }

        public bool TrySwitchPirates(List<Pirate> group1, List<Pirate> group2)
        {
            var pirate1 = group1.FirstOrDefault();
            var pirate2 = group2.FirstOrDefault();

            if (pirate1 != null && pirate2 != null && pirate1 != pirate2)
            {
                pirate1.SwapStates(pirate2);
                myPirates.Remove(pirate1);
                return true;
            }
            return false;
        }

        public static int ScaleNumber(double num, int turns, double scale)
        {
            if (turns == 0)
                return (int) (num * 100 / scale);
            return (int) (num * 100 * turns / scale);
        }

        public void GeneratePriority(MapObject mapObject)
        {
            int Priority = 0;
            Priority += NumberOfEnemies(mapObject);
            GeneralPriority[mapObject] = Priority;
        }

        public int NumberOfEnemies(MapObject mapObject) // Returns number of enemies in range of a mapobject fix it
        {
            if (mapObject is Mothership)
            {
                return game.GetEnemyLivingPirates().Count(pirate => pirate.InRange(mapObject, (((Mothership) mapObject).UnloadRange))); // Returns the number of enemies on a mothership
            }
            else if (mapObject is Pirate)
            {
                int number = game.GetEnemyLivingPirates().Count(pirate => pirate.InRange(mapObject, (((Pirate) mapObject).PushRange))); // Returns the number of enemies in range of a pirate
                if (((Pirate) mapObject).Owner == game.GetMyself())
                    number--;
                return number;
            }
            else if (mapObject is Capsule)
            {
                return game.GetEnemyLivingPirates().Count(pirate => pirate.InRange(mapObject, (((Capsule) mapObject).PickupRange))); // Returns the number of enemies in the pickup range of a capsule
            }
            return 0;
        }

        public int GetWormholeLocationScore(Wormhole wormhole, Location wormholeLocation, Location partner)
        {
            int score = 0;
            var best = bestMothershipAndCapsulePair(wormhole);
            int distance = WormholePossibleLocationDistance(
                best.First().GetLocation(),
                best.Last().GetLocation(),
                wormhole.Location,
                NewWormholeLocation[wormhole.Partner]);
            score += ScaleNumber(distance, wormhole.TurnsToReactivate, scale);
            return score;
        }

        public Wormhole GetBestWormhole(Pirate pirate)
        {
            Dictionary<Wormhole, int> wormholesScore = new Dictionary<Wormhole, int>();
            foreach (var wormhole in game.GetAllWormholes())
            {
                Location partnerLocation = wormhole.Partner.Location;
                if (NewWormholeLocation[wormhole.Partner] != partnerLocation)
                {
                    partnerLocation = wormhole.Partner.Location.Towards(partnerLocation, pirate.PushDistance);
                }
                wormholesScore.Add(wormhole, GetWormholeLocationScore(wormhole, wormhole.Location, partnerLocation)); //Add all wormholes with their Scores according to the pirate
            }
            PrintWormhole(wormholesScore, pirate);
            return wormholesScore.OrderBy(x => x.Value)
                .FirstOrDefault().Key; //Order the wormholes by the score the lowest score is the best wormhole
        }

        public List<MapObject> bestMothershipAndCapsulePair(Wormhole wormhole)
        {
            List<MapObject> best = new List<MapObject>();
            Location partnerLocation = wormhole.Partner.Location;
            Mothership bestMothership = game.GetMyMotherships() //Closest Mothership to wormhole
                .OrderBy(mothership => mothership.Distance(wormhole.Location))
                .FirstOrDefault();
            Capsule bestCapsule = game.GetMyCapsules() //Closest Capsule to partner
                .OrderBy(capsule => capsule.Distance(wormhole.Partner))
                .FirstOrDefault();
            if (NewWormholeLocation[wormhole.Partner] != partnerLocation)
            {
                partnerLocation = wormhole.Partner.Location.Towards(partnerLocation, game.PushDistance);
            }
            int distance = WormholePossibleLocationDistance(bestMothership.Location, bestCapsule.Location, wormhole.Location, partnerLocation);
            bestMothership = game.GetMyMotherships() //Closest Mothership to partner
                .OrderBy(mothership => mothership.Distance(wormhole.Partner))
                .FirstOrDefault();
            bestCapsule = game.GetMyCapsules() //Closest Capsule to wormholelocation
                .OrderBy(capsule => capsule.Distance(wormhole.Location))
                .FirstOrDefault();
            int distance2 = WormholePossibleLocationDistance(bestMothership.Location, bestCapsule.Location, wormhole.Location, partnerLocation);
            if (distance < distance2)
            {
                bestMothership = game.GetMyMotherships() //Closest Mothership to wormhole
                    .OrderBy(mothership => mothership.Distance(wormhole.Location))
                    .FirstOrDefault();
                bestCapsule = game.GetMyCapsules() //Closest Capsule to partner
                    .OrderBy(capsule => capsule.Distance(wormhole.Partner))
                    .FirstOrDefault();
                best.Add(bestMothership);
                best.Add(bestCapsule);
                return best;
            }
            best.Add(bestMothership);
            best.Add(bestCapsule);
            return best;

        }

        public Dictionary<Pirate, MapObject> PushWormhole(Wormhole wormhole, List<Pirate> availablePirates, bool Assign)
        {
            // Checks if the wormhole can be pushed to a better location, and if is it returns the new location.
            // List<Location> candidates = new List<Location>();
            // candidates.Add(wormhole.GetLocation());
            // const int steps = 24;
            // for (int i = 0; i < steps; i++)
            // {
            //     double angle = System.Math.PI * 2 * i / steps;
            //     double deltaX = pirate.PushDistance * System.Math.Sin(angle);
            //     double deltaY = pirate.PushDistance * System.Math.Cos(angle);
            //     Location option = wormhole.Location.Add(new Location(-(int)deltaX, (int)deltaY));
            //     if (!option.InMap())
            //     {
            //         continue;
            //     }
            //     candidates.Add(option);

            // }
            Dictionary<Pirate, MapObject> PiratePush = new Dictionary<Pirate, MapObject>();
            List<MapObject> best = bestMothershipAndCapsulePair(wormhole);
            foreach (MapObject mapObject in best)
            {
                Pirate closestPirate = availablePirates.OrderBy(pirate => pirate.Distance(wormhole)).FirstOrDefault();
                if (closestPirate == null)
                    break;
                PiratePush.Add(closestPirate, mapObject);
                availablePirates.Remove(closestPirate);
                myPirates.Remove(closestPirate);
                if (closestPirate.CanPush(wormhole))
                {
                    closestPirate.Push(wormhole, mapObject);
                    NewWormholeLocation[wormhole] = mapObject.GetLocation();
                    FinishedTurn[closestPirate] = true;
                }
                else if (Assign)
                {
                    AssignDestination(closestPirate, wormhole.GetLocation().Towards(closestPirate, wormhole.WormholeRange));
                }
                wormhole = wormhole.Partner;
            }
            return PiratePush;
        }
    }
}