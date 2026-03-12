using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeArt.Components;
using WeArt.Core;
using WeArt.Utils;

namespace WeArt.Components
{
    public class WeArtHandsSystem : MonoBehaviour
    {
        /// <summary>
        /// Defines the hands render type
        /// </summary>
        [SerializeField]
        internal HandsRenderType _handsRenderType = HandsRenderType.Always_Visible;
        
        /// <summary>
        /// Defines the hands physics type to handle optimization 
        /// </summary>
        [SerializeField]
        internal HandsPhysicsType _handsPhysicsType = HandsPhysicsType.High;

        private Transform _leftHandRig, _rightHandRig;

        private List<Collider> _thumbColliders, _indexColliders, _middleColliders, _ringColliders, _pinkyColliders = new List<Collider>();

        /// <summary>
        /// Get the hands render type.
        /// </summary>
        public HandsRenderType HandsRenderType
        {
            get => _handsRenderType;
            set => _handsRenderType = value;
        }

        private void Awake()
        {
            foreach (Transform t in transform)
            {
                if (t.GetComponent<WeArtHandController>())
                {
                    t.GetComponent<WeArtHandController>()._handsSystem = this;
                    switch (t.GetComponent<WeArtHandController>()._handSide)
                    {
                        case HandSide.Left:
                            _leftHandRig = t.gameObject.transform.GetChild(0);
                            break;
                        case HandSide.Right:
                            _rightHandRig = t.gameObject.transform.GetChild(0);
                            break;
                    }
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            HandlePhysicsType();
        }

        /// <summary>
        /// Sets the hands physics type and updates the colliders accordingly
        /// </summary>
        /// <param name="type"></param>
        public void SetHandsPhysicsType(HandsPhysicsType type) {
            if (_handsPhysicsType != type) { 
                _handsPhysicsType = type;
                HandlePhysicsType();
            }
        }

        /// <summary>
        /// Enables or disables the finger colliders based on the selected HandsPhysicsType
        /// </summary>
        public void HandlePhysicsType()
        {
            if (!_leftHandRig || !_rightHandRig) return;

            SetCollidersArrays(_thumbColliders, "ColliderThumb");
            SetCollidersArrays(_indexColliders, "ColliderIndex");
            SetCollidersArrays(_middleColliders, "ColliderMiddle");
            SetCollidersArrays(_ringColliders, "ColliderRing");
            SetCollidersArrays(_pinkyColliders, "ColliderPinky");
        }

        void SetCollidersArrays(List<Collider> colliders, string name) {
            if (colliders == null || colliders.Count == 0)
            {
                colliders = _leftHandRig.GetComponentsInChildren<Collider>(true).Where(t => t.name.Contains(name)).ToList();
                colliders.AddRange(_rightHandRig.GetComponentsInChildren<Collider>(true).Where(t => t.name.Contains(name)).ToList());
            }

            foreach (Collider c in colliders)
            {
                c.enabled = _handsPhysicsType == HandsPhysicsType.High;
            }
        }
    }
}
