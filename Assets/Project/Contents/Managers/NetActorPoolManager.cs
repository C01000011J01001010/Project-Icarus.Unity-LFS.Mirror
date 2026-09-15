using CoreEngine;
using CoreEngine.Manager;
using CoreEngine.Network.FishNetExtension.Pool;

namespace Icarus.Pool
{
    public enum NetActorPoolType
    {
        SharedActor,
        TestActor = 9999,
    }
    public class NetActorPoolManager : BaseNetObjectPoolManager<NetActorPoolType>, IPriority
    {
        public int Priority => (int)ManagerPriority.Infrastructure;
    }
}
