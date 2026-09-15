using CoreEngine;
using CoreEngine.Network.FishNetExtension;
using CoreEngine.Pool;
using UnityEngine;

namespace Icarus.Character.Test
{
    public class TestActor : BaseNetworkActor, IPoolable
    {
        public IPoolReleaser Releaser { get; set;  }

        protected override NetworkTickTarget networkTickTarget => NetworkTickTarget.None;
    }

}
