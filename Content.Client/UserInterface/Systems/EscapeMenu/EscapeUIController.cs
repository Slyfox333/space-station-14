﻿using Content.Client.Gameplay;
using Content.Client.Info;
using Content.Client.Links;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Info;
using JetBrains.Annotations;
using Robust.Client.Console;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.UserInterface.Systems.EscapeMenu;

[UsedImplicitly]
public sealed class EscapeUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    [Dependency] private readonly IClientConsoleHost _console = default!;
    [Dependency] private readonly IUriOpener _uri = default!;
    [Dependency] private readonly ChangelogUIController _changelog = default!;
    [Dependency] private readonly InfoUIController _info = default!;
    [Dependency] private readonly OptionsUIController _options = default!;

    private Options.UI.EscapeMenu? _escapeWindow;

    private MenuButton? _escapeButton;

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(_escapeWindow == null);
        _escapeButton = UIManager.GetActiveUIWidget<MenuBar.Widgets.GameTopMenuBar>().EscapeButton;
        _escapeButton.OnPressed += EscapeButtonOnOnPressed;

        _escapeWindow = UIManager.CreateWindow<Options.UI.EscapeMenu>();
        _escapeWindow.OnClose += () => { _escapeButton.Pressed = false; };
        _escapeWindow.OnOpen +=  () => { _escapeButton.Pressed = true; };

        _escapeWindow.ChangelogButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _changelog.ToggleWindow();
        };

        _escapeWindow.RulesButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _info.OpenWindow();
        };

        _escapeWindow.DisconnectButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _console.ExecuteCommand("disconnect");
        };

        _escapeWindow.OptionsButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _options.OpenWindow();
        };

        _escapeWindow.QuitButton.OnPressed += _ =>
        {
            CloseEscapeWindow();
            _console.ExecuteCommand("quit");
        };

        _escapeWindow.WikiButton.OnPressed += _ =>
        {
            _uri.OpenUri(UILinks.Wiki);
        };

        CommandBinds.Builder
            .Bind(EngineKeyFunctions.EscapeMenu,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<EscapeUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        if (_escapeWindow != null)
        {
            _escapeWindow.Dispose();
            _escapeWindow = null;
        }

        if (_escapeButton != null)
        {
            _escapeButton.OnPressed -= EscapeButtonOnOnPressed;
            _escapeButton.Pressed = false;
            _escapeButton = null;
        }

        CommandBinds.Unregister<EscapeUIController>();
    }

    private void EscapeButtonOnOnPressed(ButtonEventArgs obj)
    {
        ToggleWindow();
    }

    private void CloseEscapeWindow()
    {
        _escapeWindow?.Close();
    }

    private void ToggleWindow()
    {
        if (_escapeWindow == null)
            return;

        if (_escapeWindow.IsOpen)
        {
            CloseEscapeWindow();
        }
        else
        {
            _escapeWindow.OpenCentered();
            _escapeButton!.Pressed = true;
        }
    }
}
