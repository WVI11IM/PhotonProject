using System;
using Enemies.Leech;
using Pilot;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "LeechFindResource", story: "Finds [find_target] and set it as [set_target]",
    category: "Action/Find", id: "6658a2b7ba1570da3dd66915173566f3")]
public partial class LeechFindResourceAction : LeechAction {

    public enum FindTarget {

        Ship,
        NearestItem,

    }

    [SerializeReference] public BlackboardVariable<FindTarget> find_target;
    [SerializeReference] public BlackboardVariable<Transform> set_target;

    protected override Status OnStart() {
        switch (find_target.Value) {
            case FindTarget.NearestItem:
                GameObject[] items = GameObject.FindGameObjectsWithTag("Item");

                // If no items exist in the scene, return failure.
                if (items.Length == 0)
                    return Status.Failure;

                float nearestCandidateDist =
                    Vector2.Distance(Leech.transform.position, items[0].transform.position);

                GameObject nearestCandidate = items[0];

                foreach (var item in items)
                    if (Vector2.Distance(Leech.transform.position, item.transform.position) <
                        nearestCandidateDist) {
                        nearestCandidateDist = Vector2.Distance(Leech.transform.position, item.transform.position);
                        nearestCandidate = item;
                    }
                Leech.targetItem = nearestCandidate.GetComponent<ResourceItem>();
                set_target.Value = nearestCandidate.transform;
                break;

            case FindTarget.Ship:
                set_target.Value = ShipCore.Instance.transform;
                break;

            default:
                Debug.LogError("Unknown FindTarget mode");
                return Status.Failure;
        }

        return Status.Success;
    }

}