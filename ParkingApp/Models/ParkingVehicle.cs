using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ParkingApp.Models
{

    public class ParkingVehicleDto
    {
        public string FullName { get; set; }
        public string Plate { get; set; }
        public enFOV FleetOrVisitor { get; set; }

        public ParkingVehicle ToParkingVehicle() {
            var vh = new ParkingVehicle();
            vh.VehicleId = Guid.NewGuid();
            vh.FullName = FullName;
            vh.Plate = Plate;
            vh.FleetOrVisitor = FleetOrVisitor;
            return vh;
        }

    }

    public class ParkingVehicle
    {
        [Key]
        [JsonPropertyName("VehicleId")]
        public Guid? VehicleId { get; set; }
        [JsonPropertyName("FullName")]
        public string? FullName { get; set; }
        [JsonPropertyName("Plate")]
        public string? Plate { get; set; }
        [JsonPropertyName("FleetOrVisitor")]
        public enFOV FleetOrVisitor { get; set; }

        public List<ParkingActivity>? Activities { get; set; }

        public ParkingVehicle Update(ParkingVehicle vehicle) {
            FullName = vehicle.FullName;
            Plate = vehicle.Plate;
            FleetOrVisitor = vehicle.FleetOrVisitor;
            return this;
        }

        public ParkingVehicleDto ToDto() {
            var dto = new ParkingVehicleDto();
            dto.FullName = FullName;
            dto.Plate =Plate;
            dto.FleetOrVisitor = FleetOrVisitor;
            return dto;
        }
    }

    public enum enFOV
    {
        Visitor, 
        Fleet        
    }    

    public class JsonData<T>
    {
        public T? Vehicle { get; set; }
        public bool Editable { get; set; }
    }
    public class SearchParameters
    {
        public string? Query { get; set; }
    }

    public class SearchVAParameters : SearchParameters {
        public int? PageNumber { get; set; }
        public int PageSize { get; set; }

        public SearchVAParameters() {
            this.PageSize = 22;
        }
    }

    public class SearchVehicleParameters : SearchVAParameters {
        
        public string Type { get; set; }
    }

    public class SearchActivityParameters : SearchVAParameters
    {
        public enStatus InProgressCompleted { get; set; }
        public string? PlateQuery { get; set; }
        public string? CheckOutQuery { get; set; }

        public string? FullNameQuery { get; set; }
    }



    public class PagedData<T>
    {
        public int TotalPages { get; set; }
        public List<T> Data { get; set; }
    }

    public class CompletedData {
        public ParkingActivityDto parkingActivityDto { get; set; }
    }
}
