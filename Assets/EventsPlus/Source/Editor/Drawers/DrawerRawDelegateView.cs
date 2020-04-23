using UnityEngine;
using UnityEditor;
using RoboRyanTron.SearchableEnum.Editor;
namespace VisualEvent
{
    //##########################
    // Class Declaration
    //##########################
    /// <summary>Inspector class for rendering <see cref="RawDelegate"/>s in the inspector</summary>
    public class DrawerRawDelegateView<ViewType> : PropertyDrawer where ViewType : RawDelegateView
    {
        //=======================
        // Initialization
        /// <summary>Initializes the drawer and calculates the inspector height</summary>
        /// <param name="tProperty">Serialized delegate property</param>
        /// <param name="tLabel">GUI Label of the drawer</param>
        /// <returns>Height of the drawer</returns>
        public override float GetPropertyHeight(SerializedProperty tProperty, GUIContent tLabel)
        {
            return base.GetPropertyHeight(tProperty, tLabel);
        }


        //=======================
        // Render
        //=======================
        /// <summary>Renders the individual delegate property</summary>
        /// <param name="tPosition">Inspector position and size of <paramref name="tProperty"/></param>
        /// <param name="tProperty">Serialized delegate property</param>
        /// <param name="tLabel">GUI Label of the drawer</param>
        public override void OnGUI(Rect tPosition, SerializedProperty tProperty, GUIContent tLabel)
        {
            VisualEdiotrUtility.StandardStyle.CalcMinMaxWidth(tLabel, out float min, out float max);
            if (ViewCache.GetDelegateView(tProperty, out ViewType DelegateCache))
            {
                tPosition.height = base.GetPropertyHeight(tProperty, tLabel);
                validate(tProperty, DelegateCache);
                // Target  
                var targetpos = tPosition;
                // tPosition.x -= 120;
                // tPosition.y += 5;
                // tPosition.width += 120;
                EditorGUI.BeginChangeCheck();
                if (DelegateCache.CurrentTarget == null) // empty field
                {
                    //DelegateCache.Height = tPosition.height + 10;
                    UnityEngine.Object UserParentTarget = EditorGUI.ObjectField(targetpos, tLabel.text, null, typeof(UnityEngine.Object), true);
                    //on target change 
                    if (EditorGUI.EndChangeCheck())
                    {
                        tProperty.FindPropertyRelative("isUnityTarget").boolValue = true;
                        DelegateCache.HasDelegateError = false;
                        DelegateCache.SetParentTarget(UserParentTarget);
                        handleTargetUpdate(tProperty, DelegateCache);
                        DelegateCache.UpdateSelectedMember(DelegateCache.selectedMemberIndex);
                        handleMemberUpdate(tProperty, DelegateCache);
                        EditorGUIUtility.PingObject(UserParentTarget);
                    }
                }
                else // Target drop-down
                {
                    var validityprop = tProperty.FindPropertyRelative("isUnityTarget");
                    if (DelegateCache.CurrentTargetIndex != 0 && validityprop.boolValue == false)
                    {
                        validityprop.boolValue = true;
                    }
                    EditorGUI.BeginChangeCheck();
                    targetpos.width = tPosition.width / 3;
                    int UserSelectedTarget = EditorGUI.Popup(targetpos, tLabel.text, DelegateCache.CurrentTargetIndex, DelegateCache._targetNames);
                    // on memberchange 
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (UserSelectedTarget == 0)
                            validityprop.boolValue = false;
                        else validityprop.boolValue = true;

                        DelegateCache.HasDelegateError = false;
                        if (UserSelectedTarget != DelegateCache.CurrentTargetIndex)
                        {
                            DelegateCache.UpdateSelectedTarget(UserSelectedTarget);
                            handleTargetUpdate(tProperty, DelegateCache);
                            DelegateCache.UpdateSelectedMember(DelegateCache.selectedMemberIndex);
                            handleMemberUpdate(tProperty, DelegateCache);
                        }
                        EditorGUIUtility.PingObject(DelegateCache.CurrentTarget);
                    }
                }
                // Members
                if (DelegateCache.CurrentTarget != null)
                {
                    var memberpos = targetpos;
                    memberpos.x += memberpos.width;
                    memberpos.width = targetpos.width * 2;

                    int idHash = (tProperty.propertyPath + tProperty.serializedObject.targetObject.GetInstanceID().ToString()).GetHashCode();
                    int id = GUIUtility.GetControlID(idHash, FocusType.Keyboard, memberpos);
                    GUIContent buttonText = new GUIContent();
                    buttonText.text = DelegateCache.memberNames[DelegateCache.selectedMemberIndex];

                    if (DropdownButton(id, memberpos, buttonText))
                    {
                        var seratchWindowPos = memberpos;
                        seratchWindowPos.x += 70f;
                        System.Action<int> onSelect = userSelectedMember =>
                        {
                            DelegateCache.HasDelegateError = false;
                            DelegateCache.UpdateSelectedMember(userSelectedMember);
                            handleMemberUpdate(tProperty, DelegateCache);
                        };
                        SearchablePopup.Show(seratchWindowPos, DelegateCache.memberNames,
                            DelegateCache.selectedMemberIndex, onSelect);
                    }

                    //int tempSelectedMember = EditorGUI.Popup(memberpos, DelegateCache.selectedMemberIndex, DelegateCache.memberNames);
                    //if (EditorGUI.EndChangeCheck())
                    //{
                    //    DelegateCache.HasDelegateError = false;
                    //    DelegateCache.UpdateSelectedMember(tempSelectedMember);
                    //    handleMemberUpdate(tProperty, DelegateCache);
                    //}
                }

            }

        }


        /// <summary>Validates the delegate property against the <paramref name="tCache"/></summary>
        /// <param name="tProperty">Serialized delegate property</param>
        /// <param name="tCache">Cached delegate drop-down data</param>
        protected virtual void validate(SerializedProperty tProperty, ViewType tCache)
        {
            //this method is only used because the cache gets deleted on editor recompiles so we have to reconstruct the cache
            //using the data from the seralized property to make recompilation seem seemless on the front end

            if (!tCache.isvalidated) //validation only needs to occur once in a caches lifecycle
            {
                SerializedProperty isStatic_prop = tProperty.FindPropertyRelative("isStatic");
                SerializedProperty tempMemberProperty = tProperty.FindPropertyRelative("methodData");
                if (!tCache.validateTarget(tProperty.FindPropertyRelative("m_target"), isStatic_prop))
                {
                    handleTargetUpdate(tProperty, tCache);
                }
                if (!tCache.validateMember(tempMemberProperty))
                {
                    handleMemberUpdate(tProperty, tCache);
                }
                tCache.isvalidated = true;
            }
            else if (tCache.CurrentTarget == null)
            {
                tCache.isvalidated = false;
            }
            tCache.ValidateComponentTree();
        }

        /// <summary>Applies the target property of the <see cref="RawDelegate"/></summary>
        /// <param name="tProperty">Serialized delegate property</param>
        /// <param name="tCache">Cached delegate drop-down data</param>
        protected virtual void handleTargetUpdate(SerializedProperty tProperty, ViewType tCache)
        {
            var targetobject = tProperty.serializedObject.targetObject;
            Undo.RegisterCompleteObjectUndo(targetobject, "Delegate Target Change");
            tProperty.FindPropertyRelative("m_target").objectReferenceValue = tCache.CurrentTarget;
            tProperty.FindPropertyRelative("isStatic").boolValue = tCache.hasStaticTarget;
            tProperty.serializedObject.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(targetobject);

            //    handleMemberUpdate( tProperty, tCache );
        }

        /// <summary>Applies the member property of the <see cref="RawDelegate"/></summary>
        /// <param name="tProperty">Serialized delegate property</param>
        /// <param name="tCache">Cached delegate drop-down data</param>
        protected virtual void handleMemberUpdate(SerializedProperty tProperty, ViewType tCache)
        {
            var targetobject = tProperty.serializedObject.targetObject;
            Undo.RegisterCompleteObjectUndo(targetobject, "DelegateMemberChange");
            var methodData_prop = tProperty.FindPropertyRelative("methodData");
            if (tCache.SelectedMember == null)
                methodData_prop.arraySize = 0;
            else
                VisualEdiotrUtility.CopySeralizedMethodDataToProp(methodData_prop, tCache.SelectedMember.SeralizedData);
            if (!(tCache is RawCallView))
            {
                tProperty.serializedObject.ApplyModifiedProperties();
                PrefabUtility.RecordPrefabInstancePropertyModifications(targetobject);
            }
        }

        private static bool DropdownButton(int id, Rect position, GUIContent content)
        {
            Event current = Event.current;
            switch (current.type)
            {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && current.button == 0)
                    {
                        Event.current.Use();
                        return true;
                    }
                    break;
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl == id && current.character == '\n')
                    {
                        Event.current.Use();
                        return true;
                    }
                    break;
                case EventType.Repaint:
                    EditorStyles.popup.Draw(position, content, id, false);
                    break;
            }
            return false;
        }
    }
}
