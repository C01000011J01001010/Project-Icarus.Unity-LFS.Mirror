using CoreEngine.DesignPattern.StateMachine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Icarus.Character.State
{
    public enum CharacterState
    {
        // 육지에 있을 때
        Ground = 0, 
        
        // 공중에 떠 있을 때
        Air = 10,

        // 물에 떠 있을 때
        Water = 20,
    }
    public class CharacterStateManager : BaseStateManager<CharacterState, StateControlFeature>
    {
        protected override void Awake()
        {
            base.Awake();
        }
        protected override void SetUpStates()
        {
            AddState(CharacterState.Ground, new Ground());
            AddState(CharacterState.Air, new Air());
            AddState(CharacterState.Water, new Air());
        }
    }
}
