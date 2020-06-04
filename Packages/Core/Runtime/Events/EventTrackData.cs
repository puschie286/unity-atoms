using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace UnityAtoms
{
    public class EventTrackData
    {
        public DateTime Timestamp;
        public StackTrace CallerTrace;
        public Type EventType;
        public string EventPath;
        public string Value;
        public List<MonoBehaviour> Listener = null;

        public string Name => Path.GetFileNameWithoutExtension( EventPath );
    }
}
