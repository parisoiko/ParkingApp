using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ParkingApp.Data;
//using Parking.Data;
using ParkingApp.Hubs;
using ParkingApp.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ParkingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ParkingDbContext _dbcontext;
        private readonly IHubContext<ParkingHub> _hub;

        public HomeController(ILogger<HomeController> logger, ParkingDbContext dbcontext, IHubContext<ParkingHub> hub) {
            _logger = logger;
            _dbcontext = dbcontext;
            _hub = hub;
        }

        public IActionResult Index() {
            return View();
        }

        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public JsonResult GetLicensePlate(SearchParameters parameters) {
            string trimmed = parameters.Query.Replace(" ", "");
            var vehicle = (from p in _dbcontext.ParkingVehicle
                           where p.Plate == trimmed
                           select p).SingleOrDefault();

            if (vehicle != null) {
                _hub.Clients.All.SendAsync("vehicleArrival", vehicle.ToDto());
            }
            else {
                var vehicleDto = new ParkingVehicleDto();
                vehicleDto.Plate = trimmed;
                _hub.Clients.All.SendAsync("vehicleArrival", vehicleDto);
            }
            return Json(vehicle);
        }
        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        public JsonResult AddVehicle([FromBody] ParkingVehicleDto data) {
            data.Plate = data.Plate.Replace(" ", "");
            var vh = (from p in _dbcontext.ParkingVehicle
                      where p.Plate == data.Plate
                      select p).FirstOrDefault();
            if (ModelState.IsValid) {
                if (vh == null) {

                    vh = data.ToParkingVehicle();
                    _dbcontext.Add(vh);
                }
                ParkingActivity activity = new ParkingActivity() {
                    Id = Guid.NewGuid(),
                    ParkingVehicle = vh,
                    FullName = vh.FullName,
                    VehicleId = vh.VehicleId.Value,
                    Plate = vh.Plate,
                    CheckIn = DateTime.Now,
                    CheckOut = null,
                    Status = enStatus.InProgress
                };
                _dbcontext.Add(activity);
                _dbcontext.SaveChanges();
                return Json(activity.ToDto());
                //_hub.Clients.All.SendAsync("newActivity", activity);
                //AddParkingActivity(vehicle, null);
            }
            return Json(data);
        }

        public JsonResult ReceiveData() {
            return _dbcontext.ParkingVehicle != null ?
                          Json(_dbcontext.ParkingVehicle.ToList()) :
                          Json("Entity set 'ParkingDbContext.ParkingVehicle'  is null.");
        }
    }



    
}