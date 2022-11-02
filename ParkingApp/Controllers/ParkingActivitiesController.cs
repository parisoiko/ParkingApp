using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PagedList;
using ParkingApp.Data;
using ParkingApp.Hubs;
using ParkingApp.Models;

namespace ParkingApp.Controllers
{
    public class ParkingActivitiesController : Controller
    {
        private readonly ParkingDbContext _context;
        private readonly IHubContext<ParkingHub> _hub;

        public ParkingActivitiesController(ParkingDbContext context, IHubContext<ParkingHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        // GET: ParkingActivities
        public async Task<IActionResult> Index()
        {
            var activities = from p in _context.ParkingActivity
                             where p.CheckOut == null
                           select p;
            activities = activities.OrderBy(p => p.CheckIn);
            int totalPages = (activities.Count() + 22 - 1) / 22;
            int pageNumber = 1;
            var retVal = new PagedData<ParkingActivity>() { Data = activities.ToPagedList(pageNumber, 22).ToList(), TotalPages = totalPages };
            return View(retVal);
        }

        public async Task<IActionResult> Completed() {
            var completedList = from p in _context.ParkingActivity
                                where p.Status == enStatus.Completed
                                orderby p.CheckOut.Value descending
                                select p;
            int totalPages = (completedList.Count() + 22 - 1) / 22;
            int pageNumber = 1;
            var retVal = new PagedData<ParkingActivity>() { Data = completedList.ToPagedList(pageNumber, 22).ToList(), TotalPages = totalPages };
            return View(retVal);
        }
        [HttpPost]
        public JsonResult CompleteActivity([FromBody] Guid Id) {
            var pA = _context.ParkingActivity.FirstOrDefault(m => m.Id == Id);
            if (pA != null) {
                pA.CheckOut = DateTime.Now;
                pA.Status = enStatus.Completed;
                _context.SaveChangesAsync();
                _hub.Clients.All.SendAsync("removeActivity", pA.ToDto());
            }
            return Json(pA);
        }
        //public async Task<IActionResult> CompleteActivity(Guid? Id) {
        //    if (Id == null || _context.ParkingActivity == null) {
        //        return NotFound();
        //    }

        //    var parkingActivity = await _context.ParkingActivity
        //        .FirstOrDefaultAsync(m => m.Id == Id);
        //    if (parkingActivity != null) {
        //        parkingActivity.CheckOut = DateTime.Now;
        //        parkingActivity.Status = enStatus.Completed;
        //        await _context.SaveChangesAsync();
        //        await _hub.Clients.All.SendAsync("removeActivity", parkingActivity.ToDto());
        //    }
        //    return RedirectToAction(nameof(Index));
        //}

        // GET: ParkingActivities/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.ParkingActivity == null)
            {
                return NotFound();
            }

            var parkingActivity = await _context.ParkingActivity
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkingActivity == null)
            {
                return NotFound();
            }

            return View(parkingActivity);
        }

        // GET: ParkingActivities/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ParkingActivities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CheckIn,CheckOut,VehicleId,Name,Plate,Status,RowVersion")] ParkingActivity parkingActivity)
        {
            if (ModelState.IsValid)
            {
                parkingActivity.Id = Guid.NewGuid();
                _context.Add(parkingActivity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(parkingActivity);
        }

        // GET: ParkingActivities/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.ParkingActivity == null)
            {
                return NotFound();
            }

            var parkingActivity = await _context.ParkingActivity.FindAsync(id);
            if (parkingActivity == null)
            {
                return NotFound();
            }
            return View(parkingActivity);
        }

        // POST: ParkingActivities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,CheckIn,CheckOut,VehicleId,Name,Plate,Status,RowVersion")] ParkingActivity parkingActivity)
        {
            if (id != parkingActivity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(parkingActivity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkingActivityExists(parkingActivity.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(parkingActivity);
        }

        // GET: ParkingActivities/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.ParkingActivity == null)
            {
                return NotFound();
            }

            var parkingActivity = await _context.ParkingActivity
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkingActivity == null)
            {
                return NotFound();
            }

            return View(parkingActivity);
        }

        // POST: ParkingActivities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.ParkingActivity == null)
            {
                return Problem("Entity set 'ParkingDbContext.ParkingActivity'  is null.");
            }
            var parkingActivity = await _context.ParkingActivity.FindAsync(id);
            if (parkingActivity != null)
            {
                _context.ParkingActivity.Remove(parkingActivity);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParkingActivityExists(Guid id)
        {
          return (_context.ParkingActivity?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public JsonResult SearchActivities(SearchActivityParameters parameters) {
            DateTime StartDateCheckIn = DateTime.MinValue;
            DateTime EndDateCheckIn = DateTime.MaxValue;
            DateTime StartDateCheckOut = DateTime.MinValue;
            DateTime EndDateCheckOut = DateTime.MaxValue;
            if (parameters.Query != null) {
                string[] dates = parameters.Query.Split(" - ");
                StartDateCheckIn = DateTime.Parse(dates[0]);
                EndDateCheckIn = DateTime.Parse(dates[1] + " 23:59:59");
            }
            if(parameters.CheckOutQuery != null) {
                string[] dates = parameters.CheckOutQuery.Split(" - ");
                StartDateCheckOut = DateTime.Parse(dates[0]);
                EndDateCheckOut = DateTime.Parse(dates[1] + " 23:59:59");
            }
            parameters.PlateQuery = parameters.PlateQuery == null ? "" : parameters.PlateQuery;
            parameters.CheckOutQuery = parameters.CheckOutQuery == null ? "" : parameters.CheckOutQuery;
            parameters.FullNameQuery = parameters.FullNameQuery == null ? "" : parameters.FullNameQuery;
            var activities = from p in _context.ParkingActivity
                                 where p.Status == parameters.InProgressCompleted
                             select p;
            if (parameters.InProgressCompleted == enStatus.InProgress) {
                activities = activities.Where(p => DateTime.Compare(p.CheckIn, StartDateCheckIn) > 0 && DateTime.Compare(p.CheckIn, EndDateCheckIn) < 0
                                                && p.Plate.ToLower().Contains(parameters.PlateQuery.ToLower())
                                                && p.FullName.ToLower().Contains(parameters.FullNameQuery.ToLower())
                                                ).OrderBy(p => p.CheckIn);
            }
            else {
                activities = activities.Where(p => DateTime.Compare(p.CheckIn, StartDateCheckIn) > 0 && DateTime.Compare(p.CheckIn, EndDateCheckIn) < 0
                                                && DateTime.Compare((DateTime)p.CheckOut, StartDateCheckOut) > 0 && DateTime.Compare((DateTime)p.CheckOut, EndDateCheckOut) < 0
                                                && p.Plate.ToLower().Contains(parameters.PlateQuery.ToLower())
                                                && p.FullName.ToLower().Contains(parameters.FullNameQuery.ToLower())
                                                ).OrderByDescending(p => p.CheckOut);
            }
            int totalPages = (activities.Count() + parameters.PageSize - 1) / parameters.PageSize;
            int pageNumber = (parameters.PageNumber ?? 1);
            var retVal = new PagedData<ParkingActivity>() { Data = activities.ToPagedList(pageNumber, parameters.PageSize).ToList(), TotalPages = totalPages };
            return Json(retVal);
        }
    }
}
