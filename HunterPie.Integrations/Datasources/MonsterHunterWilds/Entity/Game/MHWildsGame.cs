﻿using HunterPie.Core.Address.Map;
using HunterPie.Core.Domain;
using HunterPie.Core.Domain.Process.Entity;
using HunterPie.Core.Extensions;
using HunterPie.Core.Game.Entity.Enemy;
using HunterPie.Core.Game.Entity.Game.Chat;
using HunterPie.Core.Game.Entity.Game.Quest;
using HunterPie.Core.Game.Entity.Player;
using HunterPie.Core.Game.Services;
using HunterPie.Core.Scan.Service;
using HunterPie.Core.Utils;
using HunterPie.Integrations.Datasources.Common.Entity.Game;
using HunterPie.Integrations.Datasources.MonsterHunterWilds.Definitions.Monster;
using HunterPie.Integrations.Datasources.MonsterHunterWilds.Entity.Crypto;
using HunterPie.Integrations.Datasources.MonsterHunterWilds.Entity.Enemy;
using HunterPie.Integrations.Datasources.MonsterHunterWilds.Utils;

namespace HunterPie.Integrations.Datasources.MonsterHunterWilds.Entity.Game;

public sealed class MHWildsGame : CommonGame
{
    private readonly MHWildsCryptoService _cryptoService;
    private readonly Dictionary<nint, MHWildsMonster> _monsters = new(3);

    public override IPlayer Player => throw new NotImplementedException();

    public override IAbnormalityCategorizationService AbnormalityCategorizationService => throw new NotImplementedException();

    public override IReadOnlyCollection<IMonster> Monsters => _monsters.Values;

    public override IChat? Chat => null;

    public override bool IsHudOpen { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
    public override float TimeElapsed { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

    public override IQuest? Quest => null;

    public MHWildsGame(
        IGameProcess process,
        IScanService scanService) : base(process, scanService)
    {
        _cryptoService = new MHWildsCryptoService(process.Memory);
    }

    [ScannableMethod]
    internal async Task GetMonstersAsync()
    {
        nint monsterArrayPointer = await Memory.ReadAsync(
            address: AddressMap.GetAbsolute("Game::EnemyManager"),
            offsets: AddressMap.GetOffsets("Environment::MonsterList")
        );
        nint[] monsters = await Memory.ReadDynamicArraySafeAsync(
            address: monsterArrayPointer,
            count: 700
        );
        nint[] validMonsters = monsters.Where(it => !it.IsNullPointer())
            .ToArray();

        IEnumerable<nint> monstersToCreate = validMonsters.Where(it => !_monsters.ContainsKey(it));

        (await monstersToCreate.Select(async it =>
                (
                    address: it,
                    data: await Memory.DerefPtrAsync<MHWildsMonsterBasicData>(
                        address: it,
                        offsets: AddressMap.GetOffsets("Monster::BasicData")
                    )
                )
            ).AwaitAll())
            .Where(it => it.data.Category == 0)
            .ForEach(it => HandleMonsterSpawn(it.address, it.data));


        IEnumerable<nint> monstersToDestroy = _monsters.Keys.Where(it => !validMonsters.Contains(it));

        monstersToDestroy.ForEach(HandleMonsterDespawn);
    }

    private void HandleMonsterSpawn(nint address, MHWildsMonsterBasicData data)
    {
        var monster = new MHWildsMonster(
            process: Process,
            scanService: ScanService,
            address: address,
            basicData: data,
            cryptoService: _cryptoService
        );

        _monsters[address] = monster;

        this.Dispatch(
            toDispatch: _onMonsterSpawn,
            data: monster
        );
    }

    private void HandleMonsterDespawn(nint address)
    {
        if (_monsters[address] is not { } monster)
            return;

        _monsters.Remove(address);

        this.Dispatch(
            toDispatch: _onMonsterDespawn,
            data: monster
        );

        monster.Dispose();
    }

    public override void Dispose()
    {
        base.Dispose();
        _cryptoService.Dispose();
    }
}