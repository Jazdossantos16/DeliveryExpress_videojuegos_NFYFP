using UnityEngine;

namespace DeliveryExpress
{
    /// <summary>
    /// Gestiona la compra y aplicación de mejoras permanentes adquiridas en la tienda entre jornadas.
    /// Actualiza directamente los multiplicadores físicos en el ControladorJugador y AdministradorJuego.
    /// </summary>
    public class AdministradorMejoras : MonoBehaviour
    {
        public static AdministradorMejoras Instance { get; private set; }

        // Niveles actuales de compra (0 = no comprado, 1 = comprado)
        private int currentBicycleLevel = 0;   // Moto de reparto
        private int currentSuspensionLevel = 0; // Casco protector
        private int currentBackpackLevel = 0;   // Mochila Pro
        private int currentExtraLivesLevel = 0; // Vidas Extra
        private int currentPowerUpLevel = 0;    // Power up

        // Estados de equipamiento (true = equipado, false = desequipado)
        private bool isBicycleEquipped = true;
        private bool isSuspensionEquipped = true;
        private bool isBackpackEquipped = true;
        private bool isExtraLivesEquipped = true;
        private bool isPowerUpEquipped = true;

        // Métodos de consulta de nivel
        public int GetBicycleLevel() => currentBicycleLevel;
        public int GetSuspensionLevel() => currentSuspensionLevel;
        public int GetBackpackLevel() => currentBackpackLevel;
        public int GetExtraLivesLevel() => currentExtraLivesLevel;
        public int GetPowerUpLevel() => currentPowerUpLevel;

        // Métodos de consulta de equipamiento
        public bool IsBicycleEquipped() => isBicycleEquipped;
        public bool IsSuspensionEquipped() => isSuspensionEquipped;
        public bool IsBackpackEquipped() => isBackpackEquipped;
        public bool IsExtraLivesEquipped() => isExtraLivesEquipped;
        public bool IsPowerUpEquipped() => isPowerUpEquipped;

        // Métodos para equipar/desequipar (toggles)
        public void ToggleBicycleEquipped() { if (currentBicycleLevel > 0) { isBicycleEquipped = !isBicycleEquipped; SaveUpgrades(); } }
        public void ToggleSuspensionEquipped() { if (currentSuspensionLevel > 0) { isSuspensionEquipped = !isSuspensionEquipped; SaveUpgrades(); } }
        public void ToggleBackpackEquipped() { if (currentBackpackLevel > 0) { isBackpackEquipped = !isBackpackEquipped; SaveUpgrades(); } }
        public void ToggleExtraLivesEquipped() { if (currentExtraLivesLevel > 0) { isExtraLivesEquipped = !isExtraLivesEquipped; SaveUpgrades(); } }
        public void TogglePowerUpEquipped() { if (currentPowerUpLevel > 0) { isPowerUpEquipped = !isPowerUpEquipped; SaveUpgrades(); } }

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
        public void ApplyUpgradesToGameplay(ControladorJugador player)
        {
            if (player != null)
            {
                // Moto de reparto: Velocidad lateral incrementada (Default: 1.0f -> 1.35f)
                player.speedUpgradeFactor = (currentBicycleLevel > 0 && isBicycleEquipped) ? 1.35f : 1f;

                // Casco protector: Estabilidad mejorada, reduce wobble (Default: 1.0f -> 0.3f)
                player.suspensionUpgradeFactor = (currentSuspensionLevel > 0 && isSuspensionEquipped) ? 0.3f : 1f;

                // Mochila Pro: Peso liviano, reduce penalización (Default: 1.0f -> 0.5f)
                player.backpackUpgradeFactor = (currentBackpackLevel > 0 && isBackpackEquipped) ? 0.5f : 1f;

                // Power up: Aumenta la duración de la energía (Default: 1.0f -> 1.5f)
                player.powerUpDurationFactor = (currentPowerUpLevel > 0 && isPowerUpEquipped) ? 1.5f : 1f;
            }

            if (AdministradorJuego.Instance != null)
            {
                // Vidas Extra: Añade 1 vida extra de inicio
                AdministradorJuego.Instance.extraLivesUpgrade = (currentExtraLivesLevel > 0 && isExtraLivesEquipped) ? 1 : 0;
            }
        }

        #region Métodos de Compra
        public bool BuyUpgradeBicycleSpeed()
        {
            if (currentBicycleLevel >= 1) return false; // Ya comprado

            int cost = 1000;
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.SpendCoins(cost))
            {
                currentBicycleLevel = 1;
                SaveUpgrades();
                return true;
            }
            return false;
        }

        public bool BuyUpgradeSuspension()
        {
            if (currentSuspensionLevel >= 1) return false; // Ya comprado

            int cost = 300;
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.SpendCoins(cost))
            {
                currentSuspensionLevel = 1;
                SaveUpgrades();
                return true;
            }
            return false;
        }

        public bool BuyUpgradeBackpack()
        {
            if (currentBackpackLevel >= 1) return false; // Ya comprado

            int cost = 100;
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.SpendCoins(cost))
            {
                currentBackpackLevel = 1;
                SaveUpgrades();
                return true;
            }
            return false;
        }

        public bool BuyUpgradeExtraLives()
        {
            if (currentExtraLivesLevel >= 1) return false; // Ya comprado

            int cost = 1500;
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.SpendCoins(cost))
            {
                currentExtraLivesLevel = 1;
                SaveUpgrades();
                return true;
            }
            return false;
        }

        public bool BuyUpgradePowerUp()
        {
            if (currentPowerUpLevel >= 1) return false; // Ya comprado

            int cost = 2000;
            if (AdministradorJuego.Instance != null && AdministradorJuego.Instance.SpendCoins(cost))
            {
                currentPowerUpLevel = 1;
                SaveUpgrades();
                return true;
            }
            return false;
        }

        // Mantener compatibilidad si hay referencias viejas
        public bool BuyUpgradeExtraTime()
        {
            return false;
        }
        #endregion

        #region Persistencia
        private string GetCurrentPlayerName()
        {
            return PlayerPrefs.GetString("PlayerName", "").Trim();
        }

        public void SaveUpgrades()
        {
            string playerName = GetCurrentPlayerName();
            if (string.IsNullOrEmpty(playerName)) return;
            PlayerPrefs.SetInt(playerName + "_BicycleSpeedLvl", currentBicycleLevel);
            PlayerPrefs.SetInt(playerName + "_SuspensionLvl", currentSuspensionLevel);
            PlayerPrefs.SetInt(playerName + "_BackpackLvl", currentBackpackLevel);
            PlayerPrefs.SetInt(playerName + "_ExtraLivesLvl", currentExtraLivesLevel);
            PlayerPrefs.SetInt(playerName + "_PowerUpUpgradeLvl", currentPowerUpLevel);
            
            PlayerPrefs.SetInt(playerName + "_BicycleSpeedEquipped", isBicycleEquipped ? 1 : 0);
            PlayerPrefs.SetInt(playerName + "_SuspensionEquipped", isSuspensionEquipped ? 1 : 0);
            PlayerPrefs.SetInt(playerName + "_BackpackEquipped", isBackpackEquipped ? 1 : 0);
            PlayerPrefs.SetInt(playerName + "_ExtraLivesEquipped", isExtraLivesEquipped ? 1 : 0);
            PlayerPrefs.SetInt(playerName + "_PowerUpUpgradeEquipped", isPowerUpEquipped ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void LoadUpgrades()
        {
            string playerName = GetCurrentPlayerName();
            if (string.IsNullOrEmpty(playerName))
            {
                currentBicycleLevel = 0;
                currentSuspensionLevel = 0;
                currentBackpackLevel = 0;
                currentExtraLivesLevel = 0;
                currentPowerUpLevel = 0;
                
                isBicycleEquipped = true;
                isSuspensionEquipped = true;
                isBackpackEquipped = true;
                isExtraLivesEquipped = true;
                isPowerUpEquipped = true;
                return;
            }
            currentBicycleLevel = PlayerPrefs.GetInt(playerName + "_BicycleSpeedLvl", 0);
            currentSuspensionLevel = PlayerPrefs.GetInt(playerName + "_SuspensionLvl", 0);
            currentBackpackLevel = PlayerPrefs.GetInt(playerName + "_BackpackLvl", 0);
            currentExtraLivesLevel = PlayerPrefs.GetInt(playerName + "_ExtraLivesLvl", 0);
            currentPowerUpLevel = PlayerPrefs.GetInt(playerName + "_PowerUpUpgradeLvl", 0);
            
            isBicycleEquipped = PlayerPrefs.GetInt(playerName + "_BicycleSpeedEquipped", 1) == 1;
            isSuspensionEquipped = PlayerPrefs.GetInt(playerName + "_SuspensionEquipped", 1) == 1;
            isBackpackEquipped = PlayerPrefs.GetInt(playerName + "_BackpackEquipped", 1) == 1;
            isExtraLivesEquipped = PlayerPrefs.GetInt(playerName + "_ExtraLivesEquipped", 1) == 1;
            isPowerUpEquipped = PlayerPrefs.GetInt(playerName + "_PowerUpUpgradeEquipped", 1) == 1;
        }
        #endregion
    }
}
