using ReactiveUI;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Extension;
using Xabbo.Messages.Flash;

namespace Xabbo.Controllers;

[Intercept]
public sealed partial class RoomAvatarsController : ControllerBase
{
    private readonly RoomManager _roomManager;

    [Reactive] public bool HideAvatars { get; set; }

    public RoomAvatarsController(IExtension extension, RoomManager roomManager) : base(extension)
    {
        _roomManager = roomManager;

        this
            .WhenAnyValue(x => x.HideAvatars)
            .Subscribe(hideAvatars => {
                if (hideAvatars)
                    DoHideAvatars();
                else
                    DoShowAvatars();
            });
    }

    void DoHideAvatars()
    {
        if (_roomManager.EnsureInRoom(out var room))
        {
            foreach (var avatar in room.Avatars)
            {
                Send(new AvatarRemovedMsg(avatar.Index));
            }
        }
    }

    void DoShowAvatars()
    {
        if (_roomManager.EnsureInRoom(out var room))
        {
            Send(new AvatarsAddedMsg(room.Avatars.OfType<Avatar>()));
            Send(new AvatarStatusMsg(room.Avatars.Select(x => x.CurrentUpdate).OfType<AvatarStatus>()));

            foreach (var avatar in room.Avatars)
            {
                if (avatar.HandItem != 0)
                    Send(new AvatarHandItemMsg(avatar.Index, avatar.HandItem));

                if (avatar.Effect != 0)
                    Send(new AvatarEffectMsg(avatar.Index, avatar.Effect, 0));

                if (avatar.Dance != 0)
                    Send(new AvatarDanceMsg(avatar.Index, avatar.Dance));
            }
        }
    }

    [InterceptIn(nameof(In.Users))]
    void HandleUsers(Intercept e)
    {
        if (HideAvatars)
            e.Block();
    }

    [InterceptIn(nameof(In.Chat), nameof(In.Shout))]
    void HandleChat(Intercept e)
    {
        if (HideAvatars)
            e.Block();
    }
}
