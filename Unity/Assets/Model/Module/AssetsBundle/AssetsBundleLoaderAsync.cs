using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
	[ObjectSystem]
	public class AssetsBundleLoaderAsyncSystem : UpdateSystem<AssetsBundleLoaderAsync>
	{
		public override void Update(AssetsBundleLoaderAsync self)
		{
			self.Update();
		}
	}

	public class AssetsBundleLoaderAsync : Entity
	{
		private AssetBundleCreateRequest request;

		private ETTask<AssetBundle> tcs;

		public void Update()
		{
			if (!this.request.isDone)
			{
				return;
			}

			ETTask<AssetBundle> t = tcs;
			t.SetResult(this.request.assetBundle);
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();
		}

		public ETTask<AssetBundle> LoadAsync(string path)
		{
			this.tcs = ETTask<AssetBundle>.Create();
			this.request = AssetBundle.LoadFromFileAsync(path);
			return this.tcs;
		}
	}
}
