using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityAtoms
{
    public class EventWatcher
    {
        private readonly Dictionary<Type, List<MonoBehaviour>> EventListeners = new Dictionary<Type, List<MonoBehaviour>>();

        // callback events
        public static event Action<EventTrackData> RaiseTriggered;

        public void RaisePreCall<T>( AtomEventBase caller, T value )
        {
            if( caller.EnableDebugTracking )
            {
                TrackEvent( caller, value );
            }

            DebugBefore( caller );
        }

        public void RaiseAfterCall( AtomEventBase caller )
        {
            DebugAfter( caller );
        }

        public void ListenerRegistered<T>( AtomEventBase caller, IAtomListener<T> listener )
        {
            // cast to base class
            MonoBehaviour ListenerBase = listener as MonoBehaviour;

            // failed ?
            if( ListenerBase == null )
            {
                Debug.LogWarning( "Failed to cast listener to MonoBehaviour" );
                return;
            }

            // get index
            Type ID = caller.GetType();

            // has list ?
            if( !EventListeners.ContainsKey( ID ) )
            {
                EventListeners.Add( ID, new List<MonoBehaviour>() );
            }

            // add listener
            EventListeners[ID].Add( ListenerBase );
        }

        public void ListenerUnregister<T>( AtomEventBase caller, IAtomListener<T> listener )
        {
            // cast to base class
            MonoBehaviour ListenerBase = listener as MonoBehaviour;

            // failed ?
            if( ListenerBase == null )
            {
                Debug.LogWarning( "[Watcher] Failed to cast listener to MonoBehaviour" );
                return;
            }

            // get index
            Type ID = caller.GetType();

            // has list ?
            if( !EventListeners.ContainsKey( ID ) )
            {
                Debug.LogWarning( "[Watcher] Listener is not registered" );
                return;
            }

            // remove
            EventListeners[ID].Remove( ListenerBase );
        }

        public void ListenerUnregisterAll( AtomEventBase caller )
        {
            // get index
            Type ID = caller.GetType();

            // has list ?
            if( !EventListeners.ContainsKey( ID ) )
            {
                Debug.LogWarning( "[Watcher] type is not registered" );
                return;
            }

            // clear
            EventListeners[ID].Clear();
        }

        private void TrackEvent<T>( AtomEventBase caller, T value )
        {
            EventTrackData Data = new EventTrackData
            {
                Timestamp = DateTime.Now,
                CallerTrace = new StackTrace( 2 ),
                Value = value.ToString(),
                EventType = caller.GetType(),
                EventPath = AssetDatabase.GetAssetPath( caller.GetInstanceID() ),
                Listener = EventListeners.ContainsKey( caller.GetType() ) ? EventListeners[caller.GetType()] : null,
            };
            RaiseTriggered?.Invoke( Data );
        }

        private void DebugBefore( AtomEventBase caller )
        {
            // is before trigger ?
            if( caller.DebugTrigger != EventDebugBreak.BreakBeforeRaise &&
                caller.DebugTrigger != EventDebugBreak.BreakBeforeRaiseOnce )
            {
                return;
            }

            // reset if once
            if( caller.DebugTrigger == EventDebugBreak.BreakBeforeRaiseOnce )
            {
                caller.DebugTrigger = EventDebugBreak.NoBreak;
            }

            // stop
            Debug.Break();
        }
        private void DebugAfter( AtomEventBase caller )
        {
            // is after trigger ?
            if( caller.DebugTrigger != EventDebugBreak.BreakAfterRaise &&
                caller.DebugTrigger != EventDebugBreak.BreakAfterRaiseOnce )
            {
                return;
            }

            // reset if once
            if( caller.DebugTrigger == EventDebugBreak.BreakAfterRaiseOnce )
            {
                caller.DebugTrigger = EventDebugBreak.NoBreak;
            }

            // stop
            Debug.Break();
        }
    }
}
