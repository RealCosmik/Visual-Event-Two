using System;
using System.Reflection;
using System.Collections.Generic;
namespace VisualDelegates
{ 
    public static class Utility
    {
        public const string STRING_TYPE_NAME = "System.String";
        public const string CHAR_TYPE_NAME = "System.Char";
        public const string DOTNET_TYPE_NAME = "System.Type";
        public const string BOOLEAN_TYPE_NAME = "System.Boolean";
        public const string INTEGER_TYPE_NAME = "System.Int32";
        public const string LONG_TYPE_NAME = "System.Int64";
        public const string FLOAT_TYPE_NAME = "System.Single";
        public const string DOUBLE_TYPE_NAME = "System.Double";
        public const string VECTOR2_TYPE_NAME = "UnityEngine.Vector2";
        public const string VECTOR3_TYPE_NAME = "UnityEngine.Vector3";
        public const string VECTOR4_TYPE_NAME = "UnityEngine.Vector4";
        public const string QUATERNION_TYPE_NAME = "UnityEngine.Quaternion";
        public const string UNITYRECT_TYPE_NAME = "UnityEngine.Rect";
        public const string UNITYBOUNDS_TYPE_NAME = "UnityEngine.Bounds";
        public const string UNITYCOLOR_TYPE_NAME = "UnityEngine.Color";
        public const string UNITYCURVE_TYPE_NAME = "UnityEngine.AnimationCurve";
        public const string UNITYOBJECt_TYPE_NAME = "UnityEngine.Object";

        public static Dictionary<Type[], MethodInfo> DelegateFieldCreationMethods;
        public static Dictionary<Type[], MethodInfo> DelegateDynamicFieldCreationMethods;
        public static Dictionary<Type[], MethodInfo> DelegatePropertyCreationMethod;
        public static Dictionary<Type[], MethodInfo> DelegateDynamicPropertyCreationMethod;
        public static Dictionary<Type[], MethodInfo> DelegateMethodCreationMethods;
        public static Dictionary<Type[], MethodInfo> DelegateDynamicMethodCreationMethods;
        public static Dictionary<Type[], MethodInfo> DelegateFieldReturnCreationMethods;
        public const BindingFlags memberBinding = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

        static Utility()
        {
            var comparer=new MethodParamEquaility();
            DelegateFieldCreationMethods = new Dictionary<Type[], MethodInfo>(comparer);
            DelegateDynamicFieldCreationMethods = new Dictionary<Type[], MethodInfo>(comparer);
            DelegateFieldReturnCreationMethods = new Dictionary<Type[], MethodInfo>(comparer);
            DelegatePropertyCreationMethod = new Dictionary<Type[], MethodInfo>(comparer);
            DelegateDynamicPropertyCreationMethod = new Dictionary<Type[], MethodInfo>(comparer);
            DelegateMethodCreationMethods = new Dictionary<Type[], MethodInfo>(comparer);
            DelegateDynamicMethodCreationMethods = new Dictionary<Type[], MethodInfo>(comparer);
        }
        //=======================
        // Serialization
        //=======================
        /// <summary>Serializes a <see cref="System.Reflection.MemberInfo"/> into a string</summary>
        /// <param name="tInfo">MemberInfo to serialize</param>
        /// <returns>String value of the serialized member info, null if invalid</returns>
        public static string[] QuickSeralizer(MemberInfo member_info)
        {
            List<string> member_data = new List<string>(3);
            //first element is member type
            member_data.Add(((int)member_info.MemberType).ToString());
            //second element is member name
            member_data.Add(member_info.Name);
            // if its a method all subsequent elements are the type names of the method parameters
            if (member_info.MemberType == MemberTypes.Method)
            {
                var method_info = member_info as MethodInfo;
                var method_params = method_info.GetParameters();
                for (int i = 0; i < method_params.Length; i++)
                    member_data.Add(method_params[i].ParameterType.AssemblyQualifiedName);
            }
            return member_data.ToArray();
        }
        /// <summary>
        /// deseralizes a string array of method data <see cref="QuickSeralizer(MemberInfo)"/> to see seralization format
        /// </summary>
        /// <param name="CurrentType"></param>
        /// <param name="methodata"></param>
        /// <param name="memberflags"></param>
        /// <returns></returns>
        public static MemberInfo QuickDeseralizer(Type CurrentType, string[] methodata, out Type[] paramtypes, BindingFlags memberflags = memberBinding)
        {
            MemberInfo member_info = null;
            if (methodata.Length > 0)
            {
                var member_type = (MemberTypes)ConvertStringToInt(methodata[0]);
                var member_name = methodata[1];
                if (member_type != MemberTypes.Method) //field or property
                {
                    var reflected_members = CurrentType.GetMember(member_name, member_type, memberflags);
                    if (reflected_members != null && reflected_members.Length > 0)
                        member_info = reflected_members[0];
                    if (member_info is FieldInfo field_info)
                        paramtypes = new Type[1] { field_info.FieldType };
                    else if (member_info is PropertyInfo propertyInfo)
                        paramtypes = new Type[1] { propertyInfo.PropertyType };
                    else paramtypes = null;
                }
                // here we do work if the reflected member is a method
                else
                {  
                    paramtypes = null;
                    MethodInfo method_info;
                    // data only contiains membertype and method name i.e its a void method
                    if (methodata.Length == 2)
                    {
                        method_info = CurrentType.GetMethod(member_name, memberflags, null, Type.EmptyTypes, null);
                        if (method_info.ReturnType != typeof(void))
                            paramtypes = new Type[1] { method_info.ReturnType };
                    }
                    else
                    {
                        paramtypes = new Type[methodata.Length - 2];
                        for (int i = 2; i < methodata.Length; i++)
                        {
                            paramtypes[i - 2] = Type.GetType(methodata[i]);
                        }
                        method_info = CurrentType.GetMethod(member_name, memberflags, null, paramtypes, null);
                        if (method_info.ReturnType != typeof(void))
                        {
                            var all_params = new Type[paramtypes.Length + 1];
                            paramtypes.CopyTo(all_params, 0);
                            all_params[all_params.Length - 1] = method_info.ReturnType;
                            paramtypes = all_params;
                        }
                    }
                    //method_info.MethodHandle.GetFunctionPointer();
                    member_info = method_info;
                }
            }
            else paramtypes = null;
            //  Debug.Log(member_info == null);
            return member_info;
        }
        public static int ConvertStringToInt(string value)
        {
            int num = 0;
            for (int i = 0; i < value.Length; i++)
                num = (num * 10) + (value[i] - '0');
            return num;
        }
        public static string CreateDelegateErrorMessage(string[] serializedData, UnityEngine.Object target)
        {
            var member_type = (MemberTypes)ConvertStringToInt(serializedData[0]);
            return $@"{member_type}: ""{serializedData[1]}"" was removed or renamed in type: ""{target.GetType()}""";
        }


        //=======================
        // Delegates
        //=======================
        /// <summary>Gets default flags for all instance members both public and private</summary>
        public static BindingFlags InstanceFlags
        {
            get
            {
                return BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            }
        }

        //TYPE NAMES

    }
}
