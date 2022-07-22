using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts
{
    public enum GameStateEnum : byte { Invalid, Wait, AcceptInput, Run }

    /// <summary>
    /// Handles loading of the cell prefabs and materials.
    /// </summary>
    public static class Manager
    {    
        public static GameStateEnum GameState;
        public static GameObject CellPrefab;
        public static Material[] CellMaterials;

        /// <summary>
        /// Loads the prefabs and materials.
        /// </summary>
        /// <param name="isDeadCellInvisible">If dead cell is invisible == true.</param>
        /// <returns></returns>
        public static bool Initialize(bool isDeadCellInvisible)
        {
            var result = true;

            CellPrefab = Resources.Load<GameObject>("Prefabs/Cell");
            Assert.IsNotNull(CellPrefab, "Cell prefab not found");
            if (CellPrefab == null)
            {
                GameState = GameStateEnum.Invalid;
                Debug.LogError("Cell prefab not found");
                result = false;
            }

            if(isDeadCellInvisible == true)
            {
                CellMaterials = new[]
                {
                Resources.Load<Material>("Materials/Invisible"),
                Resources.Load<Material>("Materials/Alive")
                };
            }
            else
            {
                CellMaterials = new[]
                {
                Resources.Load<Material>("Materials/Dead"),
                Resources.Load<Material>("Materials/Alive")
                };
            }
           
            for (var i = 0; i < CellMaterials.Length; i++)
            {
                if (CellMaterials[i] != null) continue;
                GameState = GameStateEnum.Invalid;
                Debug.LogErrorFormat("Cell Material {0} not found", i);
                result = false;
                break;
            }

            return result;
        }
    }
}

