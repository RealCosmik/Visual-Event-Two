using System;
using UnityEngine;

namespace VisualEvent
{
    //##########################
    // Class Declaration
    //##########################
    /// <summary>Serializable argument data for any type supported by Unity's default inspector</summary>
    [Serializable]
    public class RawArgument
    {
        //=======================
        // Variables
        //=======================
        [SerializeField] protected string assemblyQualifiedArgumentName;
        /// <summary>Type of argument used by serialization</summary>
        [SerializeField] protected string FullArgumentName;
        /// <summary>Object reference</summary>
        [SerializeField] protected UnityEngine.Object objectValue;
        /// <summary>String</summary>
        [SerializeField] private string stringValue;
        /// <summary>General numbers data</summary>
        [SerializeField]
        private float _x1;
        /// <summary>General numbers data</summary>
        [SerializeField]
        private float _x2;
        /// <summary>General numbers data</summary>
        [SerializeField]
        private float _y1;
        /// <summary>General numbers data</summary>
        [SerializeField]
        private float _y2;
        /// <summary>General numbers data</summary>
        [SerializeField]
        private float _z1;
        /// <summary>General numbers data</summary>
        [SerializeField]
        private float _z2;
        [SerializeField]
        private long longValue;
        [SerializeField]
        private double doubleValue;
        [SerializeField]
        private AnimationCurve animationCurveValue;
        [SerializeField]
        private bool UseReference;
        [SerializeField]
        private bool hasCustomType;
        [SerializeField]
        public RawReference call_Reference;
        //[SerializeReference]
        //IVisualArgument custom;
        public bool isUsingreference => UseReference && call_Reference.target != null;

        //func<func<int>>;
        public Func<ArgType> CreateArgumentDelegate<ArgType>()
        {
            if (isUsingreference)
            {
                call_Reference.ParentArgumentType = assemblyQualifiedArgumentName;
                call_Reference.initialize();
                var reference_func = call_Reference.delegateInstance as Func<ArgType>;
                return reference_func;
            }
            else
            {
                ArgType static_value = (ArgType)genericValue;
                return () => static_value;
            }
        }
        //=======================
        // Accessors
        //=======================
        /// <summary>Gets the generic value of the argument based on the <see cref="type"/></summary>
        public virtual object genericValue
        {
            get
            {
                if (!String.IsNullOrEmpty(FullArgumentName))
                {
                    switch (FullArgumentName)
                    {
                        case Utility.STRING_TYPE_NAME:
                            return stringValue;
                        case Utility.DOTNET_TYPE_NAME:
                            return System.Type.GetType(stringValue);
                        case Utility.BOOLEAN_TYPE_NAME:
                            return boolValue;
                        case Utility.INTEGER_TYPE_NAME:
                            return intValue;
                        case Utility.LONG_TYPE_NAME:
                            return longValue;
                        case Utility.FLOAT_TYPE_NAME:
                            return floatValue;
                        case Utility.DOUBLE_TYPE_NAME:
                            return doubleValue;
                        case Utility.VECTOR2_TYPE_NAME:
                            return vector2Value;
                        case Utility.VECTOR3_TYPE_NAME:
                            return vector3Value;
                        case Utility.VECTOR4_TYPE_NAME:
                            return vector4Value;
                        case Utility.QUATERNION_TYPE_NAME:
                            return quaternionValue;
                        case Utility.UNITYRECT_TYPE_NAME:
                            return rectValue;
                        case Utility.UNITYBOUNDS_TYPE_NAME:
                            return boundsValue;
                        case Utility.UNITYCOLOR_TYPE_NAME:
                            return colorValue;
                        case Utility.UNITYCURVE_TYPE_NAME:
                            return animationCurveValue;
                        case Utility.UNITYOBJECt_TYPE_NAME:
                            return objectValue;
                        default:
                            Type tempType = Type.GetType(assemblyQualifiedArgumentName);
                            if (tempType != null)
                            {
                                if (tempType.IsEnum)
                                {
                                    return enumValue;
                                }
                                // for custom types that may inherit from unityobject
                                else if (tempType.IsClass && tempType.IsSubclassOf(typeof(UnityEngine.Object)))
                                {
                                    /// if the pointer is still null here then it will be treated as a Unity.Object Reference instead of a refference
                                    /// of type "tempType" so throw early before reflection throws type excpetion to save time 
                                    if (objectValue == null)
                                        throw new UnityException("Child types of Type Unity.Object cannot be left null" +
                                            $" populate the {tempType.FullName} field in the inspector to seralize delegate");
                                    else return objectValue;
                                }
                            }
                            break;
                    }
                }

                return null;
            }
        }

        /// <summary>Gets the bool value of the argument</summary>
        private bool boolValue
        {
            get
            {
                return _x1 > 0;
            }
            set
            {
                _x1 = value ? 1 : -1;
            }
        }

        /// <summary>Gets the integer value of the argument</summary>
        private int intValue
        {
            get
            {
                return (int)_x1;
            }
            set
            {
                _x1 = value;
            }
        }

        /// <summary>Gets the enumeration value of the argument</summary>
        private Enum enumValue
        {
            get
            {
                Type tempType = Type.GetType(assemblyQualifiedArgumentName);
                if (tempType != null && tempType.IsEnum)
                {
                    return (Enum)Enum.ToObject(tempType, (int)_x1);
                }

                return default(Enum);
            }
            set
            {
                _x1 = Convert.ToInt32(value);
            }
        }

        /// <summary>Gets the floating point value of the argument</summary>
        private float floatValue
        {
            get
            {
                return _x1;
            }
            set
            {
                _x1 = value;
            }
        }

        /// <summary>Gets the Vector2 value of the argument</summary>
        private Vector2 vector2Value
        {
            get
            {
                return new Vector2(_x1, _y1);
            }
            set
            {
                _x1 = value.x;
                _y1 = value.y;
            }
        }

        /// <summary>Gets the Vector3 value of the argument</summary>
        private Vector3 vector3Value
        {
            get
            {
                return new Vector4(_x1, _y1, _z1);
            }
            set
            {
                _x1 = value.x;
                _y1 = value.y;
                _z1 = value.z;
            }
        }

        /// <summary>Gets the Vector4 value of the argument</summary>
        private Vector4 vector4Value
        {
            get
            {
                return new Vector4(_x1, _y1, _z1, _x2);
            }
            set
            {
                _x1 = value.x;
                _y1 = value.y;
                _z1 = value.z;
                _x2 = value.w;
            }
        }

        /// <summary>Gets the quaternion value of the argument</summary>
        private Quaternion quaternionValue
        {
            get
            {
                return new Quaternion(_x1, _y1, _z1, _x2);
            }
            set
            {
                _x1 = value.x;
                _y1 = value.y;
                _z1 = value.z;
                _x2 = value.w;
            }
        }

        /// <summary>Gets the Rect value of the argument</summary>
        private Rect rectValue
        {
            get
            {
                return new Rect(new Vector2(_x1, _y1), new Vector2(_z1, _x2));
            }
            set
            {
                _x1 = value.position.x;
                _y1 = value.position.y;
                _z1 = value.size.x;
                _x2 = value.size.y;
            }
        }

        /// <summary>Gets the Bounds value of the argument</summary>
        private Bounds boundsValue
        {
            get
            {
                return new Bounds(new Vector3(_x1, _y1, _z1), new Vector3(_x2, _y2, _z2));
            }
            set
            {
                _x1 = value.center.x;
                _y1 = value.center.y;
                _z1 = value.center.z;
                _x2 = value.size.x;
                _y2 = value.size.y;
                _z2 = value.size.z;
            }
        }

        /// <summary>Gets the color value of the argument</summary>
        private Color colorValue
        {
            get
            {
                return new Color(_x1, _y1, _z1, _x2);
            }
            set
            {
                _x1 = value.r;
                _y1 = value.g;
                _z1 = value.b;
                _x2 = value.a;
            }
        }
    }
}