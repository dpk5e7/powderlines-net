# PowderLines.net

[![License Apache-2.0](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](https://www.apache.org/licenses/LICENSE-2.0)

This project is an ASP.Net port of the [PowderLin.es API](http://powderlin.es/api.html). The Ruby on Rails repo is located [here](https://github.com/bobbymarko/powderlines-api).  I previously ported this to Node.js, located [here](https://github.com/dpk5e7/powderlines-js).

Apparently, there's an [official API](https://wcc.sc.egov.usda.gov/awdbRestApi/swagger-ui/index.html) now, but I think this is a fun project anyway.

## Overview

Use our free API for accessing SNOTEL station data. Our API is useful for finding current snow levels in mountainous regions across the United States. 895 SNOTEL stations are available.

### [https://powderlines.kellysoftware.org/](https://powderlines.kellysoftware.org/)

## Table Of Contents

- [Usage](#usage)
- [License](#license)
- [Contributing](#contributing)
- [Technology Used](#technology-used)
- [Acknowledgements](#acknowledgements)
- [Contributors](#contributers)

## Usage

### Get all SNOTEL stations

Description:
Returns basic information about all of the SNOTEL stations in the United States.

Endpoint: /stations

Request parameters:
| Parameter | Descriptions |
|----|----|
| State | The state to retreive data from. (optional) |

Response parameters:
The response comes as an array of objects. Here is a breakdown of a returned object.
|Parameter | Descriptions |
|----|----|
|Name (string) | Name of the station |
|Triplet (string) | Unique identifier for the station. Formatted as ###:STATE:SNTL |
|Elevation (integer) | Elevation of the station in feet |
|Location (lat, lng object) | Latitude and longitude of the station |

Sample calls:

- [https://powderlines.kellysoftware.org/api/stations](https://powderlines.kellysoftware.org/api/stations)
- [https://powderlines.kellysoftware.org/api/stations?state=CO](https://powderlines.kellysoftware.org/api/stations?state=CO)

Sample Response:

```json
[
  {
    "name": "Berthoud Summit",
    "triplet": "335:CO:SNTL",
    "elevation": 11300,
    "location": {
      "lat": 39.80392,
      "lng": -105.77789
    }
  },
  {
    "name": "Loveland Basin",
    "triplet": "602:CO:SNTL",
    "elevation": 11400,
    "location": {
      "lat": 39.67433,
      "lng": -105.90133
    }
  },
  {
    ...
  }
]
```

### Get snow info for a station

Description:
Returns detailed information for the specified SNOTEL station.

Endpoint: /station/:id

Request parameters:
| Parameter | Description |
| ---- | ---- |
| ID (triplet) | Station id in the form of ###:STATE:SNTL. Example: 335:CO:SNTL. Find the triplet for a particular station through the /stations endpoint. |
| Days (integer) | Number of days information to retrieve from today. (optional - defaults to 5 unless using start_date and end_date) |
| Start date (YYYY-MM-DD) | Historical date to pull data from. Use in conjunction with end date. (optional) |
| End date (YYYY-MM-DD) | Historical date to pull data from. Use in conjunction with start date (optional) |

Response parameters:
The response includes basic station information in addition to an array of snow data.
| Parameter | Description |
| ---- | ---- |
| Date | Date measurement was taken |
| Snow Water Equivalent (in) | The amount of water contained within the snowpack. |
| Change In Snow Water Equivalent (in) | The change in the snow water equivalent from the last measurement (typically the past 24 hours). |
| Snow Depth (in) | Depth of snow in inches. |
| Change In Snow Depth (in) | The change in the snow depth from the last measurement (typically the past 24 hours). |
| Observed Air Temperature (degrees farenheit) | The observed air temperature, in degrees farenheit. |

Sample calls:

- [https://powderlines.kellysoftware.org/api/station/335:CO:SNTL](https://powderlines.kellysoftware.org/api/station/335:CO:SNTL)
- [https://powderlines.kellysoftware.org/api/station/335:CO:SNTL?days=10](https://powderlines.kellysoftware.org/api/station/335:CO:SNTL?days=10)
- [https://powderlines.kellysoftware.org/api/station/335:CO:SNTL?start_date=2023-01-09&end_date=2023-01-11](https://powderlines.kellysoftware.org/api/station/335:CO:SNTL?start_date=2023-01-09&end_date=2023-01-11)

Sample response:

```json
{
  "station_information":
    {
      "name": "Berthoud Summit",
      "triplet": "335:CO:SNTL",
      "elevation": 11300,
      "location": {
        "lat": 39.80392,
        "lng": -105.77789
      }
    },
  "data": [
    {
      "Date": "2023-01-21",
      "Snow Water Equivalent (in)": "10.9",
      "Change In Snow Water Equivalent (in)": "-0.1",
      "Snow Depth (in)": "43",
      "Change In Snow Depth (in)": "-1",
      "Observed Air Temperature (degrees farenheit)": "6"
    },
    {
      "Date": "2023-01-22",
      "Snow Water Equivalent (in)": "11.1",
      "Change In Snow Water Equivalent (in)": "0.2",
      "Snow Depth (in)": "44",
      "Change In Snow Depth (in)": "1",
      "Observed Air Temperature (degrees farenheit)": "4"
    },
    {
      ...
    }
  ]
}
```

## Find closest station to a latitude and longitude:

Description:
Returns detailed information for the closest SNOTEL stations to a geographic point.

Endpoint: /closest_stations

Request parameters:
| Parameter | Description |
| ---- | ---- |
| lat (float) | Latitude to base search off of. (required) |
| lng (float) | Longitude to base search off of. (required) |
| count (integer) | number of station's to return (optional - defaults to 3, maximum of 5) |
data (boolean) | Setting to true will enable fetching of snow info from the stations. Note that this might be slow depending on the number of stations you're requesting information from. |
| days (integer) | Number of days information to retrieve from today. (optional - defaults to 5) |

Response parameters:
The response is an array of stations including their basic information in addition to an array of snow data.
| Parameter | Description |
| ---- | ---- |
| Date | Date measurement was taken |
| Snow Water Equivalent (in) | The amount of water contained within the snowpack. |
| Change In Snow Water Equivalent (in) | The change in the snow water equivalent from the last measurement (typically the past 24 hours). |
| Snow Depth (in) | Depth of snow in inches. |
| Change In Snow Depth (in) | The change in the snow depth from the last measurement (typically the past 24 hours). |
| Observed Air Temperature (degrees farenheit) | The observed air temperature, in degrees farenheit. |

Sample calls:

- [https://powderlines.kellysoftware.org/api/closest_stations?lat=39.7392&lng=-104.9903&count=3&data=true&days=3](https://powderlines.kellysoftware.org/api/closest_stations?lat=39.7392&lng=-104.9903&count=3&data=true&days=3)
- [https://powderlines.kellysoftware.org/api/closest_stations?lat=39.7392&lng=-104.9903&count=4](https://powderlines.kellysoftware.org/api/closest_stations?lat=39.7392&lng=-104.9903&count=4)

Sample response:

```json
[
  {
    "station_information":
    {
      "name": "Echo Lake",
      "triplet": "936:CO:SNTL",
      "elevation": 10600,
      "location": {
        "lat": 39.65627,
        "lng": -105.59345
      }
    },
    "data": [
      {
        "Date": "2023-01-19",
        "Snow Water Equivalent (in)": "3.2",
        "Change In Snow Water Equivalent (in)": "0.1",
        "Snow Depth (in)": "20",
        "Change In Snow Depth (in)": "0",
        "Observed Air Temperature (degrees farenheit)": "8"
      },
      {
        "Date": "2023-01-20",
        "Snow Water Equivalent (in)": "3.3",
        "Change In Snow Water Equivalent (in)": "0.1",
        "Snow Depth (in)": "19",
        "Change In Snow Depth (in)": "-1",
        "Observed Air Temperature (degrees farenheit)": "12"
      },
      {
        ...
      }
    ]    
  },
  {
    ...
  }
]
```

## License

[![License Apache-2.0](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](https://www.apache.org/licenses/LICENSE-2.0)

## Contributing

[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-2.1-4baaaa.svg)](code_of_conduct.md)

## Technology Used

- [ASP.Net](https://dotnet.microsoft.com/apps/aspnet)
- [Blazor](https://dotnet.microsoft.com/apps/blazor)
- [Leaflet.js](https://leafletjs.com/)
- [Dapper](https://github.com/StackExchange/Dapper)

## Acknowledgements

- [Bobby Marko](https://github.com/bobbymarko)
  - Thank you to Mr. Marko for creating a great API. It is most definitely something that skiers should use to prepare for a trip into the backcountry.
- [USDA Natural Resources Conservation Service - National Water and Climate Center](https://www.nrcs.usda.gov/wps/portal/wcc/home/)

## Contributors

- [Dan Kelly](https://github.com/dpk5e7)
