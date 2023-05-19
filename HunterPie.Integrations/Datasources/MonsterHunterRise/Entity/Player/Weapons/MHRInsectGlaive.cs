﻿using HunterPie.Core.Address.Map;
using HunterPie.Core.Architecture.Events;
using HunterPie.Core.Domain;
using HunterPie.Core.Domain.Process;
using HunterPie.Core.Extensions;
using HunterPie.Core.Game.Entity.Player.Classes;
using HunterPie.Core.Game.Enums;
using HunterPie.Core.Game.Events;
using HunterPie.Integrations.Datasources.MonsterHunterRise.Definitions;
using HunterPie.Integrations.Datasources.MonsterHunterRise.Entity.Enums;
using HunterPie.Integrations.Datasources.MonsterHunterRise.Utils;

namespace HunterPie.Integrations.Datasources.MonsterHunterRise.Entity.Player.Weapons;
public class MHRInsectGlaive : MHRMeleeWeapon, IInsectGlaive
{
    private KinsectBuff _primaryExtract;
    private KinsectBuff _secondaryExtract;
    private float _attackTimer;
    private float _speedTimer;
    private float _defenseTimer;
    private float _stamina;
    private KinsectChargeType _chargeType;
    private float _charge;

    public KinsectBuff PrimaryExtract
    {
        get => _primaryExtract;
        private set
        {
            if (value == _primaryExtract)
                return;

            _primaryExtract = value;
            this.Dispatch(_onPrimaryExtractChange, new InsectGlaiveExtractChangeEventArgs(value));
        }
    }

    public KinsectBuff SecondaryExtract
    {
        get => _secondaryExtract;
        private set
        {
            if (value == _secondaryExtract)
                return;

            _secondaryExtract = value;
            this.Dispatch(_onSecondaryExtractChange, new InsectGlaiveExtractChangeEventArgs(value));
        }
    }

    public KinsectChargeType ChargeType
    {
        get => _chargeType;
        private set
        {
            if (value == _chargeType)
                return;

            _chargeType = value;
            this.Dispatch(_onChargeChange, new KinsectChargeChangeEventArgs(this));
        }
    }

    public float AttackTimer
    {
        get => _attackTimer;
        private set
        {
            if (value == _attackTimer)
                return;

            _attackTimer = value;
            this.Dispatch(_onAttackTimerChange, new InsectGlaiveBuffTimerChangeEventArgs(value));
        }
    }

    public float SpeedTimer
    {
        get => _speedTimer;
        private set
        {
            if (value == _speedTimer)
                return;

            _speedTimer = value;
            this.Dispatch(_onSpeedTimerChange, new InsectGlaiveBuffTimerChangeEventArgs(value));
        }
    }

    public float DefenseTimer
    {
        get => _defenseTimer;
        private set
        {
            if (value == _defenseTimer)
                return;

            _defenseTimer = value;
            this.Dispatch(_onDefenseTimerChange, new InsectGlaiveBuffTimerChangeEventArgs(value));
        }
    }

    public float Stamina
    {
        get => _stamina;
        private set
        {
            if (value == _stamina)
                return;

            _stamina = value;
            this.Dispatch(_onKinsectStaminaChange, new KinsectStaminaChangeEventArgs(this));
        }
    }

    public float MaxStamina { get; private set; }

    public float Charge
    {
        get => _charge;
        private set
        {
            if (value == _charge)
                return;

            _charge = value;
            this.Dispatch(_onChargeChange, new KinsectChargeChangeEventArgs(this));
        }
    }

    private readonly SmartEvent<InsectGlaiveExtractChangeEventArgs> _onPrimaryExtractChange = new();
    public event EventHandler<InsectGlaiveExtractChangeEventArgs> OnPrimaryExtractChange
    {
        add => _onPrimaryExtractChange.Hook(value);
        remove => _onPrimaryExtractChange.Unhook(value);
    }

    private readonly SmartEvent<InsectGlaiveExtractChangeEventArgs> _onSecondaryExtractChange = new();
    public event EventHandler<InsectGlaiveExtractChangeEventArgs> OnSecondaryExtractChange
    {
        add => _onSecondaryExtractChange.Hook(value);
        remove => _onSecondaryExtractChange.Unhook(value);
    }

    private readonly SmartEvent<InsectGlaiveBuffTimerChangeEventArgs> _onAttackTimerChange = new();
    public event EventHandler<InsectGlaiveBuffTimerChangeEventArgs> OnAttackTimerChange
    {
        add => _onAttackTimerChange.Hook(value);
        remove => _onAttackTimerChange.Unhook(value);
    }

    private readonly SmartEvent<InsectGlaiveBuffTimerChangeEventArgs> _onSpeedTimerChange = new();
    public event EventHandler<InsectGlaiveBuffTimerChangeEventArgs> OnSpeedTimerChange
    {
        add => _onSpeedTimerChange.Hook(value);
        remove => _onSpeedTimerChange.Unhook(value);
    }

    private readonly SmartEvent<InsectGlaiveBuffTimerChangeEventArgs> _onDefenseTimerChange = new();
    public event EventHandler<InsectGlaiveBuffTimerChangeEventArgs> OnDefenseTimerChange
    {
        add => _onDefenseTimerChange.Hook(value);
        remove => _onDefenseTimerChange.Unhook(value);
    }

    private readonly SmartEvent<KinsectStaminaChangeEventArgs> _onKinsectStaminaChange = new();
    public event EventHandler<KinsectStaminaChangeEventArgs> OnKinsectStaminaChange
    {
        add => _onKinsectStaminaChange.Hook(value);
        remove => _onKinsectStaminaChange.Unhook(value);
    }

    private readonly SmartEvent<KinsectChargeChangeEventArgs> _onChargeChange = new();
    public event EventHandler<KinsectChargeChangeEventArgs> OnChargeChange
    {
        add => _onChargeChange.Hook(value);
        remove => _onChargeChange.Unhook(value);
    }

    public MHRInsectGlaive(IProcessManager process) : base(process, Weapon.InsectGlaive) { }

    [ScannableMethod]
    private void GetKinsectData()
    {
        MHRInsectGlaiveDataStructure structure = Memory.Deref<MHRInsectGlaiveDataStructure>(
            AddressMap.GetAbsolute("LOCAL_PLAYER_DATA_ADDRESS"),
            AddressMap.Get<int[]>("CURRENT_WEAPON_OFFSETS")
        );
        KinsectBuff[] extracts = Memory.ReadArraySafe<int>(structure.ExtractsArray, 2)
            .Select(it => (KinsectExtract)it)
            .Select(it => it.ToBuff())
            .ToArray();

        if (extracts.Length < 2)
            return;

        (KinsectBuff primary, KinsectBuff secondary) = (extracts.First(), extracts.Last());

        PrimaryExtract = primary;
        SecondaryExtract = secondary != KinsectBuff.None ? secondary : primary;
        AttackTimer = structure.AttackTimer.ToAbnormalitySeconds();
        SpeedTimer = structure.SpeedTimer.ToAbnormalitySeconds();
        DefenseTimer = structure.DefenseTimer.ToAbnormalitySeconds();
    }

    [ScannableMethod]
    private void GetKinsectStamina()
    {
        MHRKinsectStaminaStructure structure = Memory.Deref<MHRKinsectStaminaStructure>(
            AddressMap.GetAbsolute("LOCAL_PLAYER_DATA_ADDRESS"),
            AddressMap.Get<int[]>("KINSECT_STAMINA_OFFSETS")
        );

        MaxStamina = structure.Max;
        Stamina = structure.Current;
    }


    public override void Dispose()
    {
        new IDisposable[]
        {
            _onPrimaryExtractChange,
            _onSecondaryExtractChange,
            _onAttackTimerChange,
            _onSpeedTimerChange,
            _onDefenseTimerChange,
            _onKinsectStaminaChange,
            _onChargeChange
        }.DisposeAll();
    }
}
