using System;
using System.Collections.Generic;
using ETModel;
using UnityEngine;

namespace ET
{
	[ObjectSystem]
	public class UiAwakeSystem : AwakeSystem<UI, string, GameObject>
	{
		public override void Awake(UI self, string name, GameObject gameObject)
		{

			self.Awake(name, gameObject);
		}
	}
	
	[HideInHierarchy]
	public sealed class UI: Entity
	{
		public GameObject GameObject;
		public string Name { get; private set; }

		public Dictionary<string, UI> children = new Dictionary<string, UI>();
		
		public void Awake(string name, GameObject gameObject)
		{
			this.children.Clear();
			//gameObject.AddComponent<ComponentView>().Component = this;
			gameObject.layer = LayerMask.NameToLayer(LayerNames.UI);
			this.Name = name;
			this.GameObject = gameObject;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();

			foreach (UI ui in this.children.Values)
			{
				ui.Dispose();
			}
			
			UnityEngine.Object.Destroy(GameObject);
			children.Clear();
		}

		public void SetAsFirstSibling()
		{
			this.GameObject.transform.SetAsFirstSibling();
		}

		public void Add(UI ui)
		{
			this.children.Add(ui.Name, ui);
			ui.Parent = this;
		}

		public void Remove(string name)
		{
			UI ui;
			if (!this.children.TryGetValue(name, out ui))
			{
				return;
			}
			this.children.Remove(name);
			ui.Dispose();
		}

		public UI Get(string name)
		{
			UI child;
			if (this.children.TryGetValue(name, out child))
			{
				return child;
			}
			GameObject childGameObject = this.GameObject.transform.Find(name)?.gameObject;
			if (childGameObject == null)
			{
				return null;
			}
			child = (UI) CreateWithComponentParent<UI, string, GameObject>(name, childGameObject);
			this.Add(child);
			return child;
		}

        private T1 CreateWithComponentParent<T1, T2, T3>(T2 name, T3 childGameObject)
        {
            throw new NotImplementedException();
        }
    }
}