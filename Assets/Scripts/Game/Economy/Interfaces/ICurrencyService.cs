namespace Game.Economy.Interfaces
{
    /// <summary>
    /// Service for reading and modifying player currency balances.
    /// </summary>
    public interface ICurrencyService
    {
        /// <summary>
        /// Gets the current balance for the given currency.
        /// </summary>
        /// <param name="currencyId">Unique identifier of the currency type.</param>
        /// <returns>Current balance, or 0 if unknown.</returns>
        int GetBalance(string currencyId);

        /// <summary>
        /// Adds amount to the given currency balance.
        /// </summary>
        /// <param name="currencyId">Unique identifier of the currency type.</param>
        /// <param name="amount">Amount to add (must be positive).</param>
        void AddBalance(string currencyId, int amount);

        /// <summary>
        /// Attempts to spend the given amount. Balance is reduced only if sufficient.
        /// </summary>
        /// <param name="currencyId">Unique identifier of the currency type.</param>
        /// <param name="amount">Amount to spend (must be positive).</param>
        /// <returns>True if spent, false if insufficient balance.</returns>
        bool TrySpend(string currencyId, int amount);

        /// <summary>
        /// Returns whether the player can afford the given amount.
        /// </summary>
        /// <param name="currencyId">Unique identifier of the currency type.</param>
        /// <param name="amount">Amount to check.</param>
        /// <returns>True if balance is at least amount.</returns>
        bool CanAfford(string currencyId, int amount);
    }
}
