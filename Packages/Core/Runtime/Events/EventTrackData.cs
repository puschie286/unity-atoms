using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace UnityAtoms
{
#if UNITY_EDITOR
    public class EventTrackData<T>
    {
        public DateTime Timestamp;
        public StackTrace CallerTrace;
        public Type EventType;
        public string EventPath;
        public T Value;
        public List<IAtomListener<T>> Listeners;
    }

    public class EventTrackData
    {
        public DateTime Timestamp;
        public StackTrace CallerTrace;
        public Type EventType;
        public string EventPath;
        public string Value;
        public readonly List<MonoBehaviour> Listener = new List<MonoBehaviour>();

        public string Name => Path.GetFileNameWithoutExtension( EventPath );

        public static EventTrackData NormalizeGeneric<T>( EventTrackData<T> Source )
        {
            EventTrackData TrackData = new EventTrackData
            {
                Timestamp = Source.Timestamp,
                CallerTrace = Source.CallerTrace,
                EventType = Source.EventType,
                EventPath = Source.EventPath,
                Value = Source.Value.ToString()
            };

            // remove type from listeners
            foreach( IAtomListener<T> SourceListener in Source.Listeners )
            {
                // cast to base listener
                MonoBehaviour ListenerBase = SourceListener as MonoBehaviour;

                // check if cast was success
                if( ListenerBase == null )
                {
                    continue;
                }

                TrackData.Listener.Add( ListenerBase );
            }

            return TrackData;
        }
    }
#endif
}
