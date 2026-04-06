using System;
using Enemies.Leech;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "LeechNavToTarget", story: "Applies [force] force towards [nav_target]",
    category: "Action/Navigation", id: "3c1ff7ff1aa4208b45b8eb9bb6fe8d94")]
public partial class LeechNavToTargetAction : LeechAction {

    [SerializeReference] public BlackboardVariable<float> force;
    [SerializeReference] public BlackboardVariable<Transform> nav_target;

    protected override Status OnStart() {
        Leech.JerkTowards(nav_target.Value.position, force.Value);
        return Status.Running;
    }

}