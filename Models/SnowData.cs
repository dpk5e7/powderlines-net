using System.Text.Json.Serialization;

namespace Powderlines.Models
{
	public class SnowData
	{
		[JsonPropertyName("Date")]
		public required DateTime Date { get; set; }

		[JsonPropertyName("Snow Water Equivalent (in)")]
		public required string SnowWaterEquivalent { get; set; }

		[JsonPropertyName("Change In Snow Water Equivalent (in)")]
		public required string ChangeInSnowWaterEquivalent { get; set; }

		[JsonPropertyName("Snow Depth (in)")]
		public required string SnowDepth { get; set; }

		[JsonPropertyName("Change In Snow Depth (in)")]
		public required string ChangeInSnowDepth { get; set; }

		[JsonPropertyName("Observed Air Temperature (degrees farenheit)")]
		public required string ObservedAirTemperature { get; set; }
	}
}
