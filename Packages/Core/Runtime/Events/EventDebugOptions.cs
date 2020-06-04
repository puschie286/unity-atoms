namespace UnityAtoms
{
#if UNITY_EDITOR
    public enum EventDebugBreak
    {
        NoBreak,
        BreakBeforeRaise,
        BreakBeforeRaiseOnce,
        BreakAfterRaise,
        BreakAfterRaiseOnce
    }
#endif
}
