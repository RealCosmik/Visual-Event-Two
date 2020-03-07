using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EventsPlus
{
    public class ArgumentReference : Argument
    {
        public Object m_reference;
        string[] MemberNames;
        List<IMember> Members;
        public void SetRefernce(Object new_reffernce,System.Type filter)
        {
            m_reference = new_reffernce;
            Members = m_reference.GetType().GetMemberList(filter);
            MemberNames = new string[Members.Count];
            for (int i = 0; i < Members.Count; i++)
            {
                MemberNames[i] = Members[i].GetdisplayName();
            }
        }
    }
}
