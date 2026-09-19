using System.Text.Json.Serialization;

namespace Powderlines.Models
{
	public class StationResponse
	{
		[JsonPropertyName("station_information")]
		public required StationInformation StationInformation { get; set; }

		[JsonPropertyName("data")]
		public List<SnowData>? Data { get; set; }
	}
}
