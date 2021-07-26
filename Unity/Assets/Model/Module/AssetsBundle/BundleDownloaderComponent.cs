using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
	[ObjectSystem]
	public class UiBundleDownloaderComponentAwakeSystem : AwakeSystem<BundleDownloaderComponent>
	{
		public override void Awake(BundleDownloaderComponent self)
		{
			self.bundles = new Queue<string>();
			self.downloadedBundles = new HashSet<string>();
			self.downloadingBundle = "";
		}
	}

	/// <summary>
	/// 用来对比web端的资源，比较md5，对比下载资源
	/// </summary>
	public class BundleDownloaderComponent : Entity
	{
		/// <summary>
		/// 存储远程文件的配置信息
		/// </summary>
		private VersionConfig remoteVersionConfig;

		/// <summary>
		/// 存储需更新的bundle文件名队列
		/// </summary>
		public Queue<string> bundles;

		/// <summary>
		/// 存储需要更新的文件总大小 
		/// </summary>
		public long TotalSize;

		/// <summary>
		/// 存储下载好的bundle文件名
		/// </summary>
		public HashSet<string> downloadedBundles;

		/// <summary>
		/// 存储bundles.Dequeue();出队得bundle名：DownloadAsync方法中
		/// </summary>
		public string downloadingBundle;

		public UnityWebRequestAsync webRequest;


		/// <summary>
		/// 生成dir目录下所有文件的版本信息
		/// </summary>
		/// <param name="dir"></param>
		public static void GenerateVersionInfo(string dir)
		{
			DirectoryInfo streamPath = new DirectoryInfo(dir);
			if (streamPath.Exists)
			{
				FileInfo[] fileInfos = streamPath.GetFiles();

				foreach (FileInfo fileInfo in fileInfos)
				{

					if (fileInfo.Name.EndsWith(".meta")) //Version.txt文件 也跳过
					{
						continue;
					}
				}

				VersionConfig versionProto = new VersionConfig();
				GenerateVersionProto(dir, versionProto, "");

				using (FileStream fileStream = new FileStream($"{dir}/Version.txt", FileMode.Create))
				{
					byte[] bytes = JsonHelper.ToJson(versionProto).ToByteArray();
					fileStream.Write(bytes, 0, bytes.Length);
				}
			}
		}

		private static void GenerateVersionProto(string dir, VersionConfig versionProto, string relativePath)
		{
			foreach (string file in Directory.GetFiles(dir))
			{
				if (file.EndsWith(".meta")) //Version.txt文件 也跳过
				{
					continue;
				}

				FileInfo fi = new FileInfo(file);
				if (fi.Name.StartsWith("Version"))
				{
					continue;
				}

				string md5 = MD5Helper.FileMD5(file);
				long size = fi.Length;
				string filePath = relativePath == "" ? fi.Name : $"{relativePath}/{fi.Name}";

				versionProto.FileInfoDict.Add(filePath, new FileVersionInfo
				{
					File = filePath,
					MD5 = md5,
					Size = size,
				});
			}

			foreach (string directory in Directory.GetDirectories(dir))
			{
				DirectoryInfo dinfo = new DirectoryInfo(directory);
				string rel = relativePath == "" ? dinfo.Name : $"{relativePath}/{dinfo.Name}";
				GenerateVersionProto($"{dir}/{dinfo.Name}", versionProto, rel);
			}
		}



		/// <summary>
		/// 对比服务器和本地文件，需要更新的文件名存入bundles队列 需更新的总大小存入TotalSize
		/// </summary>
		/// <returns></returns>
		public async Task StartAsync()
		{
			// 获取远程的Version.txt的配置信息
			string versionUrl = "";
			try
			{
				// 创建UnityWebRequestAsync对象webRequestAsync 并执行EventSystem.Add方法：添加webRequestAsync组件到allComponents字典<key:webRequestAsync.InstanceId , value:webRequestAsync >  
				// 根据webRequestAsync.type找到UnityWebRequestUpdateSystem 属于updateSystems  然后把webRequestAsync.InstanceId加入updates队列

				using (UnityWebRequestAsync webRequestAsync = (UnityWebRequestAsync)Entity.Create(typeof(UnityWebRequestAsync),false)) //方法结束：Dispose();
				{
					versionUrl = GlobalConfigComponent.Instance.GlobalProto.GetUrl() + "StreamingAssets/" + "Version.txt"; //获取了GlobalConfigComponent.Instance.GlobalProto 的AssetBundleServerUrl信息
					///Log.Debug(versionUrl);
					await webRequestAsync.DownloadAsync(versionUrl);
					remoteVersionConfig = JsonHelper.FromJson<VersionConfig>(webRequestAsync.Request.downloadHandler.text);
					Log.Debug(JsonHelper.ToJson(remoteVersionConfig));
				}

			}
			catch (Exception e)
			{
				throw new Exception($"url: {versionUrl}", e);
			}

			// 获取本地streaming目录的Version.txt的配置信息
			VersionConfig streamingVersionConfig;
			string versionPath = Path.Combine(PathHelper.AppResPath4Web, "Version.txt");
			//Log.Debug(versionPath);
			//


			GenerateVersionInfo(PathHelper.AppResPath4Web);
			///
			//加载本地Version.txt 转换成Json配置文件存入 streamingVersionConfig
			using (UnityWebRequestAsync request = (UnityWebRequestAsync)Create(typeof(UnityWebRequestAsync),false))
			{
				await request.DownloadAsync(versionPath);
				streamingVersionConfig = JsonHelper.FromJson<VersionConfig>(request.Request.downloadHandler.text); //把从Request.downloadHandler下载的text转成Json
				Log.Debug(JsonHelper.ToJson(streamingVersionConfig));
			}

			// 删掉本地文件中,远程不存在的文件
			DirectoryInfo directory = new DirectoryInfo(PathHelper.AppHotfixResPath); //获取热更新获得的本地资源路径
			if (directory.Exists)
			{
				FileInfo[] fileInfos = directory.GetFiles();

				foreach (FileInfo fileInfo in fileInfos)
				{
					if (remoteVersionConfig.FileInfoDict.ContainsKey(fileInfo.Name)) //如果远程存在一样的文件名 就跳过
					{
						continue;
					}

					if (fileInfo.Name == "Version.txt") //Version.txt文件 也跳过
					{
						continue;
					}

					fileInfo.Delete(); //删掉多余的文件
				}


				DirectoryInfo[] directoryInfos = directory.GetDirectories();//子目录
				foreach (var directoryInfo in directoryInfos)
				{
					if (directoryInfo.Exists)
					{


						foreach (FileInfo fileInfo in directoryInfo.GetFiles())
						{


							if (remoteVersionConfig.FileInfoDict.ContainsKey(fileInfo.Name)) //如果远程存在一样的文件名 就跳过
							{
								continue;
							}

							if (fileInfo.Name == "Version.txt") //Version.txt文件 也跳过
							{
								continue;
							}

							fileInfo.Delete(); //删掉多余的文件
						}
					}
					else
					{
						directoryInfo.Create();  //如果不存在该目录 就创建该目录
					}
				}

			}
			else
			{
				directory.Create();  //如果不存在该目录 就创建该目录
			}


			// 对比MD5
			foreach (FileVersionInfo fileVersionInfo in remoteVersionConfig.FileInfoDict.Values)
			{
				if (fileVersionInfo.File == "Version.txt")
				{
					continue;
				}

				// 对比本地配置和远程配置中的md5值
				string localFileMD5 = BundleHelper.GetBundleMD5(streamingVersionConfig, fileVersionInfo.File);
				if (fileVersionInfo.MD5 == localFileMD5)
				{
					continue; //如果存在的文件 MD5值相同 就跳过
				}
				this.bundles.Enqueue(fileVersionInfo.File); //MD5值不同就 把该文件名加入bundles队列，准备更新
				this.TotalSize += fileVersionInfo.Size; //需要更新的文件总大小 
			}
		}

		public float Progress
		{
			get
			{
				if (this.TotalSize == 0)
				{
					return 0;
				}

				long alreadyDownloadBytes = 0;
				foreach (string downloadedBundle in this.downloadedBundles)
				{
					long size = this.remoteVersionConfig.FileInfoDict[downloadedBundle].Size;
					alreadyDownloadBytes += size;
				}
				if (this.webRequest != null)
				{
					alreadyDownloadBytes += (long)this.webRequest.Request.downloadedBytes;
				}
				return (alreadyDownloadBytes * 100f / this.TotalSize);
			}
		}

		/// <summary>
		/// 根据bundles 和downloadingBundle 下载需要更新得资源
		/// </summary>
		/// <returns></returns>
		public async Task DownloadAsync()
		{
			if (this.bundles.Count == 0 && this.downloadingBundle == "")
			{
				return;
			}

			try
			{
				while (true)
				{
					if (this.bundles.Count == 0)
					{
						break;
					}

					this.downloadingBundle = this.bundles.Dequeue(); //从bundles中出队

					while (true)
					{
						try
						{
							using (this.webRequest = (UnityWebRequestAsync)Create(typeof(UnityWebRequestAsync), false)) //创建UnityWebRequestAsync 对象
							{
								var ui = Game.Scene.GetComponent<UIComponent>().Get(UIType.UILoading);
								await this.webRequest.DownloadAsync(GlobalConfigComponent.Instance.GlobalProto.GetUrl() + "StreamingAssets/" + this.downloadingBundle, ui); //下载downloadingBundle名的bundle
								byte[] data = this.webRequest.Request.downloadHandler.data;

								string path = Path.Combine(PathHelper.AppHotfixResPath, this.downloadingBundle);
								//Log.Info(path);
								using (FileStream fs = new FileStream(path, FileMode.Create))
								{
									fs.Write(data, 0, data.Length);
								}
							}
						}
						catch (Exception e)
						{
							Log.Error($"download bundle error: {this.downloadingBundle}\n{e}");
							continue;
						}

						break;
					}
					this.downloadedBundles.Add(this.downloadingBundle);
					this.downloadingBundle = "";
					this.webRequest = null;
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
