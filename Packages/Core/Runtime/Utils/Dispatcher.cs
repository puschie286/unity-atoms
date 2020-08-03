using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityAtoms
{
    public class Dispatcher : MonoBehaviour
    {
        private static Thread UnityThread = null;
        private static readonly object LockObject = new object();
        private static readonly Queue<Action> Actions = new Queue<Action>();
        private static Dispatcher Instance = null;

        private void Awake()
        {
            UnityThread = Thread.CurrentThread;

            // remove old object if exist
            if( Instance != null && Instance != this )
            {
                DestroyImmediate( Instance );
            }

            // make sure to keep between scene switching
            DontDestroyOnLoad( this );
        }

        public static bool IsUnityThread()
        {
            Assert.IsNotNull( UnityThread );

            return Thread.CurrentThread == UnityThread;
        }

        public IEnumerator DelayedInvoke( Action action )
        {
            yield return new WaitUntil( () => UnityThread != null );
            Invoke( action );
        }

        private static void PreInit()
        {
            if( Instance != null ) return;

            Instance = FindObjectOfType<Dispatcher>();

            if( Instance == null )
            {
                Debug.LogError( "[Dispatcher] Cant find instance" );
                throw new Exception("Failed to find Dispatcher instance");
            }
        }

        public static void Invoke( Action action )
        {
            // delay call if we haven't initialized
            if( UnityThread == null )
            {
                PreInit();
                // delay invoke call
                Instance.StartCoroutine( Instance.DelayedInvoke( action ) );
                return;
            }

            // execute if we on main thread
            if( IsUnityThread() )
            {
                action();
            }
            else
            {
                // add to queue if not
                lock( LockObject )
                {
                    Actions.Enqueue( action );
                }
            }
        }

        private void Update()
        {
            lock( LockObject )
            {
                // execute actions if exist
                if( Actions.Count > 0 )
                {
                    Actions.Dequeue()();
                }
            }
        }
    }
}
