namespace Powderlines.Models
{
	public class FlatStationInformation
	{
		public required string Name { get; set; }
		public required string Triplet { get; set; }
		public required int Elevation { get; set; }
		public required double Latitude { get; set; }
		public required double Longitude { get; set; }
	}
}