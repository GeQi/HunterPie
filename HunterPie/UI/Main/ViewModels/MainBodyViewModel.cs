﻿using HunterPie.Core.Architecture;
using HunterPie.Core.Client;
using HunterPie.Core.Client.Configuration.Enums;
using HunterPie.Core.Domain.Interfaces;
using HunterPie.Internal;
using HunterPie.UI.Architecture;
using HunterPie.UI.SideBar.ViewModels;
using System.Collections.ObjectModel;

namespace HunterPie.UI.Main.ViewModels;

internal class MainBodyViewModel : ViewModel
{
    private const string SUPPORTER_PROMPT_KEY = "supporter_prompt_closed";
    private readonly ILocalRegistryAsync _localRegistryAsync;

    public SideBarViewModel SideBarViewModel { get; init; }

    private ViewModel? _navigationViewModel;
    public ViewModel? NavigationViewModel { get => _navigationViewModel; set => SetValue(ref _navigationViewModel, value); }

    public Observable<GameType> SelectedGame => ClientConfig.Config.Client.DefaultGameType;

    public ObservableCollection<GameType> Games { get; } = new() { GameType.Rise, GameType.World };

    private bool _shouldDisplaySupporterPrompt;
    public bool ShouldDisplaySupporterPrompt { get => _shouldDisplaySupporterPrompt; set => SetValue(ref _shouldDisplaySupporterPrompt, value); }

    public MainBodyViewModel(
        SideBarViewModel sideBarViewModel,
        ILocalRegistryAsync localRegistryAsync)
    {
        SideBarViewModel = sideBarViewModel;
        _localRegistryAsync = localRegistryAsync;
    }

    public void LaunchGame()
    {
        Steam.RunGameBy(SelectedGame.Value);
    }

    public void InitializeSupporterPrompt(bool isSupporter)
    {
        bool hasClosedPrompt = _localRegistryAsync.Exists(SUPPORTER_PROMPT_KEY);

        ShouldDisplaySupporterPrompt = !hasClosedPrompt && !isSupporter;
    }

    public void CloseSupporterPrompt()
    {
        ShouldDisplaySupporterPrompt = false;
        _localRegistryAsync.Set(SUPPORTER_PROMPT_KEY, true);
    }
}