using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityAtoms.Editor
{
    public class EventCatalog : EditorWindow
    {
        private ScrollView EventListRoot;
        private ScrollView EventInfo;

        private Type Selection;
        private readonly Type TargetClass = typeof( AtomEventBase );

        [MenuItem("Tools/Unity Atoms/Event Catalog")]
        public static void ShowWindow()
        {
            EventCatalog window = GetWindow<EventCatalog>( false, "Event Catalog" );

            window.Show();
        }

        private void OnEnable()
        {
            VisualElement Root = rootVisualElement;

            // load style
            Root.styleSheets.Add( Resources.Load<StyleSheet>( "CatalogStyle" ) );

            // create base structure
            EventListRoot = new ScrollView();
            EventListRoot.AddToClassList( "event-list" );
            EventInfo = new ScrollView();
            EventInfo.AddToClassList( "event-info" );

            // create base box container
            VisualElement Container = new VisualElement();
            Container.AddToClassList( "event-container" );

            // move elements inside container
            Container.Add( EventListRoot );
            Container.Add( EventInfo );

            // add to root
            Root.Add( Container );

            // fill will events
            GenerateEventList();
        }

        private void GenerateEventList()
        {
            Box LeftSideContainer = new Box();

            // create container for event instances ( scriptable objects )
            Foldout InstanceContainer = new Foldout { text = "Event Objects", value = true, };

            // add to left side
            LeftSideContainer.Add( InstanceContainer );

            // list of event instances
            List<AtomEventBase> EventObjectList = GetScriptableObjectList( TargetClass );

            foreach( AtomEventBase EventObject in EventObjectList )
            {
                // create entry
                InstanceContainer.Add( CreateEntry( EventObject ) );
            }

            // add to root
            EventListRoot.Add( LeftSideContainer );

            // create container for event classes
            Foldout ClassContainer = new Foldout { text = "Event Types", value = false, };

            // add to left side
            LeftSideContainer.Add( ClassContainer );

            // list of valid event classes
            List<Type> EventClassList = GetEventClassList();

            foreach( Type EventClass in EventClassList )
            {
                // create entry
                ClassContainer.Add( CreateEntry( EventClass ) );
            }
        }

        private List<Type> GetEventClassList()
        {
            List<Type> ValidEvents = new List<Type>();

            // get all loaded assemblies
            Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // generate list of valid types
            foreach( Assembly TargetAssembly in Assemblies )
            {
                // list of all types
                Type[] Types = TargetAssembly.GetTypes();

                // filter by sub class
                foreach( Type Class in Types )
                {
                    // filter non subclasses and non instance able types
                    if( !Class.IsClass ||
                        !Class.IsSubclassOf( TargetClass ) ||
                        Class.IsAbstract ||
                        Class.IsGenericType ) continue;

                    // create entry for valid type
                    ValidEvents.Add( Class );
                }
            }

            return ValidEvents;
        }

        private List<AtomEventBase> GetScriptableObjectList( Type Target )
        {
            List<AtomEventBase> InstanceList = new List<AtomEventBase>();

            // find all scriptable objects of type
            string[] InstanceGuidArray = AssetDatabase.FindAssets( "t:" + Target.Name );

            foreach( string InstanceGuid in InstanceGuidArray )
            {
                // get path
                string InstancePath = AssetDatabase.GUIDToAssetPath( InstanceGuid );

                // load asset
                AtomEventBase Instance = AssetDatabase.LoadAssetAtPath<AtomEventBase>( InstancePath );

                InstanceList.Add( Instance );
            }

            return InstanceList;
        }

        private VisualElement CreateEntry( Type Class )
        {
            // create container
            VisualElement EntryContainer = CreateButton( Class );

            // assign class
            EntryContainer.AddToClassList( "event-entry" );

            return EntryContainer;
        }

        private VisualElement CreateEntry( AtomEventBase Object )
        {
            // create container
            VisualElement EntryContainer = CreateButton( Object );

            // assign class
            EntryContainer.AddToClassList( "event-entry" );

            return EntryContainer;
        }

        private VisualElement CreateButton( Type Class )
        {
            // create clickable Info
            Button EventButton = new Button( () => ShowEventInfo( Class ) )
            {
                text = Class.Name
            };

            return EventButton;
        }

        private VisualElement CreateButton( AtomEventBase Object )
        {
            // create clickable info
            Button ObjectButton = new Button( () => ShowObjectInfo( Object ) )
            {
                text = Object.name
            };

            return ObjectButton;
        }

        private void ShowEventInfo( Type Target )
        {
            // empty old infos
            EventInfo.Clear();

            if( Selection == Target )
            {
                Selection = null;
                return;
            }

            Selection = Target;

            // create outer container
            Box RightSideContainer = new Box();

            // create type label
            Label TypeName = new Label( "Type: " + Target.Name );
            RightSideContainer.Add( TypeName );

            // create title for list
            Label UsedTitle = new Label( "Used by:" );
            RightSideContainer.Add( UsedTitle );

            // create list of scriptable object inherited from this
            List<AtomEventBase> UsedObjects = GetScriptableObjectList( Target );
            foreach( AtomEventBase UsedObject in UsedObjects )
            {
                Button ObjectInfo = new Button( () =>
                {
                    UnityEditor.Selection.activeObject = UsedObject;
                    EditorGUIUtility.PingObject( UnityEditor.Selection.activeObject );
                })
                {
                    text = UsedObject.name
                };
                ObjectInfo.AddToClassList( "no-width" );
                RightSideContainer.Add( ObjectInfo );
            }


            // add to root
            EventInfo.Add( RightSideContainer );
        }

        private string GetDescription( AtomEventBase Instance )
        {
            FieldInfo DescriptionField = typeof( BaseAtom ).GetField( "_developerDescription", BindingFlags.Instance | BindingFlags.NonPublic );
            if( DescriptionField == null )
            {
                return null;
            }
            return (string) DescriptionField.GetValue( Instance );
        }

        private void ShowObjectInfo( AtomEventBase Target )
        {
            // empty old infos
            EventInfo.Clear();

            if( Selection == Target.GetType() )
            {
                Selection = null;
                return;
            }

            Selection = Target.GetType();

            // create outer container
            Box RightSideContainer = new Box();

            // create name label
            Label Title = new Label( "Name: " + Target.name );
            RightSideContainer.Add( Title );

            // create type label
            Label TypeName = new Label( "Type: " + Target.GetType().Name );
            RightSideContainer.Add( TypeName );

            // create description label
            Label Description = new Label( "Description: " + GetDescription( Target ) );
            RightSideContainer.Add( Description );

            // create selection button
            Button SelectionButton = new Button( () =>
            {
                UnityEditor.Selection.activeObject = Target;
                EditorGUIUtility.PingObject( UnityEditor.Selection.activeObject );
            } )
            {
                text = "Select"
            };
            RightSideContainer.Add( SelectionButton );

            // add to root
            EventInfo.Add( RightSideContainer );
        }
    }
}
