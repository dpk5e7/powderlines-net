using Microsoft.AspNetCore.Http.Timeouts;
using Powderlines.Models;
using Powderlines.Services;

namespace Powderlines.API
{
	public static class APIEndpoints
	{
		public static void MapAPIEndpoints(this IEndpointRouteBuilder app)
		{
			RouteGroupBuilder group1 = app.MapGroup("/api/station");
			group1.MapGet("/{id}", (ISnotelService snotelService, string id, string? days, string? start_date, string? end_date) => snotelService.GetStationInfoAsync(id, days, start_date, end_date));

			RouteGroupBuilder group2 = app.MapGroup("/api/stations");
			group2.MapGet("/", (ISnotelService snotelService, string state ="all") => snotelService.GetStationsAsync(state));
			group2.MapGet("/{state}", (ISnotelService snotelService, string state) => snotelService.GetStationsAsync(state));

			RouteGroupBuilder group3 = app.MapGroup("/api/closest_stations");
			group3.MapGet("/", (ISnotelService snotelService, double lat, double lng, int count, bool data = false, int days = 5) => GetClosestStationsAsync(snotelService, lat, lng, count, data, days));

			RouteGroupBuilder group4 = app.MapGroup("/api/stationsCount");
			group4.MapGet("/", (ISnotelService snotelService,string state ="all") => snotelService.GetNumberOfStationsAsync(state));
			group4.MapGet("/{state}", (ISnotelService snotelService, string state) => snotelService.GetNumberOfStationsAsync(state));

			//RouteGroupBuilder group5 = app.MapGroup("/api/insert");
			//group5.MapGet("/", (ISnotelService snotelService) => snotelService.InsertStations());
		}

		private static async Task<IResult> GetClosestStationsAsync(ISnotelService snotelService, double latitude, double longitude, int count, bool data, int days)
		{
			if (data)
			{
				List<StationResponse> stations = await snotelService.GetClosestStationsWithDataAsync(latitude, longitude, count, days);
				return Results.Ok(stations);
			}
			else
			{
				List<StationInformation> stations = await snotelService.GetClosestStationsAsync(latitude, longitude, count);
				return Results.Ok(stations);
			}
		}
	}
}
