using UnityEngine;

namespace Icarus.Character.State
{
    public class Water : BaseState
    {
        public override CharacterState? CheckTransitions(StateControlFeature controller)
        {
            return null;
        }

        public override void Enter(StateControlFeature controller)
        {
        }

        public override void Exit(StateControlFeature controller, CharacterState? nextState)
        {
        }
    }
}

