#if UNITY_2019_1_OR_NEWER
using UnityEditor;

namespace UnityAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `bool`. Inherits from `AtomDrawer&lt;BoolEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(BoolEvent))]
    public class BoolEventDrawer : AtomDrawer<BoolEvent> { }
}
#endif
