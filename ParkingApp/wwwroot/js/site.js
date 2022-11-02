// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var generalViewModel = function (vehicles) {
    var self = this;
    self.vehicles = ko.observableArray();
    self.loading = ko.observable(true);
    /* Constructor */
    if (vehicles !== null) {
        self.vehicles = ko.observableArray(ko.utils.arrayMap(vehicles, function (vehicle) {
            return new parkingVehicleViewModel(vehicle);
        }));
    }
    /* SEARCHING AND PAGING */
    self.searchQueryString = ko.observable('');
    self.notPageSearch = ko.observable(false);
    self.typeQueryString = ko.observable("Either");
    self.showcaseData = function (data) {
        self.vehicles(ko.utils.arrayMap(data.data, function (vehicle) {
            return new parkingVehicleViewModel(vehicle);
        }));
        self.totalPages(data.totalPages);
        self.loading(false);
    }
    self.searchVehicle = function () {
        self.loading(true);
        var pageUrl = "/ParkingVehicles/SearchVehicles";
        self.searchQueryString(self.searchQueryString());
        if (self.notPageSearch() === true) {
            self.pageNumber(1);
        }
        var parameters = { "Query": self.searchQueryString(), "PageNumber": self.pageNumber(), "Type": self.typeQueryString() };
        $.ajax({
            type: 'GET',
            url: pageUrl,
            data: parameters,
            contentType: 'application/json',
            dataType: 'json',
            success: function (data) {
                self.showcaseData(data);
            },
            error: function (data, success, error) {
                alert("Error : " + error);
            }
        });
    }

    //self.filterByType = function (typeStr) {
    //    self.typeQueryString(typeStr);
    //    //self.searchProductByType();
    //}

    //self.removeFilters = function () {
    //    self.searchQueryString('');
    //    self.typeQueryString('');
    //    self.searchProduct();
    //}
    /* Paging */
    self.pageGroup = ko.observableArray();
    self.totalPages = ko.observable(0);
    self.pageNumber = ko.observable(1);

    self.setPages = ko.computed(function () {
        self.pageGroup.removeAll();
        for (var i = 1; i < self.totalPages() + 1; i++) {
            var pageSelected = false;
            if (self.pageNumber() === i) {
                pageSelected = true;
            }
            self.pageGroup.push({ page: i, isSelected: pageSelected });
        }
    });

    self.goToPage = function (pg) {
        self.pageNumber(pg.page);
        self.notPageSearch(false);
        self.searchVehicle();
        self.pageNumber(pg.page);
        self.notPageSearch(true);
    }

    /* INITIATE */
    self.initPages = function(pages){
        self.totalPages(pages);
    }
}

var parkingVehicleViewModel = function (vehicle) {
    var self = this;
    self.FleetOrVisitorList = ko.observableArray([{ "title": "Visitor", "value": 0 }, { "title": "Fleet", "value": 1 }]);
    self.VehicleId = ko.observable(vehicle === null ? '' : vehicle.VehicleId === null ? '' : vehicle.VehicleId);
    self.FullName = ko.observable(vehicle === null ? '' : vehicle.FullName === null ? '' : vehicle.FullName);
    self.Plate = ko.observable(vehicle === null ? '' : vehicle.Plate === null ? '' : vehicle.Plate);
    self.FleetOrVisitor = ko.observable(vehicle === null ? '' : vehicle.FleetOrVisitor === null ? '' : vehicle.FleetOrVisitor/*(vehicle.FleetOrVisitor === 0 ? "Visitor" : "Fleet")*/);
    self.FOVShow = ko.computed(function () {
        return (self.FleetOrVisitor() === 0 ? "Visitor" : "Fleet");
    });
    self.updateVehicle = function (vehicle) {
        self.FullName(vehicle === null ? '' : vehicle.FullName === null ? '' : vehicle.FullName);
        self.Plate(vehicle === null ? '' : vehicle.Plate === null ? '' : vehicle.Plate);
        self.FleetOrVisitor(vehicle === null ? '' : vehicle.FleetOrVisitor === null ? '' : vehicle.FleetOrVisitor);
    }
    self.insertVehicle = function () {
        self.editOrInsert("/Home/AddVehicle");
    }

    self.editVehicle = function () {
        self.editOrInsert("/ParkingVehicles/EditVehicle");
    }

    self.editOrInsert = function (pageURL) {
        self.FleetOrVisitor = self.FleetOrVisitor.value;
        $.ajax({
            type: 'POST',
            url: pageURL,
            data: ko.toJSON(self),
            contentType: 'application/json',
            dataType: 'json',
            success: function (data) {
            },
            error: function (data, success, error) {
                alert("Error : " + error);
            }
        });
    }
}

var generalPAViewModel = function (activities) {
    var self = this;
    self.loading = ko.observable(true);
    self.activities = ko.observableArray();
    /* Constructor */
    if (activities !== null) {
        self.activities = ko.observableArray(ko.utils.arrayMap(activities, function (activity) {
            return new parkingActivityViewModel(activity);
        }));
    }

    /* VEHICLE BEING INSERTED ALONGSIDE ACTIVITY */
    self.plateQuery = ko.observable('');
    self.editable = ko.observable(false);
    self.getLicensePlate = function () {
        var parameters = { "Query": self.plateQuery() };
        $.ajax({
            type: 'GET',
            url: '/Home/GetLicensePlate',
            data: parameters,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (data) {
            },
            error: function (data, success, error) {
                alert("Error : " + error);
            }
        });
    }
    self.alterModal = $('#alterModal');

    self.showcasedVehicle = new parkingVehicleViewModel(null);
    self.vehicleArrival = function (vh) {
        self.showcasedVehicle.updateVehicle(vh);
        if (vh.FleetOrVisitor !== 1) {
            console.log('opening vehicle modal');
            if (vh.FullName === null || vh.FullName === undefined) {
                self.editable(true);
            }
            self.alterModal.modal('show');
        } else {
            console.log('vehicle is fleet');
            self.insertVehicle();
        }
    }

    self.insertVehicle = function () {
        self.alterModal.modal('hide');
        self.showcasedVehicle.FleetOrVisitor = self.showcasedVehicle.FleetOrVisitor.value;
        $.ajax({
            type: 'POST',
            url: '/Home/AddVehicle',
            data: ko.toJSON(self.showcasedVehicle),
            contentType: 'application/json',
            dataType: 'json',
            success: function (data) {
                self.activities.push(new parkingActivityViewModel(data));
            },
            error: function (data, success, error) {
                alert("Error : " + error);
            }
        });
        self.showcasedVehicle = new parkingVehicleViewModel(null);
        self.editable(false);
    }

    /* SEARCHING */
    self.searchQueryString = ko.observable('');
    self.plateQueryString = ko.observable('');
    self.checkOutQueryString = ko.observable('');
    self.fullNameQueryString = ko.observable('');
    self.notPageSearch = ko.observable(false);

    self.showcaseData = function (data) {
        self.activities(ko.utils.arrayMap(data.data, function (activity) {
            return new parkingActivityViewModel(activity);
        }));
        self.totalPages(data.totalPages);
        self.loading(false);
    }
    self.searchActivity = function () {
        self.loading(true);
        self.searchActivityTF(0);
    }

    self.searchActivityCompleted = function () {
        self.loading(true);
        self.searchActivityTF(1);
    }

    self.searchActivityTF = function (InProgressCompleted) {
        var pageUrl = "/ParkingActivities/SearchActivities";
        self.searchQueryString(self.searchQueryString());
        self.plateQueryString(self.plateQueryString());
        self.checkOutQueryString(self.checkOutQueryString());
        self.fullNameQueryString(self.fullNameQueryString());
        if (self.notPageSearch() === true) {
            self.pageNumber(1);
        }
        var parameters = { "Query": self.searchQueryString(), "PageNumber": self.pageNumber(), "InProgressCompleted": InProgressCompleted, "PlateQuery": self.plateQueryString(), "FullNameQuery": self.fullNameQueryString(), "CheckOutQuery": self.checkOutQueryString() };
        $.ajax({
            type: 'GET',
            url: pageUrl,
            data: parameters,
            contentType: 'application/json',
            dataType: 'json',
            success: function (data) {
                self.showcaseData(data);
            },
            error: function (data, success, error) {
                alert("Error : " + error);
            }
        });
    }

    /* REMOVE FILTERS */
    self.removeFilters = function (InProgressCompleted) {
        self.searchQueryString('');
        self.plateQueryString('');
        self.checkOutQueryString('');
        self.fullNameQueryString('');
        self.searchActivityTF(InProgressCompleted);
    }
    self.removeFiltersInProgress = function () {
        self.removeFilters(0);
    }
    self.removeFiltersCompleted = function () {
        self.removeFilters(1);
    }

    /* PAGING */
    self.pageGroup = ko.observableArray();
    self.totalPages = ko.observable(0);
    self.pageNumber = ko.observable(1);

    self.setPages = ko.computed(function () {
        self.pageGroup.removeAll();
        for (var i = 1; i < self.totalPages() + 1; i++) {
            var pageSelected = false;
            if (self.pageNumber() === i) {
                pageSelected = true;
            }
            self.pageGroup.push({ page: i, isSelected: pageSelected });
        }
    });

    self.goToPageActivity = function (pg) {
        self.pageNumber(pg.page);
        self.notPageSearch(false);
        self.searchActivity();
        self.pageNumber(pg.page);
        self.notPageSearch(true);
    }

    self.goToPageCompleted = function (pg) {
        self.pageNumber(pg.page);
        self.notPageSearch(false);
        self.searchActivityCompleted();
        self.pageNumber(pg.page);
        self.notPageSearch(true);
    }

    /* INITIATE */
    self.initPages = function (pages) {
        self.totalPages(pages);
    }

    /* MOVE ACTIVITY TO THE COMPLETED ACTIVITY LIST */
    connection.on("removeActivity", function (ac) {
        self.activities.remove(function (activity) {
            return activity.Id() === ac.Id;
        });
    });


    self.completeActivity = function (activity) {
        if (confirm("Complete activity?")) {
            $.ajax({
                type: 'POST',
                url: '/ParkingActivities/CompleteActivity',
                data: ko.toJSON(activity.Id),
                contentType: 'application/json',
                dataType: 'json',
                success: function (data) {
                },
                error: function (data, success, error) {
                    alert("Error : " + error);
                }
            });
        }
    }
}

var parkingActivityViewModel = function (activity) {
    var self = this;
    self.Id = ko.observable(activity === null ? '' : activity.Id);
    self.CheckIn = ko.observable(activity === null ? '' : activity.CheckIn);
    self.CheckOut = ko.observable(activity === null ? '' : activity.CheckOut);
    self.VehicleId = ko.observable(activity === null ? '' : activity.VehicleId);
    self.FullName = ko.observable(activity === null ? '' : activity.FullName);
    self.Plate = ko.observable(activity === null ? '' : activity.Plate);
    self.Status = ko.observable(activity === null ? '' : (activity.Status === 0 ? "In Progress" : "Completed"));

    /* FUNCTIONS */
    self.updateActivity = function (activity) {
        self.Id = ko.observable(activity === null ? '' : activity.Id);
        self.ChekcIn = ko.observable(activity === null ? '' : activity.CheckIn);
        self.CheckOut = ko.observable(activity === null ? '' : activity.CheckOut);
        self.VehicleId = ko.observable(activity === null ? '' : activity.VehicleId);
        self.FullName = ko.observable(activity === null ? '' : activity.FullName);
        self.Plate = ko.observable(activity === null ? '' : activity.Plate);
        self.Status = ko.observable(activity === null ? '' : activity.Status);
    }
}

formatDate = function (value) {
    var dateString = value.toString().substring(0, 10) + "  " + value.toString().substring(11, 19);
    return dateString;
}