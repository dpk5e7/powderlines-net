//section:global variables go here 👇
let maps = new WeakMap();

//section: API calls go here 👇
async function getSNOTELStations(state) {
  let requestOptions = {
    method: "GET",
    redirect: "follow",
  };

  let params = {
    state: state,
  };

  let apiUrl = "/api/stations?";

  for (let p in params) {
    apiUrl += `${p}=${params[p]}&`;
  }
  apiUrl = encodeURI(apiUrl.slice(0, -1));

  const response = await fetch(apiUrl, requestOptions);

  const data = await response.json();
  return data;
}

async function getStationData(triplet) {
  //triplet = "672:WA:SNTL";

  // see if there's station data for today in localStorage
  if (localStorage.getItem(triplet)) {
    const lcData = JSON.parse(localStorage.getItem(triplet));
    // check the date

    if (lcData.data) {
      // Create date from input value
      let lcDate = new Date(`${lcData.data[0].Date}T12:00`);

      // Get today's date
      let today = new Date();

      // call setHours to take the time out of the comparison
      if (lcDate.setHours(0, 0, 0, 0) == today.setHours(0, 0, 0, 0)) {
        // Date equals today's date
        // return the object in localStorage
        return lcData;
      }
    }
  }

  let requestOptions = {
    method: "GET",
    redirect: "follow",
  };

  let params = {
    days: 0,
  };

  let apiUrl = `/api/station/${triplet}?`;

  for (let p in params) {
    apiUrl += `${p}=${params[p]}&`;
  }
  apiUrl = encodeURI(apiUrl.slice(0, -1));

  const response = await fetch(apiUrl, requestOptions);

  const data = await response.json();

  localStorage.setItem(triplet, JSON.stringify(data));

  return data;
}

// This function displays the SNOTEL markers
async function displaySNOTELMarkers(map, markers) {
  const SNOTELIcon = L.Icon.extend({
    options: {
      iconUrl: "./images/blue-dot.png",
      shadowUrl: "./images/msmarker.shadow.png",
      iconSize: [32, 32],
      shadowSize: [59, 32],
      iconAnchor: [16, 32],
      shadowAnchor: [16, 32],
      tooltipAnchor: [10, -24],
    },
  });

  let stationIcon = new SNOTELIcon();

  let markerOptions = {
    icon: stationIcon,
    riseOnHover: true,
  };

  let toolTipOptions = {
    offset: [0, 0],
    direction: "right",
    opacity: 0.8,
  };

  const snotelStations = await getSNOTELStations("all");

  for (let station of snotelStations) {
    let marker = L.marker(
      [station.location.lat, station.location.lng],
      markerOptions
    ).addTo(markers);

    marker.bindTooltip(`<h5>${station.name}</h5>`, toolTipOptions);

    function markerClick(event) {
      const clickedStation = snotelStations.find(
        (element) =>
          element.location.lat === event.latlng.lat &&
          element.location.lng === event.latlng.lng
      );

      // Initial popup text with indeterminate progress bar
      let popupText = `<p><h5>SNOTEL: ${clickedStation.name}</h5>
                ID: ${clickedStation.triplet}<br />
                Elevation: ${clickedStation.elevation} ft</p>`;

      let popupOptions = {
        autoPan: true,
        keepInView: true,
        offset: [0, -24],
      };

      let popup = L.popup(popupOptions)
        .setLatLng({
          lat: clickedStation.location.lat,
          lng: clickedStation.location.lng,
        })
        .setContent(popupText)
        .openOn(map);

      displayClickedSNOTELDataInPopup(clickedStation.triplet, popup);
    }
    marker.addEventListener("click", markerClick);
 }
}

// This function fetches and displays the SNOTEL data in the popup.
async function displayClickedSNOTELDataInPopup(triplet, popup) {
  const snowData = await getStationData(triplet);

  // Data is:
  // {
  //   station_information: {
  //     name: "Loveland Basin",
  //     triplet: "602:CO:SNTL",
  //     elevation: 11427,
  //     location: {
  //       lat: 39.67428,
  //       lng: -105.90264,
  //     },
  //     distance: "0.50",
  //   },
  //   data: [
  //     {
  //       Date: "2023-01-26",
  //       "Snow Water Equivalent (in)": "10.6",
  //       "Change In Snow Water Equivalent (in)": "0.1",
  //       "Snow Depth (in)": "46",
  //       "Change In Snow Depth (in)": "1",
  //       "Observed Air Temperature (degrees farenheit)": "0.1",
  //     },
  //   ],
  // }

  // Update the popup text with SNOTEL data
  let popupText = `<p><h5>SNOTEL: ${snowData.station_information.name}</h5>
                ID: ${snowData.station_information.triplet}<br />
                Elevation: ${snowData.station_information.elevation} ft<br />
                Snow Depth: ${snowData.data[0]["Snow Depth (in)"]}"<br />
                Change In Snow Depth: ${snowData.data[0]["Change In Snow Depth (in)"]}"<br />
                Air Temperature: ${snowData.data[0]["Observed Air Temperature (degrees farenheit)"]} \xB0F</p>
              `;

  popup.setContent(popupText);
}

// This function is the initial starting point of operation
export function initializeMap(element) {
  if (maps.has(element)) {
      maps.get(element).remove();
  }

  let lat = "41.91562";
  let lon = "-113.41154";
  let view = 5;

  const map = L.map(element).setView([lat, lon], view);

  L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
    maxZoom: 19,
    attribution: "© OpenStreetMap",
  }).addTo(map);

  let markers = L.layerGroup().addTo(map);

  displaySNOTELMarkers(map, markers);

  maps.set(element, map);

  setTimeout(() => map.invalidateSize(), 100);
}

export function disposeMap(elementId) {
  const map = maps[elementId];

  if (map) {
    map.remove();
    delete maps[elementId];
  }
}

