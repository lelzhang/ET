using System;
using UnityEngine.Networking;

namespace ET
{
	
	public class UnityWebRequestUpdateSystem : UpdateSystem<UnityWebRequestAsync>
	{
		public override void Update(UnityWebRequestAsync self)
		{
			self.Update();
		}
	}
	
	public class UnityWebRequestAsync : Entity
	{
		public class AcceptAllCertificate: CertificateHandler
		{
			protected override bool ValidateCertificate(byte[] certificateData)
			{
				return true;
			}
		}
		
		public static AcceptAllCertificate certificateHandler = new AcceptAllCertificate();
		
		public UnityWebRequest Request;

		public bool isCancel;

		public ETTask<bool> tcs;
		
		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			this.Request?.Dispose();
			this.Request = null;
			this.isCancel = false;
		}

		public float Progress
		{
			get
			{
				if (this.Request == null)
				{
					return 0;
				}
				return this.Request.downloadProgress;
			}
		}

		public ulong ByteDownloaded
		{
			get
			{
				if (this.Request == null)
				{
					return 0;
				}
				return this.Request.downloadedBytes;
			}
		}

		public void Update()
		{
			if (this.isCancel)
			{
				this.tcs.SetException(new Exception($"request error: {this.Request.error}"));
				return;
			}
			
			if (!this.Request.isDone)
			{
				return;
			}
			if (!string.IsNullOrEmpty(this.Request.error))
			{
				this.tcs.SetException(new Exception($"request error: {this.Request.error}"));
				return;
			}

			this.tcs.SetResult(true);
		}

		public async ETTask<bool> DownloadAsync(string url)
		{
			this.tcs = ETTask<bool>.Create();
			
			url = url.Replace(" ", "%20");
			this.Request = UnityWebRequest.Get(url);
			this.Request.certificateHandler = certificateHandler;
			this.Request.SendWebRequest();
			
			await this.tcs;
			return this.tcs.IsCompleted;
		}

		public async ETTask<bool> DownloadAsync(string url, UI ui)
		{
			var tcs = ETTask<bool>.Create(); ; //TaskCompletionSource<T>这是一种受你控制创建Task的方式。你可以使Task在任何你想要的时候完成，你也可以在任何地方给它一个异常让它失败。

			url = url.Replace(" ", "%20");//把空格" "替换成"%20"
			url = url.Replace("\\", "/");
			Log.Debug(url);
			this.Request = UnityWebRequest.Get(url);
			UnityWebRequestAsyncOperation operation = this.Request.SendWebRequest();


			//float targetValue = operation.progress;
			//float loadingSpeed = 1.0f;

			//Slider loadingSlider = (Slider)ui.GameObject.GetComponentInChildren(typeof(Slider),true);
			//Log.Msg(loadingSlider.value);
			////string loadingText = (Text)ui.GameObject.GetComponentInChildren(typeof(Text), true);
			//if (operation.progress >= 0.9f)
			//{
			//	//operation.progress的值最大为0.9
			//	targetValue = 1.0f;
			//}

			//if (targetValue != loadingSlider.value)
			//{
			//	//插值运算
			//	loadingSlider.value = Mathf.Lerp(loadingSlider.value, targetValue, Time.deltaTime * loadingSpeed);
			//	if (Mathf.Abs(loadingSlider.value - targetValue) < 0.01f)
			//	{
			//		loadingSlider.value = targetValue;
			//	}
			//}

			//string loadingText = ((int)(loadingSlider.value * 100)).ToString() + "%";

			//if ((int)(loadingSlider.value * 100) == 100)
			//{
			//	//允许异步加载完毕后自动切换场景
			//	operation.allowSceneActivation = true;

			//	// 异步加载进度条完成后再实例化Stage
			//	/*
			//             if (!Stage)
			//             {
			//                 Stage = (GameObject)GameObject.Instantiate(operation.asset, danceroot.transform);
			//             }
			//             else return;
			//             */
			//	//异步加载进度条完成前 实例化Stage并隐藏



			//	//  Debug.Log("结束");

			//}
			//Log.Msg(loadingSlider.value);
			await this.tcs;
			return this.tcs.IsCompleted;
		}

	}
}
