namespace Common.IAP.Data
{
    /// <summary>
    /// Runtime presentation information for an IAP product.
    /// </summary>
    public readonly struct IapProductInfo
    {
        public IapProduct Product { get; }
        public bool IsAvailable { get; }
        public bool IsOwned { get; }
        public string LocalizedPrice { get; }

        public IapProductInfo(IapProduct product, bool isAvailable, bool isOwned, string localizedPrice)
        {
            Product = product;
            IsAvailable = isAvailable;
            IsOwned = isOwned;
            LocalizedPrice = localizedPrice;
        }
    }
}
