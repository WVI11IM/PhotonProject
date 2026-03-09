using UnityEngine;

namespace Pilot {
    
    public class ShipStats : MonoBehaviour {

        [Header("Resources (Max)")]
        [field:SerializeField] public float  ResMaxFuel { get; private set; }
        [field:SerializeField] public int    ResMaxAmmo { get; private set; }
        [field:SerializeField] public int    ResMaxHp   { get; private set; } 
        
        [Header("Resources (Current)")]
        [field:SerializeField] public float  ResFuel    { get; private set; }
        [field:SerializeField] public int    ResAmmo    { get; private set; }
        [field:SerializeField] public int    ResHp      { get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            ResFuel = ResMaxFuel;
            ResAmmo = ResMaxAmmo;
            ResHp = ResMaxHp;
        }

        // Update is called once per frame
        void Update() { }

        /// <summary>
        /// Subtract <paramref name="amount"/> from the ammo count, returning however much ammo exceeds the current ammo count.
        /// </summary>
        /// <param name="amount">How much ammo to consume.</param>
        /// <returns>The difference between <paramref name="amount"/> and the current ammo count, if less than zero.</returns>
        public int ConsumeAmmo(int amount) {
            int diff = -Mathf.Min(0, ResAmmo - amount);
            ResAmmo = Mathf.Max(0, ResAmmo - amount);
            return diff;
        }

    }

}