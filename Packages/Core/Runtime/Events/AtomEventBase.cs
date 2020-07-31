using System;
using UnityEngine;

namespace UnityAtoms
{
    /// <summary>
    /// None generic base class for Events. Inherits from `BaseAtom` and `ISerializationCallbackReceiver`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    public abstract class AtomEventBase : BaseAtom, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Event without value.
        /// </summary>
        public event Action OnEventNoValue;

        #region DEBUGGING

        public bool EnableDebugTracking = true;
        public EventDebugBreak DebugTrigger = EventDebugBreak.NoBreak;

        // watcher for trigger
        protected static EventWatcher Watcher = null;

        protected AtomEventBase()
        {
            // skip debug setup outside of editor
            if( !Application.isEditor )
            {
                return;
            }

            // create debug interface instance
            if( Watcher == null )
            {
                Watcher = new EventWatcher();
            }
        }

        #endregion

        public virtual void Raise()
        {
            Watcher?.RaisePreCall( this );

            OnEventNoValue?.Invoke();

            Watcher?.RaiseAfterCallBase( this );
        }

        /// <summary>
        /// Register handler to be called when the Event triggers.
        /// </summary>
        /// <param name="del">The handler.</param>
        public void Register(Action del)
        {
            OnEventNoValue += del;
        }

        /// <summary>
        /// Unregister handler that was registered using the `Register` method.
        /// </summary>
        /// <param name="del">The handler.</param>
        public void Unregister(Action del)
        {
            OnEventNoValue -= del;
        }

        /// <summary>
        /// Register a Listener that in turn trigger all its associated handlers when the Event triggers.
        /// </summary>
        /// <param name="listener">The Listener to register.</param>
        public void RegisterListener(IAtomListener listener)
        {
            OnEventNoValue += listener.OnEventRaised;
        }

        /// <summary>
        /// Unregister a listener that was registered using the `RegisterListener` method.
        /// </summary>
        /// <param name="listener">The Listener to unregister.</param>
        public void UnregisterListener(IAtomListener listener)
        {
            OnEventNoValue -= listener.OnEventRaised;
        }

        public void OnBeforeSerialize() { }

        public virtual void OnAfterDeserialize()
        {
            // Clear all delegates when exiting play mode
            OnEventNoValue = null;
        }

    }
}
