﻿using System;
using GeneralUtils;
using UnityEngine;
using UnityEngine.AI;

namespace _Game.Scripts.Objects.Pedestrian {
    public class WalkState : BasePedestrianState {
        private readonly Func<Vector3> _getNextPosition;

        public WalkState(NavMeshAgent agent, UpdatedValue<float> speed, Func<Vector3> getNextPosition) : base(agent, speed) {
            _getNextPosition = getNextPosition;
        }

        protected override void PerformStateEnter() {
            Agent.speed = Speed.Value;
        }

        public override void OnStatePersist(float deltaTime) {
            if (ShouldEvade()) {
                SwitchState(PedestrianState.Evade);
                return;
            }

            if (ShouldBeware()) {
                SwitchState(PedestrianState.Beware);
                return;
            }

            if (Arrived()) {
                Agent.SetDestination(_getNextPosition());
            }
        }

        protected override void OnSpeedChanged(float newSpeed) {
            Agent.speed = newSpeed;
        }
    }
}