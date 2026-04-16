using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Delivery.Model;
using BookingBoardgamesILoveBan.Src.Delivery.Model.Validators;
using BookingBoardgamesILoveBan.Src.Delivery.Service.MapServices;
using BookingBoardgamesILoveBan.Src.Delivery.ViewModel;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;

namespace BookingBoardgamesLoveBan.Tests.Delivery
{
    public class DeliveryViewModelTests // TODO i need mock interfaces
    {
        //private readonly FakeMapService fakeMapService;
        //private readonly FakeValidator fakeValidator;
        //private readonly DeliveryViewModel viewModel;

        //public DeliveryViewModelTests()
        //{
        //    fakeMapService = new FakeMapService();
        //    fakeValidator = new FakeValidator();

        //    viewModel = new DeliveryViewModel(
        //        currentUserId: 1,
        //        mapService: fakeMapService,
        //        userService: new FakeUserService(),
        //        validator: fakeValidator
        //    );
        //}

        //[Fact]
        //public void OpenMap_SetsIsMapVisibleTrue()
        //{
        //    viewModel.OpenMap();

        //    Assert.True(viewModel.IsMapVisible);
        //}
    }
}

// Fake implementations - put these at the bottom of your test file

//public class FakeUserService : UserService
//{

//    public override User GetById(int id)
//    {
//        return new User(id, "Name", "Romania", "Cluj-Napoca", "Teodor Mihali", "58" );
//    }
//}

//public class FakeMapService : IMapService
//{
//    public Address AddressToReturn { get; set; } = null;

//    public Task<Address> GetAddressFromMapAsync(double latitude, double longitude)
//    {
//        return Task.FromResult(AddressToReturn);
//    }
//}

//public class FakeValidator : IValidator<Dictionary<string, string>, Address>
//{
//    public Dictionary<string, string> ErrorsToReturn { get; set; } = new();

//    public Dictionary<string, string> Validate(Address address)
//    {
//        return ErrorsToReturn;
//    }
//}