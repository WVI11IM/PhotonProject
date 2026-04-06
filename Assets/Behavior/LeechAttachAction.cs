using System;
using Enemies.Leech;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "LeechAttach", story: "[attach_mode] [attach_target]", category: "Action",
    id: "00732bb793caf83705295aac6d3406f7")]
public partial class LeechAttachAction : LeechAction {

    [SerializeReference] public BlackboardVariable<Transform> attach_target;

    public enum AttachActionMode {

        AttachTo,
        DetachFrom

    }

    [SerializeReference] public BlackboardVariable<AttachActionMode> attach_mode;

    protected override Status OnStart() {
        if (attach_mode.Value == AttachActionMode.AttachTo && attach_target.Value == null) {
            Debug.LogError("Attempted to attach, but target was null", Parent.GameObject);
            return Status.Failure;
        }

        switch (attach_mode.Value) {
            case AttachActionMode.AttachTo:
                Leech.transform.parent = attach_target.Value;

                //TODO: Make the leech point at the target
                break;

            case AttachActionMode.DetachFrom:
                Leech.transform.parent = null;
                break;
        }

        return Status.Success;
    }

}