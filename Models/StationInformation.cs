using System.Text.Json.Serialization;

namespace Powderlines.Models
{
	public class StationInformation
	{
		[JsonPropertyName("name")]
		public required string Name { get; set; }
		
		[JsonPropertyName("triplet")]
		public required string Triplet { get; set; }
		
		[JsonPropertyName("elevation")]
		public required int Elevation { get; set; }
		
		[JsonPropertyName("location")]
		public required StationLocation Location { get; set; }
	}

	public class StationLocation
	{
		[JsonPropertyName("lat")]
		public required double Lat { get; set; }
		
		[JsonPropertyName("lng")]
		public required double Lng { get; set; }
	}
}