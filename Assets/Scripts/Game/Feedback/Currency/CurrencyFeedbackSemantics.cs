using Game.Economy.Events;

namespace Game.Feedback.Currency
{
    /// <summary>
    /// Converts the existing typed economy event into presentation-only gain/spend semantics.
    /// </summary>
    public static class CurrencyFeedbackSemantics
    {
        public static CurrencyFeedbackKind Classify(CurrencyBalanceChangedEventData data)
        {
            if (data.AmountChanged > 0)
            {
                return CurrencyFeedbackKind.Gain;
            }

            if (data.AmountChanged < 0)
            {
                return CurrencyFeedbackKind.Spend;
            }

            return CurrencyFeedbackKind.None;
        }
    }
}
