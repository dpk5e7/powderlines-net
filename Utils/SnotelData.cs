using Powderlines.Models;
using System.Text.Json;

namespace Powderlines.Utils
{
	public static class SnotelData
	{
		private static readonly string dataFilePath = Path.Combine(
				Directory.GetCurrentDirectory(),
				"data",
				"snotelStations.json"
		);

		public static async Task<List<StationInformation>> LoadStationsFromDataFileAsync()
		{
			using var stream = File.OpenRead(dataFilePath);
			return await JsonSerializer.DeserializeAsync<List<StationInformation>>(stream) ?? new List<StationInformation>();
		}

		public static double GetDistanceAsCrowFlies(double[] start, double[] end)
		{
			const double R = 3960.0; // Earth radius in miles

			double dLat = Deg2Rad(end[1] - start[1]);
			double dLon = Deg2Rad(end[0] - start[0]);

			double a =
					Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
					Math.Cos(Deg2Rad(start[1])) *
					Math.Cos(Deg2Rad(end[1])) *
					Math.Sin(dLon / 2) *
					Math.Sin(dLon / 2);

			double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
			return R * c; // distance in miles
		}

		private static double Deg2Rad(double deg)
		{
			return deg * (Math.PI / 180.0);
		}

		/// <summary>
		/// Fetches daily SNOTEL data from USDA WCC report generator and returns a JSON string
		/// mirroring the JS function: an array of objects keyed by the CSV header row.
		/// 
		/// Example id: "301:CA:SNTL" or station id used by the API.
		/// - If startDate and endDate are supplied, the API date segment is "yyyy-MM-dd,yyyy-MM-dd".
		/// - Otherwise it uses "-{days}".
		/// </summary>
		public static async Task<List<SnowData>?> GetSnowDataAsync(string id, string? days = "5", string? startDate = null, string? endDate = null)
		{
			if (string.IsNullOrWhiteSpace(id))
				throw new ArgumentException("id must be provided", nameof(id));

			int intDays = 5;
			if (!String.IsNullOrWhiteSpace(days))
			{
				intDays = Math.Abs(int.TryParse(days, out int result) ? result - 1 : intDays - 1);
			}

			// Build date segment just like the JS: either "startDate,endDate" or "-days"
			string dateSegment = (String.IsNullOrWhiteSpace(startDate) || String.IsNullOrWhiteSpace(endDate))
					? $"-{intDays}"
					: $"{startDate:yyyy-MM-dd},{endDate:yyyy-MM-dd}";

			// This preserves your original URL structure and percent-encoding
			string apiUrl =
					$"https://wcc.sc.egov.usda.gov/reportGenerator/view_csv/customSingleStationReport/daily/" +
					$"{id}%7Cid%3D%22%22%7Cname/{dateSegment}%2C0/" +
					$"WTEQ%3A%3Avalue%2CWTEQ%3A%3Adelta%2CSNWD%3A%3Avalue%2CSNWD%3A%3Adelta%2CTOBS%3A%3Avalue";

			using var http = new HttpClient();

			// The JS sets Content-Type for a GET (not required); we can set Accept to text/csv
			// to be explicit without changing behavior.
			http.DefaultRequestHeaders.Accept.Clear();
			http.DefaultRequestHeaders.Accept.ParseAdd("text/csv");

			var response = await http.GetAsync(apiUrl);
			response.EnsureSuccessStatusCode();

			string csv = await response.Content.ReadAsStringAsync();

			// Split lines, remove comments (#...) and blank lines (like the JS)
			var lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
										 .Where(line => !line.TrimStart().StartsWith("#"))
										 .ToList();

			if (lines.Count == 0)
				return null;

			// Mutate the header row to match the JS replacements
			string header = lines[0];

			header = header.Replace(
					"Snow Water Equivalent (in) Start of Day Values",
					"Snow Water Equivalent (in)");

			header = header.Replace(
					"Snow Depth (in) Start of Day Values",
					"Snow Depth (in)");

			header = header.Replace(
					"Air Temperature Observed (degF) Start of Day Values",
					"Observed Air Temperature (degrees farenheit)");

			var keys = header.Split(',');

			var rows = new List<Dictionary<string, string>>();

			for (int i = 1; i < lines.Count; i++)
			{
				var line = lines[i];
				if (string.IsNullOrWhiteSpace(line))
					continue;

				var values = line.Split(',');

				// Map values to keys (naïve CSV split to match the JS approach)
				var obj = new Dictionary<string, string>(StringComparer.Ordinal);
				for (int k = 0; k < keys.Length; k++)
				{
					string key = keys[k];
					string value = (k < values.Length) ? values[k] : string.Empty;
					obj[key] = value;
				}

				rows.Add(obj);
			}

			string? rawData = JsonSerializer.Serialize(rows);

			List<SnowData>? snowData = JsonSerializer.Deserialize<List<SnowData>>(rawData);

			return snowData;
		}
	}
}
