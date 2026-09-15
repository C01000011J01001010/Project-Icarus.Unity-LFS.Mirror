using CoreEngine.Actor;
using UnityEngine;

namespace Icarus.Character.State
{
    public class Air : BaseState
    {
        public override CharacterState? CheckTransitions(IActorHost host)
        {
            return null;
        }

        public override void Enter(IActorHost host)
        {

        }

        public override void Exit(IActorHost host, CharacterState? nextState)
        {

        }
    }
}

