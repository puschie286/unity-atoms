namespace UnityAtoms.Editor
{
    public class EventDelegator<T>
    {
        public void Trigger( EventTrackData<T> Parameters )
        {
            EventTracker.OnEventValueRaised( Parameters );
        }
    }
}
