using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
namespace VisualDelegates.Editor
{
    //##########################
    // Class Declaration
    //##########################
    /// <summary>Stores cached data for delegate drop-downs; used by the delegate inspector drawers</summary>
    public abstract class RawDelegateView
    {
        public Color color = Color.clear;
        /// <summary>Index of the currently selected member</summary>
        protected const float DefaultHeight = 10f;
        //=======================
        // Variables 
        //=======================
        /// <summary>Cached target objects</summary>
        protected List<UnityEngine.Object> AvailableTargetObjects;
        /// <summary>Display names of each target in a drop-down</summary>
        public string[] _targetNames;
        /// <summary>Index of the currently selected target</summary> 
        public int CurrentTargetIndex { get; protected set; }
        /// <summary>Display names of each of the members belonging to the selected target</summary>
        public string[] memberNames { get; protected set; }
        /// <summary>Cached members belonging to the selected target</summary>
        protected List<IMember> CurrentMembers;
        /// <summary>Gets/Sets the <see cref="selectedMemberIndex"/></summary>
        public virtual int selectedMemberIndex { get; protected set; }
        /// <summary>Gets the current selected member of the view</summary>
        public IMember SelectedMember => CurrentMembers == null || selectedMemberIndex >= CurrentMembers.Count ? null : CurrentMembers[selectedMemberIndex];

        /// <summary>Gets/Sets the selected target object; if set, regenerates the target's members</summary>
        public virtual UnityEngine.Object CurrentTarget { get; protected set; }
        /// <summary>
        /// The height in unity units of this current delegateview
        /// </summary>
        public float Height { get; set; } = DefaultHeight;
        /// <summary>
        /// Signal to property drawers that delegateview height needs to be recalculated
        /// </summary>
        public bool RequiresRecalculation = true;
        /// <summary>
        /// validation status of this current cache instance
        /// </summary>
        public bool isvalidated;
        public bool serializationError;
        public bool executionError;
        public RawDelegateView()
        {
            Undo.undoRedoPerformed += () => ClearViewCache();
        }
        public void trythis()
        {
#if LOG
            Debug.Log("try this");
#endif
        }
        /// <summary>
        /// Clears the data from this view when delegate is removed
        /// </summary>
        public virtual void ClearViewCache()
        {
            Debug.Log("nah this gets called alot");
            // there seems to be a weird bug in unity where while drag and drops are being perfomed 
            // undo/redo is constantly being called
            AvailableTargetObjects = null;
            CurrentTarget = null;
            CurrentMembers = null;
            memberNames = null;
            isvalidated = false;
            RequiresRecalculation = true;
        }
        /// <summary>
        /// Returns the given Object from the delegatView's target tree
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public UnityEngine.Object GetObjectFromTree(int index)
        {
            if (index < AvailableTargetObjects.Count)
                return AvailableTargetObjects[index];
            else return null;
        }
        /// <summary>
        /// Sets the parent target of this delegate. This object will be used to construct delegate component tree
        /// </summary>
        /// <param name="ParentObj"></param>
        public void SetParentTarget(UnityEngine.Object ParentObj)
        {
            CurrentTarget = ParentObj;
            CurrentTargetIndex = GenerateChildTargets(ParentObj, out AvailableTargetObjects, out _targetNames);
            GenerateNewTargetMembers(CurrentTargetIndex);
        }
        /// <summary>
        /// Updates the the current target from<see cref="AvailableTargetObjects"/>
        /// </summary>
        /// <param name="newTargetIndex"> index in <see cref="AvailableTargetObjects"/></param>
        public void UpdateSelectedTarget(int newTargetIndex)
        { 
            if (AvailableTargetObjects != null)
            {
                CurrentTargetIndex = newTargetIndex < 0 ? 0 : newTargetIndex;
                var availability_count = AvailableTargetObjects.Count;
                //if (CurrentTargetIndex == availability_count - 1) // this can be static OR scriptable object
                //{
                //    if (AvailableTargetObjects[CurrentTargetIndex] == null) // if last item is null its static utility
                //        hasStaticTarget = true;
                //    else
                //    {
                //        //if its not null it was a scriptable objects since they have no children components
                //        CurrentTarget = AvailableTargetObjects[CurrentTargetIndex]; 
                //        hasStaticTarget = false;
                //    }
                //}
                //// any other index thats not the bottom of the list is some component child
                //else
                {
                   
                    CurrentTarget = AvailableTargetObjects[CurrentTargetIndex];
                }
                GenerateNewTargetMembers(CurrentTargetIndex);
            }
            else Debug.LogError("No parent target was set for this delegate");
        }

        /// <summary>Generates a drop-down list of a target's relatives and output display names</summary>
        /// <param name="new_target">Target object</param>
        /// <param name="target_tree">Output list of drop-down targets</param>
        /// <param name="tTargetNames">Output list of drop-down target display names</param>
        /// <returns>The relative index of the <paramref name="new_target"/> relative to the outputted <paramref name="target_tree"/></returns>
        protected virtual int GenerateChildTargets(UnityEngine.Object new_target, out List<UnityEngine.Object> target_tree, out string[] target_names)
        {
            if (new_target == null)
            {
                target_tree = null;
                target_names = null;
            }
            else
            {
                // set selected target to second target obj in component tree
                int tempTarget_index = 1;
                // Try to get GameObject
                GameObject FocusedTarget = null;
                if (new_target is GameObject)
                    FocusedTarget = new_target as GameObject;
                // user may drag and drop component directly instead of gameobject
                else if (new_target is Component foucsedComponent)
                    FocusedTarget = foucsedComponent.gameObject;

                // Component tree
                if (FocusedTarget != null)
                {
                    List<string> targetnames = new List<string>();
                    targetnames.Add("NONE");

                    targetnames.Add(FocusedTarget.name);

                    target_tree = new List<UnityEngine.Object>();
                    target_tree.Add(null);
                    target_tree.Add(FocusedTarget);

                    Component[] tempComponents = FocusedTarget.GetComponents<Component>();
                    int tempListLength = tempComponents.Length;
                    for (int i = 0; i < tempListLength; i++)
                    {
                        //in case there is a component in the tree that has loaded with an error
                        if (tempComponents[i] != null)
                        {
                            string tempName = tempComponents[i].GetType().Name;
                            targetnames.Add(FocusedTarget.name + "." + tempName); //gameobjectname.componentname 
                            target_tree.Add(tempComponents[i]);

                            if (FocusedTarget == null && tempComponents[i] == new_target)
                            {
                                tempTarget_index += i + 1;
                            }
                        }
                    }


                    // here we populate array of all potential targetname 
                    target_names = targetnames.ToArray();

                }
                // was not a gameobject OR a component??
                else
                {
                    target_tree = new List<UnityEngine.Object> { null, new_target };
                    target_names = new string[] { "NONE", new_target.name };
                }
                return target_tree.IndexOf(new_target);
            }
            return 0;
        }

        /// <summary>
        /// Checks if any components have been added or removed from component tree
        /// </summary>
        /// <param name="delegateproperty"></param>
        public void ValidateComponentTree()
        {
            if (AvailableTargetObjects != null)
            {
                var target_object = AvailableTargetObjects[1] as GameObject; // main object is always in the ssecond index

                // we use count-2 because our list is always 3 elements more than component list.
                // our list contains null, and the gameobject, so to compare componets we do count-2
                if (target_object != null && target_object.GetComponents<Component>().Length != AvailableTargetObjects.Count - 2)
                {
                    SetParentTarget(AvailableTargetObjects[CurrentTargetIndex]);
                }
            }
        }

        /// <summary>
        /// Generatess the target members from the gameobjet at given index in <see cref="AvailableTargetObjects"/>
        /// </summary>
        /// <param name="TargetIndex"></param>
        protected virtual void GenerateNewTargetMembers(int TargetIndex)
        {
            // Generate members and names
            if (AvailableTargetObjects == null || CurrentTargetIndex == 0)
            {
                ClearViewCache();
                return;
            }

                CurrentMembers = AvailableTargetObjects[CurrentTargetIndex].GetType().GetMemberList();

            int tempListLength = CurrentMembers.Count;
            memberNames = new string[tempListLength]; // update membernames
            for (int i = (tempListLength - 1); i >= 0; --i)
            {
                memberNames[i] = CurrentMembers[i].GetdisplayName();
            }
            // Select member
            selectedMemberIndex = 0;
        }

        /// <summary>Checks for discrepancies between the <see cref="UnityEditor.SerializedProperty"/>s and the cached data; tries to match the cache to the properties
        /// because on recompile or when switching gameobjects cache data will get desynced</summary>
        /// <param name="seralizedTarget">Selected target property</param>
        /// <param name="seralizedMember">Selected member property</param>
        /// <returns>True if the data matches</returns> 
        public virtual bool validateTarget(SerializedProperty seralizedTarget)
        { 
            //  on assembly recompile we must rebuild the view from the seralized values of the delegate
            if (AvailableTargetObjects == null && seralizedTarget.objectReferenceValue != null)
            {
                
                UnityEngine.Object newtarget = seralizedTarget.objectReferenceValue;
                GenerateChildTargets(newtarget, out AvailableTargetObjects, out _targetNames);
                {
                    if (this is RawReferenceView)
                        Debug.Log("non static ref");
                    UpdateSelectedTarget(AvailableTargetObjects.IndexOf(seralizedTarget.objectReferenceValue));
                }
                return false;
            }
            // if user reorders components in editor 
            else if (seralizedTarget.objectReferenceValue != null && seralizedTarget.objectReferenceValue != CurrentTarget && AvailableTargetObjects != null)
            {
                if (this is RawReferenceView)
                    Debug.Log("checking next");
                // Debug.Log("mis match targets");
                int tempIndex = AvailableTargetObjects.IndexOf(seralizedTarget.objectReferenceValue);
                if (tempIndex >= 0)
                {
                      Debug.LogWarning("found the index");
                    CurrentTargetIndex = tempIndex;
                    GenerateNewTargetMembers(CurrentTargetIndex);
                }
                //could not find target in seralized object so just force set
                else
                {
                     Debug.LogWarning("setting here");
                    CurrentTargetIndex = 0;
                    SetParentTarget(seralizedTarget.objectReferenceValue);
                }
                return false;
            }
            // on return from playmode
            else if (AvailableTargetObjects != null && CurrentTarget == null && seralizedTarget.objectReferenceValue != null)
            {
                CurrentTarget = seralizedTarget.objectReferenceValue;
                return false;
            }
            else if (AvailableTargetObjects == null)
                return false;
            return true;
        }


        /// <summary>Checks for discrepancies between the <see cref="UnityEditor.SerializedProperty"/> and the cached data; tries to match the cache to the property</summary>
        /// <param name="tMember">Selected member property</param>
        /// <returns>True if the data matches</returns>
        public virtual bool validateMember(SerializedProperty memberDataprop)
        {
            var seralizedMethodData = GetSeralizedMemberDataFromprop(memberDataprop);
            if (CurrentMembers == null || seralizedMethodData == null)
            {
                 //Debug.Log("no members");
                UpdateSelectedMember(0);
                return false;
            }
            UpdateSelectedMember(findMember(seralizedMethodData));
            return selectedMemberIndex >= 0;
        }


        /// <summary>Finds the index of a serialized member name within the <see cref="CurrentMembers"/> list</summary>
        /// <param name="tSerializedName">Serialized name of the member being searched</param>
        /// <returns>Index if found, -1 if not, 0 if member is null or empty</returns>
        protected virtual int findMember(string[] seralizedmethodData)
        {
            int index = -1;
            //no member seralized member set
            if (seralizedmethodData.Length == 0 || seralizedmethodData == null)
            {
                index = 0;
            }
            // search all members for the index
            else if (CurrentMembers != null && seralizedmethodData.Length > 0)
            {  
                //filters based on member type  (field,prop,method)
                var possibleMembers = CurrentMembers.Where(m => m.SeralizedData[0] == seralizedmethodData[0]);
                for (int i = 0; i < possibleMembers.Count(); i++)
                {
                    if (possibleMembers.ElementAt(i).SeralizedData.SequenceEqual(seralizedmethodData))
                    {
                       
                        index = CurrentMembers.IndexOf(possibleMembers.ElementAt(i));
                        break;
                    }
                }
            }
            //index was never found
            if (index == -1)
            {
                Debug.Log(seralizedmethodData == null);
                Debug.Log(seralizedmethodData.Length == 0);
                Debug.Log(CurrentMembers == null);
                Debug.Log(CurrentTarget?.name);
                Debug.Log(AvailableTargetObjects == null);
               // Debug.Log(CurrentMembers[0].GetdisplayName());
                 
               // Debug.Log(CurrentTarget.GetType().FullName);
                for (int i = 0; i < seralizedmethodData.Length; i++)
                {
                    Debug.Log(seralizedmethodData[i]);
                }
                Debug.LogError($"cannot find member {seralizedmethodData[1]}");
            }

            return index;
        }
        /// <summary>
        /// Extract Seralized MebeerData from seralzied property propertty
        /// </summary>
        /// <param name="methodDataprop"></param>
        /// <returns></returns>
        protected string[] GetSeralizedMemberDataFromprop(SerializedProperty methodDataprop)
        {
            int array_size = methodDataprop.arraySize;
            string[] member_data = new string[array_size];
            for (int i = 0; i < array_size; i++)
            {  
                member_data[i] = methodDataprop.GetArrayElementAtIndex(i).stringValue;
            }
            return member_data;
        }
        /// <summary>
        /// Updates selected member index
        /// </summary>
        /// <param name="value"></param>
        public virtual void UpdateSelectedMember(int value) => selectedMemberIndex = value < 0 ? 0 : value;
    }
}