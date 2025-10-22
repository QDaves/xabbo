using Xabbo.Components;
using Xabbo.ViewModels;

namespace Xabbo.Controllers;

// TODO A better way to initialize persistent background services.

#pragma warning disable CS9113 // Parameter is unread.
public sealed class ControllerInitializer(
    ClickToController clickTo,
    PrivacyController privacy,
    RoomFurniController roomFurni,
    RoomModerationController moderaion,
    RoomRightsController rights,
    MimicViewModel mimic
);
#pragma warning restore CS9113 // Parameter is unread.