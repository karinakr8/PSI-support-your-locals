using SupportYourLocals.Data;

namespace SupportYourLocals.WebAPI.Controllers
{
    public class SellerDataController : GenericDataController<SellerData>
    {
        public SellerDataController(IDataStorage<SellerData> storage) : base(storage) { }
    }

    public class MarketplaceDataController : GenericDataController<MarketplaceData>
    {
        public MarketplaceDataController(IDataStorage<MarketplaceData> storage) : base(storage) { }
    }

    public class UserDataController : GenericDataController<UserData>
    {
        public UserDataController(IDataStorage<UserData> storage) : base(storage) { }
    }
}
