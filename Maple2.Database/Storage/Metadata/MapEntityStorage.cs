﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Maple2.Database.Context;
using Maple2.Model.Common;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public class MapEntityStorage : MetadataStorage<string, MapEntityMetadata> {
    private const int CACHE_SIZE = 1500; // ~1.1k total Maps

    public MapEntityStorage(MetadataContext context) : base(context, CACHE_SIZE) { }

    public MapEntityMetadata? Get(string xblock) {
        if (Cache.TryGet(xblock, out MapEntityMetadata mapEntity)) {
            return mapEntity;
        }

        var breakables = new Dictionary<Guid, Breakable>();
        var liftables = new Dictionary<Guid, Liftable>();
        var objectWeapons = new Dictionary<Vector3B, ObjectWeapon>();
        var portals = new Dictionary<int, Portal>();
        var playerSpawns = new Dictionary<int, SpawnPointPC>();
        var npcSpawns = new List<SpawnPointNPC>();
        var regionSpawns = new Dictionary<int, Ms2RegionSpawn>();
        var regionSkills = new List<Ms2RegionSkill>();
        var eventNpcSpawns = new Dictionary<int, EventSpawnPointNPC>();
        TaxiStation? taxi = null;
        Telescope? telescope = null;
        var breakableActors = new Dictionary<Guid, BreakableActor>();
        var interactActors = new Dictionary<int, InteractActor>();
        var triggerModels = new Dictionary<int, TriggerModel>();
        var triggers = new List<Trigger>();
        lock (Context) {
            foreach (MapEntity entity in Context.MapEntity.Where(entity => entity.XBlock == xblock)) {
                switch (entity.Block.Class) {
                    case MapBlock.Discriminator.Breakable:
                        if (entity.Block is Breakable breakable) {
                            breakables[entity.Guid] = breakable;
                        }
                        break;
                    case MapBlock.Discriminator.BreakableActor:
                        if (entity.Block is BreakableActor breakableActor) {
                            breakableActors[entity.Guid] = breakableActor;
                        }
                        break;
                    case MapBlock.Discriminator.InteractActor:
                        if (entity.Block is InteractActor interactActor) {
                            interactActors[interactActor.InteractId] = interactActor;
                        }
                        break;
                    case MapBlock.Discriminator.Liftable:
                        if (entity.Block is Liftable liftable) {
                            liftables[entity.Guid] = liftable;
                        }
                        break;
                    case MapBlock.Discriminator.ObjectWeapon:
                        if (entity.Block is ObjectWeapon objectWeapon) {
                            objectWeapons[objectWeapon.Position] = objectWeapon;
                        }
                        break;
                    case MapBlock.Discriminator.Portal:
                        if (entity.Block is Portal portal) {
                            portals[portal.Id] = portal;
                        }
                        break;
                    case MapBlock.Discriminator.Ms2RegionSpawn:
                        if (entity.Block is Ms2RegionSpawn regionSpawn) {
                            regionSpawns[regionSpawn.Id] = regionSpawn;
                        }
                        break;
                    case MapBlock.Discriminator.Ms2RegionSkill:
                        if (entity.Block is Ms2RegionSkill regionSkill) {
                            regionSkills.Add(regionSkill);
                        }
                        break;
                    case MapBlock.Discriminator.SpawnPointPC:
                        if (entity.Block is SpawnPointPC playerSpawn) {
                            playerSpawns[playerSpawn.Id] = playerSpawn;
                        }
                        break;
                    case MapBlock.Discriminator.SpawnPointNPC:
                        if (entity.Block is SpawnPointNPC npcSpawn) {
                            npcSpawns.Add(npcSpawn);
                        }
                        break;
                    case MapBlock.Discriminator.EventSpawnPointNPC:
                        if (entity.Block is EventSpawnPointNPC eventNpcSpawn) {
                            eventNpcSpawns.Add(eventNpcSpawn.Id, eventNpcSpawn);
                        }
                        break;
                    case MapBlock.Discriminator.TaxiStation:
                        if (entity.Block is TaxiStation taxiStation) {
                            Debug.Assert(taxi == null, $"Multiple taxi stations found in xblock:{xblock}");
                            taxi = taxiStation;
                        }
                        break;
                    case MapBlock.Discriminator.Telescope:
                        if (entity.Block is Telescope mapTelescope) {
                            Debug.Assert(telescope == null, $"Multiple telescopes found in xblock:{xblock}");
                            telescope = mapTelescope;
                        }
                        break;
                    case MapBlock.Discriminator.TriggerModel:
                        if (entity.Block is TriggerModel triggerModel) {
                            triggerModels.Add(triggerModel.Id, triggerModel);
                        }
                        break;
                    case MapBlock.Discriminator.Ms2TriggerActor:
                    case MapBlock.Discriminator.Ms2TriggerBox:
                    case MapBlock.Discriminator.Ms2TriggerCamera:
                    case MapBlock.Discriminator.Ms2TriggerCube:
                    case MapBlock.Discriminator.Ms2TriggerEffect:
                    case MapBlock.Discriminator.Ms2TriggerLadder:
                    case MapBlock.Discriminator.Ms2TriggerMesh:
                    case MapBlock.Discriminator.Ms2TriggerRope:
                    case MapBlock.Discriminator.Ms2TriggerSkill:
                    case MapBlock.Discriminator.Ms2TriggerSound:
                        triggers.Add((Trigger) entity.Block);
                        break;
                }
            }
        }

        mapEntity = new MapEntityMetadata {
            Breakables = breakables,
            Liftables = liftables,
            ObjectWeapons = objectWeapons,
            Portals = portals,
            PlayerSpawns = playerSpawns,
            NpcSpawns = npcSpawns,
            EventNpcSpawns = eventNpcSpawns,
            RegionSpawns = regionSpawns,
            RegionSkills = regionSkills,
            Taxi = taxi,
            Telescope = telescope,
            BreakableActors = breakableActors,
            InteractActors = interactActors,
            TriggerModels = triggerModels,
            Trigger = new TriggerStorage(triggers),
        };
        Cache.AddReplace(xblock, mapEntity);

        return mapEntity;
    }

    public bool Contains(string xblock) {
        return Get(xblock) != null;
    }
}
