using System;
using System.Collections.Generic;
using System.Reactive;
using ReactiveUI;

using Xabbo.Extension;
using Xabbo.Interceptor;
using Xabbo.Messages;
using Xabbo.Messages.Flash;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Services.Abstractions;
using Xabbo.Controllers;
using Xabbo.Configuration;

namespace Xabbo.ViewModels;

[Intercept]
public sealed partial class MimicViewModel : ControllerBase
{
    private enum MimicState { Idle, Selecting, Active }

    private readonly IUiContext _uiContext;
    private readonly IConfigProvider<AppConfig> _config;
    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    private MimicState _state = MimicState.Idle;
    private Id _targetId = -1;
    private int _targetIndex = -1;
    private string? _targetName;
    private string? _activeEffect;

    private string? _originalFigure;
    private Gender _originalGender;
    private string? _originalMotto;

    public MimicConfig Config => _config.Value.Mimic;

    [Reactive] public string ButtonText { get; set; } = "Start";
    [Reactive] public string? TargetFigure { get; set; }
    [Reactive] public string? StatusText { get; set; }

    public ReactiveCommand<Unit, Unit> ToggleMimicCmd { get; }

    public MimicViewModel(
        IExtension extension,
        IConfigProvider<AppConfig> config,
        IUiContext uiContext,
        ProfileManager profileManager,
        RoomManager roomManager)
        : base(extension)
    {
        _config = config;
        _uiContext = uiContext;
        _profileManager = profileManager;
        _roomManager = roomManager;

        ToggleMimicCmd = ReactiveCommand.Create(ToggleMimic);

        _roomManager.Left += OnRoomLeft;
        _roomManager.Entered += OnRoomEntered;
        _roomManager.AvatarAdded += OnAvatarAdded;
        _roomManager.AvatarUpdated += OnAvatarUpdated;
        _roomManager.AvatarRemoved += OnAvatarRemoved;
    }

    private void ToggleMimic()
    {
        if (_state == MimicState.Idle)
        {
            _state = MimicState.Selecting;
            ButtonText = "Cancel";
            StatusText = "Click on a user to mimic...";
            TargetFigure = null;
        }
        else
        {
            StopMimic();
        }
    }

    private void StopMimic()
    {
        if (_state == MimicState.Active && _originalFigure != null)
        {
            if (Config.Figure)
                Ext.Send(Out.UpdateFigureData, _originalGender.ToClientString(), _originalFigure);
            if (Config.Motto && _originalMotto != null)
                Ext.Send(Out.ChangeMotto, _originalMotto);
        }

        _state = MimicState.Idle;
        _targetId = -1;
        _targetIndex = -1;
        _targetName = null;
        ButtonText = "Start";
        StatusText = null;
        TargetFigure = null;
        _activeEffect = null;
        _originalFigure = null;
        _originalMotto = null;
    }

    private void OnRoomLeft()
    {
        _uiContext.Invoke(() => StopMimic());
    }

    private void OnAvatarAdded(AvatarEventArgs e)
    {
        if (e.Avatar is not IUser user) return;

        if (user.Id == _targetId && _state == MimicState.Active)
        {
            _targetIndex = user.Index;
            _uiContext.Invoke(() =>
            {
                if (Config.Figure)
                    Ext.Send(new UpdateAvatarMsg(user.Gender, user.Figure));
                if (Config.Motto)
                    Ext.Send(Out.ChangeMotto, user.Motto);
            });
        }
    }

    private void OnAvatarUpdated(AvatarEventArgs e)
    {
        if (e.Avatar is not IUser user) return;

        if (_state == MimicState.Active && user.Id == _targetId)
        {
            if (Config.Figure)
                Ext.Send(Out.UpdateFigureData, user.Gender.ToClientString(), user.Figure);
            if (Config.Motto)
                Ext.Send(Out.ChangeMotto, user.Motto);
        }
    }

    private void OnAvatarRemoved(AvatarEventArgs e)
    {
        if (e.Avatar.Index == _targetIndex)
            _uiContext.Invoke(() => StopMimic());
    }

    private void OnRoomEntered(RoomEventArgs e)
    {
        _uiContext.Invoke(() => StopMimic());
    }

    [Intercept(~ClientType.Shockwave)]
    [InterceptOut(nameof(Out.GetSelectedBadges))]
    private void OnUserClicked(Intercept e)
    {
        if (_state != MimicState.Selecting) return;

        Id clickedId = e.Packet.Read<Id>();

        if (!_roomManager.EnsureInRoom(out var room)) return;
        if (clickedId == _profileManager.UserData?.Id) return;

        if (!room.TryGetUserById(clickedId, out IUser? user))
        {
            _uiContext.Invoke(() => StatusText = $"User not found (id={clickedId})");
            return;
        }

        var userData = _profileManager.UserData;
        if (userData != null)
        {
            _originalFigure = userData.Figure;
            _originalGender = userData.Gender;
            _originalMotto = userData.Motto;
        }

        _targetId = user.Id;
        _targetIndex = user.Index;
        _targetName = user.Name;
        _state = MimicState.Active;

        _uiContext.Invoke(() =>
        {
            ButtonText = "Stop";
            StatusText = $"Mimicking: {_targetName}";
            TargetFigure = user.Figure;

            if (Config.Figure)
                Ext.Send(Out.UpdateFigureData, user.Gender.ToClientString(), user.Figure);

            if (Config.Motto)
                Ext.Send(Out.ChangeMotto, user.Motto);
        });
    }

    [InterceptOut(nameof(Out.LookTo))]
    private void BlockLookWhileSelecting(Intercept e)
    {
        if (_state == MimicState.Selecting)
            e.Block();
    }

    [Intercept]
    private void OnAvatarMovement(AvatarStatusMsg msg)
    {
        if (_state != MimicState.Active || _targetIndex == -1)
            return;

        foreach (var update in msg)
        {
            if (update.Index != _targetIndex)
                continue;

            if (Config.Follow)
            {
                Ext.Send(Out.LookTo, update.Location.X, update.Location.Y);
                if (update.MovingTo != null)
                    Ext.Send(Out.MoveAvatar, update.Location.X, update.Location.Y);
            }

            if (Config.Sit)
            {
                if (update.Stance == AvatarStance.Sit)
                    Ext.Send(Out.ChangePosture, 1);
                else
                    Ext.Send(Out.ChangePosture, 0);
            }

            if (Config.Sign && update.Sign != AvatarSign.None)
            {
                Ext.Send(Out.Sign, (int)update.Sign);
            }
        }
    }

    [Intercept]
    private void OnAction(AvatarActionMsg msg)
    {
        if (!Config.Action || _state != MimicState.Active || msg.Index != _targetIndex)
            return;

        Ext.Send(Out.AvatarExpression, (int)msg.Action);
    }

    [Intercept]
    private void OnDance(AvatarDanceMsg msg)
    {
        if (!Config.Dance || _state != MimicState.Active || msg.Index != _targetIndex)
            return;

        Ext.Send(Out.Dance, (int)msg.Dance);
    }

    [Intercept]
    private void OnEffect(AvatarEffectMsg msg)
    {
        if (_state != MimicState.Active || msg.Index != _targetIndex)
            return;

        int effectId = msg.Effect;

        if (effectId is 140 or 196 or 136)
        {
            string command = effectId switch
            {
                140 => ":habnam",
                196 => ":YYXXABXA",
                136 => ":moonwalk",
                _ => ""
            };

            Ext.Send(Out.Chat, command, 0, -1);
            _activeEffect = command;
        }
        else if ((effectId == 0 || effectId == -1) && _activeEffect != null)
        {
            Ext.Send(Out.Chat, _activeEffect, 0, -1);
            _activeEffect = null;
        }

        if (Config.Effect)
        {
            if (effectId == 0 || effectId == -1)
            {
                Ext.Send(Out.AvatarEffectActivated, -1);
                Ext.Send(Out.AvatarEffectSelected, -1);
            }
            else if (effectId is not (140 or 196 or 136))
            {
                Ext.Send(Out.AvatarEffectActivated, effectId);
                Ext.Send(Out.AvatarEffectSelected, effectId);
            }
        }
    }

    [Intercept]
    private void OnTyping(AvatarTypingMsg msg)
    {
        if (!Config.Typing || _state != MimicState.Active || msg.Index != _targetIndex)
            return;

        if (msg.Typing)
            Ext.Send(Out.StartTyping);
        else
            Ext.Send(Out.CancelTyping);
    }

    [Intercept]
    private void OnChat(AvatarChatMsg msg)
    {
        if (_state != MimicState.Active || msg.AvatarIndex != _targetIndex)
            return;

        if ((msg.Type == ChatType.Talk && !Config.Talk) ||
            (msg.Type == ChatType.Shout && !Config.Shout) ||
            (msg.Type == ChatType.Whisper && !Config.Whisper))
            return;

        Identifier outIdentifier = msg.Type switch
        {
            ChatType.Talk => Out.Chat,
            ChatType.Shout => Out.Shout,
            ChatType.Whisper => Out.Whisper,
            _ => Out.Chat
        };

        if (msg.Type == ChatType.Whisper)
            Ext.Send(outIdentifier, $"{_targetName} {msg.Message}", msg.BubbleStyle);
        else
            Ext.Send(outIdentifier, msg.Message, msg.BubbleStyle, -1);
    }
}
