namespace ET
{
    public class AppStart_Init: AEvent<EventType.AppStart>
    {
        protected override async ETTask Run(EventType.AppStart args)
        {
            Game.Scene.AddComponent<TimerComponent>();
            Game.Scene.AddComponent<CoroutineLockComponent>();//协程锁，防止重入，多用户同时使用一个数据，需要加锁，排队队列执行

            // 加载配置
            Game.Scene.AddComponent<ConfigComponent>();
            await ConfigComponent.Instance.LoadAsync();
            
            Game.Scene.AddComponent<OpcodeTypeComponent>();//反射遍历加载消息
            Game.Scene.AddComponent<MessageDispatcherComponent>(); //注册收发消息 注册handlers
            Game.Scene.AddComponent<NetThreadComponent>();//添加网络线程组件 Services中存不同的net service类型（NetInner NetOuter）
            Game.Scene.AddComponent<ZoneSceneManagerComponent>();
            Game.Scene.AddComponent<AIDispatcherComponent>(); //注册 AIHandlers
            Game.Scene.AddComponent<RobotCaseComponent>(); //RobotCase实例组件
            Game.Scene.AddComponent<RobotCaseDispatcherComponent>(); //注册 RobotCaseAttribute 事件  ，
           
            
            var processScenes = StartSceneConfigCategory.Instance.GetByProcess(Game.Options.Process);
            foreach (StartSceneConfig startConfig in processScenes)
            {
                //根据配置表创建Robot专用的Scene
                await RobotSceneFactory.Create(Game.Scene, startConfig.Id, startConfig.InstanceId, startConfig.Zone, startConfig.Name, startConfig.Type, startConfig);
            }
            
            if (Game.Options.Console == 1)
            {
                Game.Scene.AddComponent<ConsoleComponent>(); //注册ConsoleHandler 事件， 可在控制台输入的命令。
            }

            //创建机器人 会执行一遍登录流程  并可以在RobotCaseSystem 中定义需要测试的内容 。
            //创建机器人时，会CreateZoneScene，并添加网络组件NetKcpComponent  AIComponent调用AI事件
        }
    }
}
