using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Gestiona la compra y aplicación de mejoras permanentes adquiridas en la tienda entre jornadas.
    /// Actualiza directamente los multiplicadores físicos en el PlayerController y GameManager.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [System.Serializable]
        public struct UpgradeTier
        {
            public int level;
            public int cost;
            public float modifierValue;
        }

        [Header("Configuraciones de Mejoras y Precios")]
        [SerializeField] private UpgradeTier[] bicycleSpeedTiers;  // Bicicleta Mejorada (Giro)
        [SerializeField] private UpgradeTier[] suspensionTiers;     // Mejor Suspensión (Estabilidad)
        [SerializeField] private UpgradeTier[] backpackTiers;       // Mochila Liviana (Peso)
        [SerializeField] private UpgradeTier[] extraTimeTiers;      // Tiempo Extra (Segundos)

        // Niveles actuales de compra
        private int currentBicycleLevel = 0;
        private int currentSuspensionLevel = 0;
        private int currentBackpackLevel = 0;
        private int currentExtraTimeLevel = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadUpgrades();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Aplica los modificadores actuales de las mejoras a las clases físicas/lógicas principales.
        /// </summary>
        public void ApplyUpgradesToGameplay(PlayerController player)
        {
            if (player != null)
            {
                // Bicicleta: Multiplicador de velocidad (Default: 1.0f)
                player.speedUpgradeFactor = GetModifierValue(bicycleSpeedTiers, currentBicycleLevel, 1f);

                // Suspensión: Factor reductor de amplitud de tambaleo (Default: 1.0f. Nivel máximo reduce la inestabilidad)
                player.suspensionUpgradeFactor = GetModifierValue(suspensionTiers, currentSuspensionLevel, 1f);

                // Mochila: Factor reductor de penalización de peso (Default: 1.0f. Menor factor implica menor pena)
                player.backpackUpgradeFactor = GetModifierValue(backpackTiers, currentBackpackLevel, 1f);
            }

            if (GameManager.Instance != null)
            {
                // Tiempo extra sumado en segundos a la jornada
                GameManager.Instance.extraTimeUpgrade = GetModifierValue(extraTimeTiers, currentExtraTimeLevel, 0f);
            }
        }

        #region Métodos de Compra
        public bool BuyUpgradeBicycleSpeed()
        {
            if (currentBicycleLevel >= bicycleSpeedTiers.Length) return false; // Nivel Max

            int cost = bicycleSpeedTiers[currentBicycleLevel].cost;
            if (GameManager.Instance != null && GameManager.Instance.SpendCoins(cost))
            {
                currentBicycleLevel++;
                SaveUpgrades();
                return true;
            }
            return false;
        }

        public bool BuyUpgradeSuspension()
        {
            if (currentSuspensionLevel >= suspensionTiers.Length) return false;

            int cost = suspensionTiers[currentSuspensionLevel].cost;
            if (GameManager.Instance != null && GameManager.Instance.SpendCoins(cost))
            {
                currentSuspensionLevel++;
                SaveUpgrades();
                return true;
            }
            return false;
        }

        public bool BuyUpgradeBackpack()
        {
            if (currentBackpackLevel >= backpackTiers.Length) return false;

            int cost = backpackTiers[currentBackpackLevel].cost;
            if (GameManager.Instance != null && GameManager.Instance.SpendCoins(cost))
            {
                currentBackpackLevel++;
                SaveUpgrades();
                return true;
            }
            return false;
        }

        public bool BuyUpgradeExtraTime()
        {
            if (currentExtraTimeLevel >= extraTimeTiers.Length) return false;

            int cost = extraTimeTiers[currentExtraTimeLevel].cost;
            if (GameManager.Instance != null && GameManager.Instance.SpendCoins(cost))
            {
                currentExtraTimeLevel++;
                SaveUpgrades();
                return true;
            }
            return false;
        }
        #endregion

        #region Helpers y Persistencia
        private float GetModifierValue(UpgradeTier[] tiers, int currentLevel, float defaultValue)
        {
            if (tiers == null || tiers.Length == 0 || currentLevel == 0) return defaultValue;
            
            int index = Mathf.Clamp(currentLevel - 1, 0, tiers.Length - 1);
            return tiers[index].modifierValue;
        }

        private void SaveUpgrades()
        {
            PlayerPrefs.SetInt("BicycleSpeedLvl", currentBicycleLevel);
            PlayerPrefs.SetInt("SuspensionLvl", currentSuspensionLevel);
            PlayerPrefs.SetInt("BackpackLvl", currentBackpackLevel);
            PlayerPrefs.SetInt("ExtraTimeLvl", currentExtraTimeLevel);
            PlayerPrefs.Save();
        }

        private void LoadUpgrades()
        {
            currentBicycleLevel = PlayerPrefs.GetInt("BicycleSpeedLvl", 0);
            currentSuspensionLevel = PlayerPrefs.GetInt("SuspensionLvl", 0);
            currentBackpackLevel = PlayerPrefs.GetInt("BackpackLvl", 0);
            currentExtraTimeLevel = PlayerPrefs.GetInt("ExtraTimeLvl", 0);
        }
        #endregion
    }
}
