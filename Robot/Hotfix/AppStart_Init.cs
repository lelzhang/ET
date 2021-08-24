namespace ET
{
    public class AppStart_Init: AEvent<EventType.AppStart>
    {
        protected override async ETTask Run(EventType.AppStart args)
        {
            Game.Scene.AddComponent<TimerComponent>();
            Game.Scene.AddComponent<CoroutineLockComponent>();

            // 加载配置
            Game.Scene.AddComponent<ConfigComponent>();
            await ConfigComponent.Instance.LoadAsync();
            
            Game.Scene.AddComponent<OpcodeTypeComponent>();//加载消息
            Game.Scene.AddComponent<MessageDispatcherComponent>(); //收发消息
            Game.Scene.AddComponent<NetThreadComponent>();//网络组件
            Game.Scene.AddComponent<ZoneSceneManagerComponent>();
            Game.Scene.AddComponent<AIDispatcherComponent>();
            Game.Scene.AddComponent<RobotCaseDispatcherComponent>();
            Game.Scene.AddComponent<RobotCaseComponent>();
            
            var processScenes = StartSceneConfigCategory.Instance.GetByProcess(Game.Options.Process);
            foreach (StartSceneConfig startConfig in processScenes)
            {
                await RobotSceneFactory.Create(Game.Scene, startConfig.Id, startConfig.InstanceId, startConfig.Zone, startConfig.Name, startConfig.Type, startConfig);
            }
            
            if (Game.Options.Console == 1)
            {
                Game.Scene.AddComponent<ConsoleComponent>();
            }
        }
    }
}
