﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GraphQLinq.Tests
{
    [TestFixture]
    class IntegrationTests
    {
        List<LocationType> locationTypes = new List<LocationType> { LocationType.STORE, LocationType.SERVICE };
        SuperChargersGraphContext superChargersContext = new SuperChargersGraphContext("https://www.superchargers.io/graphql");

        HslGraphContext hslGraphContext = new HslGraphContext("https://api.digitransit.fi/routing/v1/routers/finland/index/graphql");
        const string TripId = "HSL:1055_20170501_To_1_1205";

        [Test]
        public void SelectingCitiesReturnsListOfCities()
        {
            var query = superChargersContext.Locations(type: locationTypes).Select(l => l.city);

            var locations = query.ToList();

            CollectionAssert.IsNotEmpty(locations);
            CollectionAssert.AllItemsAreNotNull(locations);
        }

        [Test]
        public void SelectingLocationsDoesNotReturnPhones()
        {
            var query = superChargersContext.Locations(type: locationTypes);

            var locations = query.ToList();

            var locationsWithPhones = locations.Where(location => location.salesPhone != null).ToList();
            CollectionAssert.IsEmpty(locationsWithPhones);
        }

        [Test]
        public void SelectingLocationsAndIncludingPhonesReturnsPhones()
        {
            var query = superChargersContext.Locations(type: locationTypes).Include(location => location.salesPhone);

            var locations = query.ToList();

            var locationsWithNullPhones = locations.Where(location => location.salesPhone == null).ToList();
            CollectionAssert.IsEmpty(locationsWithNullPhones);
        }

        [Test]
        public void SelectingCitiesAndPhonesReturnsPhones()
        {
            var query = superChargersContext.Locations(type: locationTypes).Select(location => new { location.city, location.salesPhone });

            var locations = query.ToList();

            var locationsWithNullPhones = locations.Where(location => location.salesPhone == null).ToList();
            CollectionAssert.IsEmpty(locationsWithNullPhones);
        }

        [Test]
        public void SelectingSingleTripIdIsNotNull()
        {
            var tripId = hslGraphContext.Trip(TripId).Select(t => t.gtfsId).ToItem();

            Assert.That(tripId, Is.Not.Null);
        }

        [Test]
        public void SelectingNestedPropertiesOfSingleTripNestedPropertiesAreNotNull()
        {
            var item = hslGraphContext.Trip(TripId).Select(trip =>
                new
                {
                    tg = trip.gtfsId,
                    aatrg = trip.route.gtfsId,
                    trip.pattern.geometry,
                    n = trip.route.agency.name,
                    p = trip.route.agency.phone
                }
            ).ToItem();

            Assert.Multiple(() =>
            {
                Assert.That(item.aatrg, Is.Not.Null);
                Assert.That(item.geometry, Is.Not.Null);
                Assert.That(item.n, Is.Not.Null);
                Assert.That(item.p, Is.Not.Null);
            });
        }

        [Test]
        public void SelectingThreeLevelNestedPropertyOfSingleTripNestedPropertyIsNotNull()
        {
            var routes = hslGraphContext.Trip(TripId).Select(trip => trip.route.agency.routes).ToItem();

            CollectionAssert.IsNotEmpty(routes);
        }

        [Test]
        public void SelectingSingleTripNestedPropertyIsNull()
        {
            var pattern = hslGraphContext.Trip(TripId).ToItem();

            Assert.That(pattern.pattern, Is.Null);
        }
    }
}
