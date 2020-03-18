﻿using System;
using UnityEditor;
namespace VisualEvent
{
    public class RawReferenceView : RawDelegateView
    {
        public Type reference_type;
        public RawReferenceView(Type newtype) => reference_type = newtype;
        protected override void GenerateNewTargetMembers(int TargetIndex)
        {
            if (CurrentTarget != null && AvailableTargetObjects != null||CurrentTargetIndex!=0)
            {
                base.GenerateNewTargetMembers(TargetIndex);
                CurrentMembers = CurrentMembers.GetMemberList(reference_type);
                int membercount = CurrentMembers.Count;
                if (membercount == 0)
                {
                    memberNames = new string[] { "No Applicalbe members" };
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
            reference_type = new_type;
            GenerateNewTargetMembers(CurrentTargetIndex);
            selectedMemberIndex = 0;
        }
    }
}
