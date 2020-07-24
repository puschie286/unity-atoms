using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityAtoms
{
    public class Dispatcher : MonoBehaviour
    {
        private static Thread UnityThread = null;
        private static object LockObject = new object();
        private static readonly Queue<Action> Actions = new Queue<Action>();
        private static Dispatcher Instance = null;

        private void Awake()
        {
            UnityThread = Thread.CurrentThread;

            // remove old object if exist
            if( Instance != null )
            {
                DestroyImmediate( Instance );
            }

            // store pre-created instance
            Instance = this;
        }

        public static bool IsUnityThread()
        {
            Assert.IsNotNull( UnityThread );

            return Thread.CurrentThread == UnityThread;
        }

        public static void Invoke( Action action )
        {
            EnsureExistence();

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

        private void OnDestroy()
        {
            Instance = null;
        }

        private static void EnsureExistence()
        {
            // do we already exist ?
            if( Instance != null )
            {
                return;
            }

            // create instance if not exist
            Instance = new GameObject("Dispatcher" ).AddComponent<Dispatcher>();

            // keep instance alive
            DontDestroyOnLoad( Instance.gameObject );
        }
    }
}
