using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HTF2020.Contracts;
using HTF2020.Contracts.Enums;
using HTF2020.Contracts.Models;
using HTF2020.Contracts.Models.Adventurers;
using HTF2020.Contracts.Requests;
using System.Diagnostics;

namespace TheFellowshipOfCode.DotNet.YourAdventure
{
    public class MyAdventure : IAdventure
    {
        private readonly Random _random = new Random();

        public Task<Party> CreateParty(CreatePartyRequest request)
        {
            var party = new Party
            {
                Name = "My Party",
                Members = new List<PartyMember>()
            };

            for (var i = 0; i < request.MembersCount; i++)
            {
                party.Members.Add(new Fighter()
                {
                    Id = i,
                    Name = $"Member {i + 1}",
                    Constitution = 11,
                    Strength = 12,
                    Intelligence = 11
                });
            }

            return Task.FromResult(party);
        }

        public Task<Turn> PlayTurn(PlayTurnRequest request)
        {
            return PlayToEnd();

            Task<Turn> PlayToEnd()
            {

                Dictionary<Location, TileType> TheMap = WriteMap(request.Map);
                return GetSpecialFeatures(TheMap, request);
            }

            Task<Turn> Strategic()
            {
                const double goingEastBias = 0.35;
                const double goingSouthBias = 0.25;
                if (request.PossibleActions.Contains(TurnAction.Loot))
                {
                    return Task.FromResult(new Turn(TurnAction.Loot));
                }

                if (request.PossibleActions.Contains(TurnAction.Attack))
                {
                    return Task.FromResult(new Turn(TurnAction.Attack));
                }

                if (request.PossibleActions.Contains(TurnAction.WalkEast) && _random.NextDouble() > (1 - goingEastBias))
                {
                    return Task.FromResult(new Turn(TurnAction.WalkEast));
                }

                if (request.PossibleActions.Contains(TurnAction.WalkSouth) && _random.NextDouble() > (1 - goingSouthBias))
                {
                    return Task.FromResult(new Turn(TurnAction.WalkSouth));
                }

                return Task.FromResult(new Turn(request.PossibleActions[_random.Next(request.PossibleActions.Length)]));
            }
        }

        public Dictionary<Location, TileType> WriteMap(Map map)
        {
            Dictionary<Location, TileType> tilesWithLocation = new Dictionary<Location, TileType>();
            for (int x = 0; x < map.Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < map.Tiles.GetLength(1); y++)
                {
                    tilesWithLocation.Add(new Location(x, y), map.Tiles[x, y].TileType);
                }
            }
            return tilesWithLocation;
        }

        public Task<Turn> GetSpecialFeatures(Dictionary<Location, TileType> TheMap, PlayTurnRequest request)
        {
            Location location = null;
            foreach (KeyValuePair<Location, TileType> entry in TheMap)
            {
                if (entry.Value.Equals(TileType.TreasureChest))
                {
                    if (!request.Map.Tiles[entry.Key.X, entry.Key.Y].TreasureChest.IsEmpty)
                    {
                        location = entry.Key;
                        if (request.PossibleActions.Contains(TurnAction.Loot))
                        {
                            return Task.FromResult(new Turn(TurnAction.Loot));
                        }
                    }
                }
                else if (entry.Value.Equals(TileType.Enemy))
                {
                    if (!request.Map.Tiles[entry.Key.X, entry.Key.Y].EnemyGroup.IsDead)
                    {
                        location = entry.Key;
                        if (request.PossibleActions.Contains(TurnAction.Attack))
                        {
                            return Task.FromResult(new Turn(TurnAction.Attack));
                        }
                        if (request.PossibleActions.Contains(TurnAction.Loot))
                        {
                            return Task.FromResult(new Turn(TurnAction.Loot));
                        }
                    }
                }
                else if (entry.Value.Equals(TileType.Finish))
                {
                    location = entry.Key;
                }
            }

            return ShowPath(location, request);
        }

        public Task<Turn> ShowPath(Location featureLocation, PlayTurnRequest request)
        {
            Location partylocation = request.PartyLocation;

            if (featureLocation != null)
            {
                if (featureLocation.X > partylocation.X)
                {
                    return Task.FromResult(new Turn(TurnAction.WalkEast));
                }
                if (featureLocation.X < partylocation.X)
                {
                    return Task.FromResult(new Turn(TurnAction.WalkWest));
                }
                if (featureLocation.Y > partylocation.Y)
                {
                    return Task.FromResult(new Turn(TurnAction.WalkSouth));
                }

                if (featureLocation.Y < partylocation.Y)
                {
                    return Task.FromResult(new Turn(TurnAction.WalkNorth));
                }
            }
            
            return Task.FromResult(request.PossibleActions.Contains(TurnAction.WalkSouth)
                ? new Turn(TurnAction.WalkSouth) : new Turn(request.PossibleActions[_random.Next(request.PossibleActions.Length)]));
        }

    }
}
