using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
	[ObjectSystem]
	public class UIComponentAwakeSystem : AwakeSystem<UIComponent>
	{
		public override void Awake(UIComponent self)
		{
			self.Global = GameObject.Find("Global");			
			self.UIRoot = self.Global.GetComponent<ReferenceCollector>().Get<GameObject>("UI");
			self.Camera = self.UIRoot.transform.Find("UICamera").gameObject;
		}
	}
	
	/// <summary>
	/// 管理所有UI
	/// </summary>
	public class UIComponent: Entity
	{
		public GameObject Global;
		public GameObject Camera;
		public GameObject UIRoot;

		public Dictionary<string, UI> uis = new Dictionary<string, UI>();

		public void Add(UI ui)
		{
			ui.GameObject.GetComponent<Canvas>().worldCamera = this.Camera.GetComponent<Camera>();
			
			this.uis.Add(ui.Name, ui);
			ui.Parent = this;
		}

		public void Remove(string name)
		{
			if (!this.uis.TryGetValue(name, out UI ui))
			{
				return;
			}
			this.uis.Remove(name);
			ui.Dispose();
		}

		public UI Get(string name)
		{
			UI ui = null;
			this.uis.TryGetValue(name, out ui);
			return ui;
		}
	}
}