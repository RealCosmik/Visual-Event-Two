using System;
using System.Text;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
namespace VisualEvent
{
    public static class Utility
    {
        public const BindingFlags memberBinding = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

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
                var method_params = (member_info as MethodInfo).GetParameters();
                for (int i = 0; i < method_params.Length; i++)
                    member_data.Add(method_params[i].ParameterType.AssemblyQualifiedName);
            }
            else
                // place holder to be populated by member to tell if it will be treated on deseralize as setfield or get field
                member_data.Add(null);
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
            var member_type = (MemberTypes)ConvertStringToInt(methodata[0]);
            var member_name = methodata[1];
            if (member_type != MemberTypes.Method) //field or property
            {
                var reflected_members = CurrentType.GetMember(member_name, member_type, memberflags);
                if (reflected_members != null && reflected_members.Length > 0)
                    member_info = reflected_members[0];
                paramtypes = null;
            }
            // here we do work if the reflected member is a method
            else
            {
                paramtypes = null;
                MethodInfo method_info;
                // data only contiains membertype and method name i.e its a void method
                if (methodata.Length == 2)
                    method_info = CurrentType.GetMethod(member_name, memberflags, null, Type.EmptyTypes, null);
                else
                {
                    paramtypes = new Type[methodata.Length - 2];
                    for (int i = 2; i < methodata.Length; i++)
                    {
                        paramtypes[i - 2] = Type.GetType(methodata[i]);
                    }
                    method_info = CurrentType.GetMethod(member_name, memberflags, null, paramtypes, null);
                }
                //method_info.MethodHandle.GetFunctionPointer();
                member_info = method_info;
            }
            //  Debug.Log(member_info == null);
            return member_info;
        }
        private static int ConvertStringToInt(string membertype)
        {
            int num = 0;
            for (int i = 0; i < membertype.Length; i++)
                num = (num * 10) + (membertype[i] - '0');
            return num;
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
        public const string STRING_TYPE_NAME = "System.String";
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

    }
}
