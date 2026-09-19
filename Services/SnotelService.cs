using Dapper;
using Powderlines.Models;
using Powderlines.Utils;
using System.Data;

namespace Powderlines.Services
{
	public interface ISnotelService
	{
		Task<int> GetNumberOfStationsAsync(string state);
		Task<List<StationInformation>> GetStationsAsync(string state);
		Task<StationResponse?> GetStationInfoAsync(string id, string? days, string? start_date, string? end_date);
		Task<List<StationInformation>> GetClosestStationsAsync(double latitude, double longitude, int count);
		Task<List<StationResponse>> GetClosestStationsWithDataAsync(double latitude, double longitude, int count, int days);
		//Task<string> InsertStations();
	}

	public class SnotelService : ISnotelService
	{
		private readonly IDbConnection _context;

		public SnotelService(IDbConnection context)
		{
			_context = context;
		}

		public async Task<int> GetNumberOfStationsAsync(string state)
		{
			if (state == "all")
			{
				var sql = "SELECT Count(SnotelID) FROM Snotel";
				_context.Open();
				var result = await _context.QueryFirstOrDefaultAsync<int>(sql);
				_context.Close();
				return result;
			}
			else
			{
				var sql = "SELECT Count(SnotelID) FROM Snotel WHERE State = @State";
				_context.Open();
				var result = await _context.QueryFirstOrDefaultAsync<int>(sql, new { State = state });
				_context.Close();
				return result;
			}
		}

		public async Task<List<StationInformation>> GetStationsAsync(string state)
		{
			IEnumerable<FlatStationInformation> flatStations;

			if (state == "all")
			{
				var sql = "SELECT * FROM Snotel";
				_context.Open();
				flatStations = await _context.QueryAsync<FlatStationInformation>(sql);
				_context.Close();
			}
			else
			{
				var sql = "SELECT * FROM Snotel WHERE State = @State";
				_context.Open();
				flatStations = await _context.QueryAsync<FlatStationInformation>(sql, new { State = state });
				_context.Close();
			}

			List<StationInformation> stations = flatStations.Select(fs => new StationInformation
			{
				Triplet = fs.Triplet,
				Name = fs.Name,
				Elevation = fs.Elevation,
				Location = new StationLocation
				{
					Lat = fs.Latitude,
					Lng = fs.Longitude
				}
			}).ToList();

			return stations;
		}

		public async Task<StationResponse?> GetStationInfoAsync(string id, string? days, string? start_date, string? end_date)
		{
			try
			{
				var sql = "SELECT Triplet, Name, State, Elevation, Latitude, Longitude FROM Snotel WHERE Triplet = @triplet";
				
				_context.Open();
				var result = await _context.QueryFirstOrDefaultAsync<FlatStationInformation>(sql, new { triplet = id });
				_context.Close();

				if (result == null)
					return null;

				StationInformation station = new()
				{
					Triplet = result.Triplet,
					Name = result.Name,
					Elevation = result.Elevation,
					Location = new StationLocation
					{
						Lat = result.Latitude,
						Lng = result.Longitude
					}
				};

				if (station == null)
					return null;

				List<SnowData>? snowData = await SnotelData.GetSnowDataAsync(id, days, start_date, end_date);

				StationResponse stationResponse = new()
				{
					StationInformation = station,
					Data = snowData
				};

				return stationResponse;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error retrieving station info for {id}: {ex.Message}");
				_context.Close();
				return null;
			}	
		}

		public async Task<List<StationInformation>> GetClosestStationsAsync(double latitude, double longitude, int count)
		{

			var stations = await GetStationsAsync("all");
			if (stations == null || stations.Count == 0)
				return new List<StationInformation>();
			var closestStations = stations
				.Select(station =>
				{
					double distance = Utils.SnotelData.GetDistanceAsCrowFlies(new double[] { longitude, latitude }, new double[] { station.Location.Lng, station.Location.Lat });
					return new { Station = station, Distance = distance };
				})
				.OrderBy(x => x.Distance)
				.Take(count)
				.Select(x => x.Station)
				.ToList();
			return closestStations;

		}

		public async Task<List<StationResponse>> GetClosestStationsWithDataAsync(double latitude, double longitude, int count, int days)
		{
			// Need to update to get the closest stations, and then get the data associated with those stations.
			List<StationResponse> stationResponses = new List<StationResponse>();

			List<StationInformation> stations = await GetClosestStationsAsync(latitude, longitude, count);

			// Iterate through each station
			foreach (StationInformation si in stations)
			{
				// Get the snow data for each station
				List<SnowData>? snowData = await SnotelData.GetSnowDataAsync(si.Triplet, days.ToString(), null, null);

				// Create a Station Response object for each station with the original station information and the snow data
				StationResponse stationResponse = new()
				{
					StationInformation = si,
					Data = snowData
				};

				// Add the station response to the list
				stationResponses.Add(stationResponse);
			}

			return stationResponses;
		}

		//public async Task<string> InsertStations()
		//{
		//	List<StationInformation> stations = await SnotelData.LoadStationsFromDataFileAsync();
		//	if (stations == null)
		//		return "Failed to load stations.";

		//	int insertedCount = 0;
		//	int totalCount = stations.Count;

		//	_context.Open();
		//	var sql = "DELETE FROM Snotel";
		//	await _context.ExecuteAsync(sql);

		//	sql = "INSERT INTO Snotel (SnotelID, Name, State, Elevation, Latitude, Longitude) VALUES (@ID, @Name, @State, @Elevation, @Lat, @Lng)";
		//	foreach (var station in stations)
		//	{
		//		try
		//		{
		//			await _context.ExecuteAsync(sql, new
		//			{
		//				ID = station.Triplet.Split(':')[0],
		//				Name = station.Name,
		//				State = station.Triplet.Split(':')[1],
		//				Elevation = station.Elevation,
		//				Lat = station.Location.Lat,
		//				Lng = station.Location.Lng
		//			});

		//			insertedCount++;
		//		}
		//		catch (Exception ex)
		//		{
		//			Console.WriteLine($"Error inserting station {station.Triplet}: {ex.Message}");
		//		}
		//		_context.Close();
		//	}

		//	return $"Inserted {insertedCount} out of {totalCount} stations into the database.";

		//}
	}
}