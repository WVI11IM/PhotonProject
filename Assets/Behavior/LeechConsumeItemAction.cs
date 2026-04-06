using System;
using Enemies.Leech;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "LeechConsumeItem", story: "Consumes the target item", category: "Action",
    id: "597c41457ffec2b50dff75603685f791")]
public partial class LeechConsumeItemAction : LeechAction {

    protected override Status OnStart() {
        Leech.targetItem.LeechConsume();
        return Status.Success;
    }

}