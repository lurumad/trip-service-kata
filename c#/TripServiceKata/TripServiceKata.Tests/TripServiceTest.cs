using System;
using System.Collections.Generic;
using TripServiceKata.Trip;
using Xunit;
using FluentAssertions;
using TripServiceKata.Exception;

namespace TripServiceKata.Tests
{
    public class trip_service_should
    {
        private static readonly User.User Anonimous = null;
        private static readonly User.User LoggedInUser = new User.User();
        private static readonly User.User AnotherUser = new User.User();
        private static readonly Trip.Trip ToSpain = new Trip.Trip();
        private readonly User.User loggedInUser;

        public trip_service_should()
        {
            loggedInUser = LoggedInUser;
        }

        [Fact]
        public void throws_exception_if_user_is_not_logged_in()
        {
            var tripService = TestableTripService.New(Anonimous, AnotherUser);
            Action action = () => tripService.GetTripsByUser(AnotherUser);
            action.ShouldThrow<UserNotLoggedInException>();
        }

        [Fact]
        public void not_returns_trips_when_users_are_not_friends()
        {
            var tripService = TestableTripService.New(loggedInUser, AnotherUser);
            var notFriend = UserBuilder
                .User()
                .WithFriends(new[] {AnotherUser})
            .Build();
            var trips = tripService.GetTripsByUser(notFriend);
            trips.Should().BeEmpty();
        }

        [Fact]
        public void returns_friend_trips_when_users_are_friends()
        {
            var friend = new User.User();
            friend.AddFriend(loggedInUser);
            friend.AddTrip(ToSpain);
            var tripService = TestableTripService.New(loggedInUser, friend);
            var trips = tripService.GetTripsByUser(friend);
            trips.Should().NotBeEmpty();
        }

        public class UserBuilder
        {
            private User.User [] friends = new User.User[0];

            private Trip.Trip [] trips = new Trip.Trip[0];

            public static UserBuilder User()
            {
                return new UserBuilder();
            }

            public User.User Build()
            {
                var user = new User.User();
                AddFriends(user);
                AddTrips(user);

                return user;
            }

            private void AddTrips(User.User user)
            {
                foreach (var trip in trips)
                {
                    user.AddTrip(trip);
                }
            }

            private void AddFriends(User.User user)
            {
                foreach (var friend in friends)
                {
                    user.AddFriend(friend);
                }
            }

            public UserBuilder WithFriends(User.User [] users)
            {
                this.friends = users;
                return this;
            }

            public UserBuilder WithTrips(Trip.Trip[] trips)
            {
                this.trips = trips;
                return this;
            }
        }

        internal class TestableTripService : TripService
        {
            private readonly User.User loggedInUser;
            private readonly User.User anotherUser;

            private TestableTripService(User.User loggedInUser, User.User anotherUser)
            {
                this.loggedInUser = loggedInUser;
                this.anotherUser = anotherUser;
            }

            public static TestableTripService New(User.User loggedInUser, User.User anotherUser)
            {
                return new TestableTripService(loggedInUser,anotherUser);
            }

            protected override User.User GetLoggedUser()
            {
                return loggedInUser;
            }

            protected override List<Trip.Trip> FindTripsByUser(User.User user)
            {
                return anotherUser.Trips();
            }
        }
    }
}
