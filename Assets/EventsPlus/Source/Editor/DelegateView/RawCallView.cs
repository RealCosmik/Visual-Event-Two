using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace EventsPlus
{
	//##########################
	// Class Declaration
	//##########################
	/// <summary>Stores cached data for call delegate drop-downs; used by <see cref="DrawerPublisher"/></summary>
	public class RawCallView : RawDelegateView
	{
        //=======================
        // Variables
        //=======================
        /// <summary> Dynamic types are determined by the generics of the <see cref="Publisher.types"/> the raw call resodes in  and <see cref="DrawerRawCallView.createCache(SerializedProperty)"/> 
        /// type definition used to determine if a delegate is treated as a direct invocation rather than having predefined arguments</summary>
        protected Type[] dynamicTypes;
		/// <summary>True if the selected member can be invoked without predefined arguments</summary>
		public bool isDynamicable { get; protected set; }
        /// <summary>True if the selected member is to be invoked without predefined arguments</summary>
        protected bool m_isDynamic;
        /// <summary>Gets/Sets the <see cref="m_isDynamic"/> toggle</summary>
        public virtual bool isDynamic { get => m_isDynamic; set => m_isDynamic = value && isDynamicable; }
        /// <summary>Cached predefined arguments of the selected member</summary>
        public Argument[] arguments { get; protected set; }

        public bool HasDelegateError;
        //=======================
        // Constructor
        //=======================
        /// <summary>Constructor</summary>
        /// <param name="tDynamicTypes">Type definition used to check if this call is <see cref="isDynamicable"/></param>
        public RawCallView(Type[] tDynamicTypes) => dynamicTypes = tDynamicTypes;
        public RawCallView() { }
		
		//=======================
		// Target
		//=======================
		/// <summary>Checks for discrepancies between the <see cref="UnityEditor.SerializedProperty"/>s and the cached data; tries to match the cache to the properties</summary>
		/// <param name="tTarget">Selected target property</param>
		/// <param name="tMember">Selected member property</param>
		/// <param name="tDynamic">Dynamic toggle property</param>
		/// <returns>True if the data matches</returns>
		public virtual bool validateTarget( SerializedProperty tTarget, SerializedProperty memberdataprop, SerializedProperty tDynamic )
		{
			if ( !base.validateTarget( tTarget, memberdataprop) )
			{
				isDynamic = tDynamic.boolValue;
				
				return false;
			}
			return true;
		}
        /// <summary>
        /// Called when user selects member and populates array for in editor parameter choosing
        /// </summary>
        /// <param name="memberindex"></param>
        public override void UpdateSelectedMember(int memberindex)
        {
            if (memberindex == -1)
                SetDelegateError();
            else
            {
                //   base.UpdateSelectedMember(value);
                selectedMemberIndex = memberindex;
            }
            // Generate arguments and determine if can be dynamic
            arguments = null;
            isDynamicable = false;

            if (CurrentMembers != null)
            {
                IMember tempMember = CurrentMembers[selectedMemberIndex];
                switch (tempMember.info.MemberType)
                {
                    case MemberTypes.Field:
                        arguments = new Argument[] { new Argument(tempMember.info as FieldInfo) };
                        isDynamicable = dynamicTypes != null && dynamicTypes.Length == 1 && dynamicTypes[0] == arguments[0].type;
                        break;
                    case MemberTypes.Property:
                        arguments = new Argument[] { new Argument(tempMember.info as PropertyInfo) };
                        isDynamicable = dynamicTypes != null && dynamicTypes.Length == 1 && dynamicTypes[0] == arguments[0].type;
                        break;
                    case MemberTypes.Method:
                        ParameterInfo[] tempParameters = (tempMember.info as MethodInfo).GetParameters();
                        if (tempParameters != null)
                        {
                            int tempListLength = tempParameters.Length;
                            arguments = new Argument[tempListLength];
                            //if dynamicTypes is null the meothod is void and method less
                            isDynamicable = dynamicTypes != null && tempListLength == dynamicTypes.Length; // methods without arguments are not dynamic

                            for (int i = (tempListLength - 1); i >= 0; --i)
                            {
                                arguments[i] = new Argument(tempParameters[i]);
                                if (isDynamicable && arguments[i].type != dynamicTypes[i])
                                {
                                    isDynamicable = false;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            // Turn off dynamic if not able
            if (!isDynamicable && m_isDynamic)
            {
                isDynamic = false;
            }
        }
        /// <summary>Checks for discrepancies between the <see cref="UnityEditor.SerializedProperty"/> and the cached data; tries to match the cache to the property</summary>
        /// <param name="tMember">Selected member property</param>
        /// <param name="tDynamic">Dynamic toggle property</param>
        /// <returns>True if the data matches</returns>
        public virtual bool validateMember(SerializedProperty methodDataprop, SerializedProperty tDynamic )
		{
			if ( !base.validateMember(methodDataprop) )
			{ 
				isDynamic = tDynamic.boolValue;
				return false;
			}
			
			return true;
		}
        private void SetDelegateError() 
        {
            Debug.LogError("issue");
            HasDelegateError = true;
            UpdateSelectedTarget(AvailableTargetObjects.Count - 1);
            selectedMemberIndex = 2;

        }
        public override void ClearViewCache()
        {
            Debug.Log("clearing");
            base.ClearViewCache();
            HasDelegateError = false;
            arguments = null;
            dynamicTypes = null;
        }
        public void CopyCall(RawCallView othercall)
        {
            CurrentTarget = othercall.CurrentTarget;
            AvailableTargetObjects = othercall.AvailableTargetObjects;
            CurrentMembers = othercall.CurrentMembers;
            memberNames = othercall.memberNames;
            selectedMemberIndex = othercall.selectedMemberIndex;
            arguments = othercall.arguments;
            dynamicTypes=othercall.dynamicTypes;
            HasDelegateError = othercall.HasDelegateError;
        }
	}
}