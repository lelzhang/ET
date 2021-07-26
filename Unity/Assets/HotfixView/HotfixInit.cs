using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    class HotfixInit
    {
        public static void Start()
        {
            // 注册热更层回调
            ET.Game.Hotfix.Update = () => { Update(); };
            ET.Game.Hotfix.LateUpdate = () => { LateUpdate(); };
            ET.Game.Hotfix.FixedUpdate = () => { FixedUpdate(); };
            ET.Game.Hotfix.OnApplicationQuit = () => { OnApplicationQuit(); };

            Game.EventSystem.Publish(new EventType.AppStart());
        }

		public static void Update()
		{
			try
			{
				Game.EventSystem.Update();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public static void LateUpdate()
		{
			try
			{
				Game.EventSystem.LateUpdate();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
        public static void FixedUpdate()
        {
            try
            {
                //Game.EventSystem.FixedUpdate();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static void OnApplicationQuit()
		{
			Game.Close();
		}
	}
}

