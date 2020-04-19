using System;
using UnityEditor;
using UnityEngine;
namespace VisualEvent
{
    public class RawReferenceView : RawDelegateView
    {
        public Type reference_type;
        public RawReferenceView(Type newtype) => reference_type = newtype;
        protected override void GenerateNewTargetMembers(int TargetIndex)
        {
            if (CurrentTarget != null && AvailableTargetObjects != null || CurrentTargetIndex != 0 && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                base.GenerateNewTargetMembers(TargetIndex);
                CurrentMembers = CurrentMembers.GetMemberList(reference_type);
                int membercount = CurrentMembers.Count;
                if (membercount == 0)
                {
                    if (CurrentTarget != null)
                    {
                        Debug.LogError($"{CurrentTarget.name} has no members that match the type {reference_type.FullName}");
                        CurrentTarget = null;
                    }
                }
                else
                {
                    memberNames = new string[membercount];
                    for (int i = 0; i < membercount; i++)
                    {
                        memberNames[i] = CurrentMembers[i].GetdisplayName();
                    }
                } 
            }
        }
        public void SetNewReferenceType(Type new_type)
        {
            UnityEngine.Debug.Log(new_type);
            reference_type = new_type;
            GenerateNewTargetMembers(CurrentTargetIndex);
            selectedMemberIndex = 0;
        }
        public override bool validateTarget(SerializedProperty seralizedTarget, SerializedProperty isstaticTarget)
        {
            return base.validateTarget(seralizedTarget, isstaticTarget);
        }
    }
}
