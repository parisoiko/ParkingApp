using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList;
using ParkingApp.Data;
using ParkingApp.Models;

namespace ParkingApp.Controllers
{
    public class ParkingVehiclesController : Controller
    {
        private readonly ParkingDbContext _context;

        public ParkingVehiclesController(ParkingDbContext context) {
            _context = context;
        }

        // GET: ParkingVehicles
        public async Task<IActionResult> Index() {
            var vehicles = from p in _context.ParkingVehicle
                           select p;
            vehicles = vehicles.OrderBy(p => p.FullName);
            int totalPages = (vehicles.Count() + 22 - 1) / 22;
            int pageNumber = 1;
            var retVal = new PagedData<ParkingVehicle>() { Data = vehicles.ToPagedList(pageNumber, 22).ToList(), TotalPages = totalPages };
            return View(retVal);
            //return _context.ParkingVehicle != null ?
            //              View(await _context.ParkingVehicle.OrderBy(v => v.FullName).ToListAsync()) :
            //              Problem("Entity set 'ParkingDbContext.ParkingVehicle'  is null.");
        }

        // GET: ParkingVehicles/Details/5
        public async Task<IActionResult> Details(Guid? id) {
            if (id == null || _context.ParkingVehicle == null) {
                return NotFound();
            }

            var parkingVehicle = await _context.ParkingVehicle
                .FirstOrDefaultAsync(m => m.VehicleId == id);
            if (parkingVehicle == null) {
                return NotFound();
            }

            return View(parkingVehicle);
        }

        // GET: ParkingVehicles/Create
        public IActionResult Create() {
            return View();
        }

        // POST: ParkingVehicles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VehicleId,Name,Plate,FleetOrVisitor")] ParkingVehicle parkingVehicle) {
            if (ModelState.IsValid) {
                parkingVehicle.VehicleId = Guid.NewGuid();
                _context.Add(parkingVehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(parkingVehicle);
        }

        // GET: ParkingVehicles/Edit/5
        public async Task<IActionResult> Edit(Guid? id) {
            if (id == null || _context.ParkingVehicle == null) {
                return NotFound();
            }

            var parkingVehicle = await _context.ParkingVehicle.FindAsync(id);
            if (parkingVehicle == null) {
                return NotFound();
            }
            return View(parkingVehicle);
        }
        [HttpPost]
        public JsonResult EditVehicle([FromBody] ParkingVehicle parkingVehicle) {
            var dbproduct = _context.ParkingVehicle.Find(parkingVehicle.VehicleId);
            if (dbproduct == null) {
                return Json(NotFound());
            }

            try {
                dbproduct.Update(parkingVehicle);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex) {
                throw ex;
            }
            return Json(dbproduct);
        }

        // POST: ParkingVehicles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("VehicleId,Name,Plate,FleetOrVisitor")] ParkingVehicle parkingVehicle) {
            if (id != parkingVehicle.VehicleId) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(parkingVehicle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!ParkingVehicleExists((Guid)parkingVehicle.VehicleId)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(parkingVehicle);
        }

        // GET: ParkingVehicles/Delete/5
        public async Task<IActionResult> Delete(Guid? id) {
            if (id == null || _context.ParkingVehicle == null) {
                return NotFound();
            }

            var parkingVehicle = await _context.ParkingVehicle
                .FirstOrDefaultAsync(m => m.VehicleId == id);
            if (parkingVehicle == null) {
                return NotFound();
            }

            return View(parkingVehicle);
        }

        // POST: ParkingVehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id) {
            if (_context.ParkingVehicle == null) {
                return Problem("Entity set 'ParkingDbContext.ParkingVehicle'  is null.");
            }
            var parkingVehicle = await _context.ParkingVehicle.FindAsync(id);
            if (parkingVehicle != null) {
                _context.ParkingVehicle.Remove(parkingVehicle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParkingVehicleExists(Guid id) {
            return (_context.ParkingVehicle?.Any(e => e.VehicleId == id)).GetValueOrDefault();
        }

        public JsonResult SearchVehicles(SearchVehicleParameters parameters) {
            var vehicles = from p in _context.ParkingVehicle
                           select p;
            if (!String.IsNullOrEmpty(parameters.Query)) {
                parameters.Query = parameters.Query.Trim().ToLower();
                vehicles = vehicles.Where(p => p.Plate.ToLower().Equals(parameters.Query));
            }
            vehicles = vehicles.OrderBy(p => p.FullName);
            int totalPages = (vehicles.Count() + parameters.PageSize - 1) / parameters.PageSize;
            int pageNumber = (parameters.PageNumber ?? 1);
            var retVal = new PagedData<ParkingVehicle>() { Data = vehicles.ToPagedList(pageNumber, parameters.PageSize).ToList(), TotalPages = totalPages };
            return Json(retVal);
        }
    }
}
