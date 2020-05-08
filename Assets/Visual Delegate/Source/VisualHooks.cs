using UnityEngine;
namespace VisualDelegates
{
    public class VisualHooks : MonoBehaviour
    {
        public VisualDelegate onstart;
        private void Awake()
        { 
            onstart.initialize();
        }
        private void Start() => onstart.Invoke();
    }
}
