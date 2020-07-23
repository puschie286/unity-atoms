using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityAtoms.Editor
{
    public class EventTracker : EditorWindow
    {
        private enum InfoType
        {
            Caller,
            Listener
        }

        private static readonly List<EventTrackData> EventTracks = new List<EventTrackData>();

        private VisualElement EventListRoot;
        private VisualElement InfoPanel;
        private DateTime InfoPanelTime;
        private InfoType InfoPanelType;


        [MenuItem( "Tools/Unity Atoms/Event Tracker _%#T" )]
        public static void ShowWindow()
        {
            EventTracker window = GetWindow<EventTracker>( false, "Event Logger" );

            window.Show();
        }

        [InitializeOnEnterPlayMode]
        public static void Init()
        {
            RegisterWatcher();
        }

        private static void RegisterWatcher()
        {
            // add event to list
            EventWatcher.RaiseTriggered += EventTracks.Add;
        }

        private void OnEnable()
        {
            VisualElement Root = rootVisualElement;

            // load style
            Root.styleSheets.Add( Resources.Load<StyleSheet>( "LogStyle" ) );

            // load visual structure
            VisualTreeAsset LogTree = Resources.Load<VisualTreeAsset>( "LogMain" );

            // clone structure
            LogTree.CloneTree( Root );

            // get container
            EventListRoot = Root.Query( "EventList" ).First();

            // fill with track entries
            foreach( EventTrackData Track in EventTracks )
            {
                CreateEntry( Track );
            }

            // track new events
            RegisterLiveTracking();
        }

        private void OnDisable()
        {
            // disable tracking when closing window
            UnregisterLiveTracking();
        }

        private void RegisterLiveTracking()
        {
            // register on tracking
            EventWatcher.RaiseTriggered += CreateEntry;
        }

        private void UnregisterLiveTracking()
        {
            // unregister on tracking
            EventWatcher.RaiseTriggered -= CreateEntry;
        }

        /**
         * CreateEntry
         * - construct entry container
         * - contains time, event info, caller info, listener info
         */
        private void CreateEntry( EventTrackData Info )
        {
            // create container for entry visuals
            Box EntryContainer = new Box();

            EntryContainer.AddToClassList( "event-entry" );

            // construct time: [12:34:56]
            EntryContainer.Add( CreateTimeInfo( Info ));

            // construct event info: ExampleEventName
            EntryContainer.Add( CreateEventInfo( Info ) );

            // construct listener info
            EntryContainer.Add( CreateListenerInfo( Info ) );

            // construct caller info:
            EntryContainer.Add( CreateCallerInfo( Info ) );

            // add class type ad end
            EntryContainer.Add( CreateTypeInfo( Info ) );

            // add to root container
            EventListRoot.Add( EntryContainer );
        }

        private VisualElement CreateTimeInfo( EventTrackData Info )
        {
            TextElement TextElement = new TextElement
            {
                text = Info.Timestamp.ToString( "[HH:mm:ss]" )
            };

            // override style
            TextElement.AddToClassList( "normal-width" );
            return TextElement;
        }

        private VisualElement CreateEventInfo( EventTrackData Info )
        {
            // show text info for runtime events
            if( string.IsNullOrEmpty( Info.EventPath ) )
            {
                return new TextElement
                {
                    text = Info.EventType.Name,
                    tooltip = "EventInstance"
                };
            }

            // define local callback function
            void SelectEvent()
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath( Info.EventPath, Info.EventType );
                EditorGUIUtility.PingObject( Selection.activeObject );
            }

            // create element
            Button EventInfoButton = new Button( SelectEvent )
            {
                text = Info.Name,
                tooltip = Info.EventType.Name // TODO: doesnt work
            };

            // set info class
            EventInfoButton.AddToClassList( "event-info" );

            return EventInfoButton;
        }

        private VisualElement CreateCallerInfo( EventTrackData Info )
        {
            // create element
            Button EventCallerButton = new Button
            {
                text = "Caller"
            };

            // define local callback function
            void OpenCallerInfo()
            {
                if( CreateOrCloseInfoPanel( Info.Timestamp, InfoType.Caller ) ) return;

                // TODO: add more caller info
                InfoPanel.Add( new TextElement
                {
                    text = "Value: " + Info.Value + "\n" + Info.CallerTrace
                });

                // insert after current entry
                InsertAfter( EventCallerButton.parent, InfoPanel );
            }

            // add bind local function
            EventCallerButton.clickable = new Clickable( OpenCallerInfo );

            // set caller class
            EventCallerButton.AddToClassList( "event-caller" );

            return EventCallerButton;
        }

        private VisualElement CreateTypeInfo( EventTrackData Info )
        {
            TextElement TypeInfo = new TextElement
            {
                text = Info.EventType.Name
            };

            TypeInfo.AddToClassList( "normal-width" );
            return TypeInfo;
        }

        private VisualElement CreateListenerInfo( EventTrackData Info )
        {
            // show text info if no data available
            if( Info.Listener == null || Info.Listener.Count == 0 )
            {
                return new TextElement
                {
                    text = "no Listeners"
                };
            }

            // create element
            Button EventListenerInfo = new Button
            {
                text = Info.Listener.Count + " Listener"
            };

            // define local callback function
            void OpenListenerInfo()
            {
                if( CreateOrCloseInfoPanel( Info.Timestamp, InfoType.Listener ) ) return;

                // make long list scroll able
                VisualElement ScrollView = new ScrollView();

                // add listener info
                foreach( MonoBehaviour Listener in Info.Listener )
                {
                    ScrollView.Add( CreateListenerEntry( Listener ) );
                }

                InfoPanel.Add( ScrollView );

                // insert after current entry
                InsertAfter( EventListenerInfo.parent, InfoPanel );
            }

            // add bind local function
            EventListenerInfo.clickable = new Clickable( OpenListenerInfo );

            // set listener class
            EventListenerInfo.AddToClassList( "event-listener" );

            return EventListenerInfo;
        }

        private VisualElement CreateListenerEntry( MonoBehaviour Listener )
        {
            VisualElement EntryContainer = new VisualElement();

            EntryContainer.AddToClassList( "event-listener-entry" );

            // define local callback function
            void SelectComponent()
            {
                Selection.activeObject = Listener.gameObject;
                EditorGUIUtility.PingObject( Selection.activeObject );
            }

            // create select object button
            EntryContainer.Add( new Button( SelectComponent )
            {
                text = Listener.gameObject.name
            } );

            // create component info
            EntryContainer.Add( new TextElement
            {
                text = Listener.GetType().Name
            });

            return EntryContainer;
        }

        private bool CreateOrCloseInfoPanel( DateTime PanelTime, InfoType Type )
        {
            // close if panel is open
            if( InfoPanel != null )
            {
                EventListRoot.Remove( InfoPanel );
                InfoPanel = null;

                // return only if we closed it by same entry
                if( PanelTime == InfoPanelTime && Type == InfoPanelType )
                {
                    return true;
                }
            }

            // set panel statics
            InfoPanelTime = PanelTime;
            InfoPanelType = Type;

            // create new panel
            InfoPanel = CreateInfoPanel( "event-info-panel" );
            return false;
        }

        private void InsertAfter( VisualElement CurrentElement, VisualElement NewElement )
        {
            int EntryIndex = EventListRoot.IndexOf( CurrentElement );
            EventListRoot.Insert( EntryIndex + 1, NewElement );
        }

        private VisualElement CreateInfoPanel( string ClassName )
        {
            Box InfoContainer = new Box();

            InfoContainer.AddToClassList( ClassName );

            return InfoContainer;
        }
    }
}
