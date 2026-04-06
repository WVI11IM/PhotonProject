using System;
using Enemies.Leech;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;


[Serializable, GeneratePropertyBag]
[NodeDescription(name: "LeechDrainResource", story: "Drains resource from ship", category: "Action",
    id: "cdea190db9c9331095dbb06853574669")]
public partial class LeechDrainResourceAction : LeechAction {

    protected override Status OnStart() {
        //TODO: Send Photon message to Engineer to destroy the last item in queue
        return Status.Success;
    }

}