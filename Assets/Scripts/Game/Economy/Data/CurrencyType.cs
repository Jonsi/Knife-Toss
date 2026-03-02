namespace Game.Economy.Data
{
    /// <summary>
    /// Store-facing currency enum. Converted to string IDs only at the economy service boundary.
    /// </summary>
    public enum CurrencyType
    {
        None = 0,
        Coin = 1,
        Orange = 2
    }

    /// <summary>
    /// Mapping helpers between enum values and persisted economy type IDs.
    /// </summary>
    public static class CurrencyTypeExtensions
    {
        public static string ToTypeId(this CurrencyType currencyType)
        {
            switch (currencyType)
            {
                case CurrencyType.Coin:
                    return "tomato_currency";
                case CurrencyType.Orange:
                    return "orange_currency";
                default:
                    return string.Empty;
            }
        }

        public static CurrencyType FromTypeId(string typeId)
        {
            switch (typeId)
            {
                case "tomato_currency":
                    return CurrencyType.Coin;
                case "orange_currency":
                    return CurrencyType.Orange;
                default:
                    return CurrencyType.None;
            }
        }

        public static string ToDisplayLabel(this CurrencyType currencyType)
        {
            switch (currencyType)
            {
                case CurrencyType.Coin:
                    return "COIN";
                case CurrencyType.Orange:
                    return "ORANGE";
                default:
                    return string.Empty;
            }
        }
    }
}
