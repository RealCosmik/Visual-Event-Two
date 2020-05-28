using UnityEngine;
using System.Collections.Generic;
namespace VisualDelegates.Events
{
    [System.Serializable]
    public class ResponseList
    {
        public List<EventResponse> responses;
        public ResponseList()
        {
            responses = new List<EventResponse>();
        }
    }
   
}
