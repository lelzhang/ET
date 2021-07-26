using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Generated;
using ILRuntime.Runtime.Intepreter;
using UnityEngine;

namespace ET
{
	public static class ILHelper
	{
		public unsafe static void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
		{
			// 注册重定向函数
			var arr = typeof(GameObject).GetMethods();
			foreach (var i in arr)
			{
				if (i.Name == "AddComponent" && i.GetGenericArguments().Length == 1)
				{
					appdomain.RegisterCLRMethodRedirection(i, MonoBehaviourAdapter.AddComponent);
				}

				if (i.Name == "GetComponent" && i.GetGenericArguments().Length == 1)
				{
					appdomain.RegisterCLRMethodRedirection(i, MonoBehaviourAdapter.GetComponent);
				}
			}



			// 注册委托
			appdomain.DelegateManager.RegisterMethodDelegate<List<object>>();
			appdomain.DelegateManager.RegisterMethodDelegate<AChannel, System.Net.Sockets.SocketError>();
			appdomain.DelegateManager.RegisterMethodDelegate<byte[], int, int>();
			appdomain.DelegateManager.RegisterMethodDelegate<IResponse>();
			appdomain.DelegateManager.RegisterMethodDelegate<Session, object>();
			appdomain.DelegateManager.RegisterMethodDelegate<Session, ushort, MemoryStream>();
			appdomain.DelegateManager.RegisterMethodDelegate<Session>();
			appdomain.DelegateManager.RegisterMethodDelegate<ILTypeInstance>();
			//appdomain.DelegateManager.RegisterFunctionDelegate<Google.Protobuf.Adapt_IMessage.Adaptor>();
			//appdomain.DelegateManager.RegisterMethodDelegate<Google.Protobuf.Adapt_IMessage.Adaptor>();
			appdomain.DelegateManager.RegisterFunctionDelegate<System.Threading.Tasks.Task>();
			//appdomain.DelegateManager.RegisterMethodDelegate<Action>();

			//注册MonoBehaviourAdapter 适配器  否则会报错 TypeLoadException: Cannot find Adaptor for:MonoBehaviour 
			appdomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());

			CLRBindings.Initialize(appdomain);

			// 注册ETModel适配器
			Assembly assembly = typeof(Object).Assembly;
			foreach (Type type in assembly.GetTypes())
			{
				object[] attrs = type.GetCustomAttributes(typeof(ILAdapterAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				object obj = Activator.CreateInstance(type);
				CrossBindingAdaptor adaptor = obj as CrossBindingAdaptor;
				if (adaptor == null)
				{
					continue;
				}
				appdomain.RegisterCrossBindingAdaptor(adaptor);
			}

			LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);
		}


	}
}