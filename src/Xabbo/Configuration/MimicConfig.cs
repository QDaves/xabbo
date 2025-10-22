using ReactiveUI;

namespace Xabbo.Configuration;

public sealed class MimicConfig : ReactiveObject
{
    [Reactive] public bool Figure { get; set; } = true;
    [Reactive] public bool Motto { get; set; } = true;
    [Reactive] public bool Action { get; set; } = true;
    [Reactive] public bool Dance { get; set; } = true;
    [Reactive] public bool Sign { get; set; } = true;
    [Reactive] public bool Effect { get; set; } = true;
    [Reactive] public bool Sit { get; set; } = true;
    [Reactive] public bool Follow { get; set; } = true;
    [Reactive] public bool Typing { get; set; } = true;
    [Reactive] public bool Talk { get; set; } = true;
    [Reactive] public bool Shout { get; set; } = true;
    [Reactive] public bool Whisper { get; set; } = true;
}
