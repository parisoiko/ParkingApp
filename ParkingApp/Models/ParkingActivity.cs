using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ParkingApp.Models
{
    public class ParkingActivityDto {
        [JsonPropertyName("Id")]
        public Guid Id { get; set; }
        [JsonPropertyName("CheckIn")]
        public DateTime CheckIn { get; set; }
        [JsonPropertyName("CheckOut")]
        public DateTime? CheckOut { get; set; }
        [JsonPropertyName("VehicleId")]
        public Guid VehicleId { get; set; }
        [JsonPropertyName("FullName")]
        public string FullName { get; set; }
        [JsonPropertyName("Plate")]
        public string Plate { get; set; }
        [JsonPropertyName("Status")]
        public enStatus Status { get; set; }
    }
    public class ParkingActivity
    {
        [Key]
        [JsonPropertyName("Id")]
        public Guid Id { get; set; }
        [JsonPropertyName("CheckIn")]
        public DateTime CheckIn { get; set; }
        [JsonPropertyName("CheckOut")]
        public DateTime? CheckOut { get; set; }
        public virtual ParkingVehicle ParkingVehicle { get; set; }
        [JsonPropertyName("VehicleId")]
        public Guid VehicleId { get; set; }
        [JsonPropertyName("FullName")]
        public string? FullName { get; set; }
        [JsonPropertyName("Plate")]
        public string? Plate { get; set; }
        [JsonPropertyName("Status")]
        public enStatus Status { get; set; }
        [JsonPropertyName("RowVersion")]
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public ParkingActivity Update(ParkingActivity activity) {
            CheckIn = activity.CheckIn;
            CheckOut = activity.CheckOut;
            ParkingVehicle = activity.ParkingVehicle;
            Status = activity.Status;
            return this;
        }

        public ParkingActivityDto ToDto() {
            var dto = new ParkingActivityDto();
            dto.Id = Id;
            dto.CheckIn = CheckIn;
            dto.CheckOut = CheckOut;
            dto.VehicleId = VehicleId;
            dto.FullName = FullName;
            dto.Plate = Plate;
            dto.Status = Status;
            return dto;
        }
    }
    public enum enStatus
    {
        InProgress,
        Completed
    }
}