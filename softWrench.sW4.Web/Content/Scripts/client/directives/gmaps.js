(function (angular) {
    "use strict";

    angular.module("sw_layout").directive("googleMap", ["$rootScope", "$timeout", "$q", "$log", "loadGoogleMapApi", "restService", "configurationService", function ($rootScope, $timeout, $q, $log, loadGoogleMapApi, restService, configurationService) {
        return {
            restrict: "C", // restrict by class name
            scope: {
                mapId: "@id", // map ID
                tabid: "@", // used to know if the map is displayed - workaround to only use gmaps api when map is visible
                lat: "@",     // latitude
                long: "@",     // longitude
                zoom: "@",     // zoom
                code: "@",     // address code
                description: "@", // address description
                streetaddress: "@",
                city: "@",
                stateprovince: "@",
                country: "@"
            },
            link: function ($scope) {
                var log = $log.get("googleMap");

                var defaultCity = null;
                var defaultState = null;
                var defaultCountry = null;

                // used to know if the map should be updated on tab shown
                var lastLocationKey = null;


                var zoom = parseInt($scope.zoom);
                if (isNaN(zoom)) {
                    zoom = 19;
                }

                // builds a key to know if the location changed
                var getLocationKey = function() {
                    return $scope.lat + "." + $scope.long + "." + $scope.streetaddress;
                }

                var pushAddressLineTwoPart = function(array, value, defaultValue, mayUseDefault) {
                    if (value) {
                        array.push(value);
                        return;
                    }
                    if (mayUseDefault && defaultValue) {
                        array.push(defaultValue);
                    }
                }

                var buildAddressLineTwo = function (mayUseDefault) {
                    var parts = [];
                    pushAddressLineTwoPart(parts, $scope.city, defaultCity, mayUseDefault);
                    pushAddressLineTwoPart(parts, $scope.stateprovince, defaultState, mayUseDefault);
                    pushAddressLineTwoPart(parts, $scope.country, defaultCountry, mayUseDefault);
                    return parts.join(", ");
                }

                var buildAddressInfo = function (mayUseDefault, overrideHeading) {
                    var description = [];
                    if ($scope.code) {
                        description.push("(" + $scope.code + ")");
                    }
                    if ($scope.description) {
                        description.push($scope.description);
                    }
                    description = description.join(" - ");

                    var addressLineOne = $scope.streetaddress || "";
                    var addressLineTwo = buildAddressLineTwo(mayUseDefault);
                    var heading = overrideHeading || "Location Address";
                    var html = "<div><h5>{0}</h5><div>{1}</div><div>{2}</div><div>{3}</div></div>".format(heading, description, addressLineOne, addressLineTwo);
                    return html;
                }

                // opens the info window with the address
                var showInfo = function() {
                    if ($scope.infoWindow.getContent()) {
                        $scope.infoWindow.open($scope.map, $scope.mark);
                    }
                }

                // load the map - only first time for a tab map
                var loadMap = function (location, addressInfo, overrideZoom) {
                    lastLocationKey = getLocationKey();

                    var mapOptions = {
                        zoom: overrideZoom || zoom,
                        center: location,
                        scrollwheel: false
                    };

                    $scope.map = new google.maps.Map(document.getElementById($scope.mapId), mapOptions);
                    $scope.mark = new google.maps.Marker({
                        label: "L",
                        position: location,
                        map: $scope.map
                    });

                    $scope.infoWindow = new google.maps.InfoWindow({
                        position: location,
                        disableAutoPan: true,
                        content: addressInfo
                    });

                    $scope.mark.addListener("click", function () {
                        showInfo();
                    });

                    showInfo();
                }

                var updateMap = function (location, addressInfo, overrideZoom) {
                    // map update
                    $scope.map.getStreetView().setVisible(false);
                    $scope.map.setCenter(location);
                    $scope.map.setZoom(overrideZoom || zoom);
                    $scope.map.setMapTypeId(google.maps.MapTypeId.ROADMAP);

                    // mark update
                    $scope.mark.setPosition(location);

                    // info update
                    $scope.infoWindow.setContent(addressInfo);
                    showInfo();
                }

                var innerLocationChanged = function (location, fromLatLong, overrideHeading, overrideZoom) {
                    var addressInfo = buildAddressInfo(!fromLatLong, overrideHeading);
                    if (!$scope.map) {
                        loadMap(location, addressInfo, overrideZoom);
                    } else {
                        updateMap(location, addressInfo, overrideZoom);
                    }
                    log.debug("Gmap location changed to key ({0})".format(lastLocationKey));
                }

                var floatOrNull = function(value) {
                    var float = parseFloat(value);
                    return isNaN(float) ? null : float;
                }

                var locationChanged = function () {
                    var lat = floatOrNull($scope.lat);
                    var long = floatOrNull($scope.long);

                    if (lat && long) {
                        innerLocationChanged(new google.maps.LatLng(lat, long), true);
                        return;
                    }

                    if (!$scope.streetaddress) {
                        return;
                    }

                    var address = $scope.streetaddress;
                    var addressLineTwo = buildAddressLineTwo(true);
                    if (addressLineTwo) {
                        address += ", " + addressLineTwo;
                    }
                    $scope.geocoder.geocode({ address: address }, function (results, status) {
                        if (status === google.maps.GeocoderStatus.OK) {
                            innerLocationChanged(results[0].geometry.location, false);
                            return;
                        }
                        log.warn("Could not find address ({0})".format(address));
                        innerLocationChanged(new google.maps.LatLng(0, 0), false, "Could Not Find Location Address", 3);
                    });
                }

                const tabidChanged = function (newTabid) {
                    if (newTabid !== $scope.tabid) {
                        return;
                    }

                    // do not update map on every tab change
                    // just if the location is changed
                    const currentLocationKey = getLocationKey();
                    if (lastLocationKey === currentLocationKey) {
                        return;
                    }

                    lastLocationKey = currentLocationKey;

                    // ensures map is visible
                    $timeout(() => locationChanged());
                }

                // loads the gmaps api and loads default values from server configs
                log.debug("Loading gmap on tab ({0}), zoom ({1}), lat ({2}), long ({3}) and street address ({4}).".format($scope.tabid, zoom, $scope.lat, $scope.long, $scope.streetaddress));
                loadGoogleMapApi.then(function () {
                    defaultCity = configurationService.getConfigurationValue("/Global/Maps/DefaultCity");
                    defaultState = configurationService.getConfigurationValue("/Global/Maps/DefaultState");
                    defaultCountry = configurationService.getConfigurationValue("/Global/Maps/DefaultCountry");
                    log.debug("Default city ({0}), default state ({1}) and default country ({2}).".format(defaultCity, defaultState, defaultCountry));

                    $scope.geocoder = new google.maps.Geocoder();

                    locationChanged();

                    $rootScope.$on("sw4_activetabchanged", function (event, tabid) {
                        tabidChanged(tabid);
                    });

                }, error => {
                    // Promise rejected
                });
            }
        };
    }]);
})(angular);