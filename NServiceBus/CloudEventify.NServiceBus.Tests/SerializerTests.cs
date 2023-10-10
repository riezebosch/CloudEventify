using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using FluentAssertions;

namespace CloudEventify.NServiceBus.Tests;

public class SerializerTests
{
    public class Serialize
    {
        [Fact]
        public async Task PoCoToCloudEventTypeSerializationWithMapper()
        {
            //Arrange
            var mapper = new Mapper();
            mapper.Map<A.UserLoggedIn>("UserLoggedIn");
            var jsonOptions = CloudEventExtensions.CloudEventStandardJsonSerializerOptions;
            var sud = new Serializer(mapper, jsonOptions);
            var memoryStream = new MemoryStream();

            //Act
            sud.Serialize(new A.UserLoggedIn(1234), memoryStream);

            //Assert
            memoryStream.Position = 0;
            var cloudEvent = await new JsonEventFormatter().DecodeStructuredModeMessageAsync(memoryStream, null, null);
            var data = ((JsonElement)cloudEvent.Data!).Deserialize<A.UserLoggedIn>(jsonOptions);
            Assert.NotNull(data);
            data.Id.Should().Be(1234);
            cloudEvent.Type.Should().Be("UserLoggedIn");
        }

        [Fact]
        public async Task PoCoToCloudEventTypeSerializationNoMapping()
        {
            //Arrange
            var jsonOptions = CloudEventExtensions.CloudEventStandardJsonSerializerOptions;
            var sud = new Serializer(null, jsonOptions);
            var memoryStream = new MemoryStream();

            //Act
            sud.Serialize(new A.UserLoggedIn(1234), memoryStream);

            //Assert
            memoryStream.Position = 0;
            var cloudEvent = await new JsonEventFormatter().DecodeStructuredModeMessageAsync(memoryStream, null, null);
            var data = ((JsonElement)cloudEvent.Data!).Deserialize<A.UserLoggedIn>(jsonOptions);
            Assert.NotNull(data);
            data.Id.Should().Be(1234);
            cloudEvent.Type.Should().Be(typeof(A.UserLoggedIn).FullName);
        }

        [Fact]
        public async Task CloudEventToCloudEventTypeSerialization_ShouldBeUnModifiedWhenTypeSet()
        {
            //Arrange
            var mapper = new Mapper();
            mapper.Map<A.UserLoggedIn>("UserLoggedIn");
            var jsonOptions = CloudEventExtensions.CloudEventStandardJsonSerializerOptions;
            var sud = new Serializer(mapper, jsonOptions);
            var message = A.GetLoginEvent("CustomType", 12345);

            //Act
            var memoryStream = new MemoryStream();
            sud.Serialize(message, memoryStream);
            //Assert
            await A.VerifyLoggedInUserCloudEvent(memoryStream, message, "CustomType", jsonOptions, 12345);
        }

        [Fact]
        public async Task CloudEventToCloudEventTypeSerialization_ShouldBeUnModifiedAndTypeMapped()
        {
            //Arrange
            var mapper = new Mapper();
            mapper.Map<A.UserLoggedIn>("UserLoggedIn");
            var jsonOptions = CloudEventExtensions.CloudEventStandardJsonSerializerOptions;
            var sud = new Serializer(mapper, jsonOptions);
            var memoryStream = new MemoryStream();
            var message = A.GetLoginEvent("UserLoggedIn", 12345);

            //Act
            sud.Serialize(message, memoryStream);

            //Assert
            await A.VerifyLoggedInUserCloudEvent(memoryStream, message, "UserLoggedIn", jsonOptions, 12345);
        }
    }

    public class Deserialize
    {
        [Fact]
        public void ToPoCoUsingMapper()
        {
            //Arrange
            var mapper = new Mapper();
            mapper.Map<A.UserLoggedIn>("MappedUserLoggedIn");
            var jsonOptions = CloudEventExtensions.CloudEventStandardJsonSerializerOptions;
            var sud = new Serializer(mapper, jsonOptions);

            var cloudEvent = A.GetLoginEvent("MappedUserLoggedIn");
            cloudEvent.Type.Should().Be("MappedUserLoggedIn");
            var encodedCloudEvent = Formatter.New(jsonOptions).Encode(cloudEvent);

            //Act
            var deserializedObjects = sud.Deserialize(new ReadOnlyMemory<byte>(encodedCloudEvent), new List<Type>());
            var deserializedObject = deserializedObjects.FirstOrDefault();

            //Assert
            Assert.NotNull(deserializedObject);
            deserializedObject.Should().BeOfType<A.UserLoggedIn>();
            var userLoggedIn = (A.UserLoggedIn)deserializedObject;
            userLoggedIn.Id.Should().Be(1234);
        }

        [Fact]
        public void ToPoCoUsingMessageTypeList()
        {
            //Arrange
            var jsonOptions = CloudEventExtensions.CloudEventStandardJsonSerializerOptions;
            var sud = new Serializer(null, jsonOptions);

            var cloudEvent = A.GetLoginEvent(typeof(A.UserLoggedIn).FullName!);
            var encodedCloudEvent = Formatter.New(jsonOptions).Encode(cloudEvent);

            //Act
            var deserializedObjects = sud.Deserialize(new ReadOnlyMemory<byte>(encodedCloudEvent),
                new List<Type>(new[] { typeof(A.UserLoggedIn) }));
            var deserializedObject = deserializedObjects.FirstOrDefault();

            //Assert
            Assert.NotNull(deserializedObject);
            deserializedObject.Should().BeOfType<A.UserLoggedIn>();
            var userLoggedIn = (A.UserLoggedIn)deserializedObject;
            userLoggedIn.Id.Should().Be(1234);
        }

        [Fact]
        public void ToPoCoWithoutMatchingMapThrows()
        {
            //Arrange
            var mapper = new Mapper();
            mapper.Map<A.UserLoggedIn>("MappedUserLoggedIn");
            var jsonOptions = CloudEventExtensions.CloudEventStandardJsonSerializerOptions;
            var sud = new Serializer(mapper, jsonOptions);

            var cloudEvent = A.GetLoginEvent("MyOwnMessageType");
            var encodedCloudEvent = Formatter.New(jsonOptions).Encode(cloudEvent);

            //Act / Assert
            sud.Invoking(y => y.Deserialize(new ReadOnlyMemory<byte>(encodedCloudEvent),
                    new List<Type>(new[] { typeof(A.UserLoggedIn) })))
                .Should().Throw<ArgumentException>()
                .WithMessage("Could not find any type-mapping for*");
        }

        [Fact]
        public void ToCloudEventWrappedUsingMapper()
        {
            //Arrange
            var mapper = new Mapper();
            mapper.Map<B.UserLoggedIn>("MappedUserLoggedIn");
            var jsonOptions = CloudEventExtensions.CloudEventStandardJsonSerializerOptions;
            var sud = new Serializer(mapper, jsonOptions);

            var cloudEvent = A.GetLoginEvent("MappedUserLoggedIn", 5432);
            cloudEvent.Type.Should().Be("MappedUserLoggedIn");
            var encodedCloudEvent = Formatter.New(jsonOptions).Encode(cloudEvent);

            //Act
            var deserializedObjects = sud.Deserialize(new ReadOnlyMemory<byte>(encodedCloudEvent), new List<Type>());
            var deserializedObject = deserializedObjects.FirstOrDefault();

            //Assert
            B.VerifyCloudEvent(deserializedObject, cloudEvent, 5432);
        }

        [Fact]
        public void ToCloudEventWrappedUsingTypedList()
        {
            //Arrange
            var jsonOptions = CloudEventExtensions.CloudEventStandardJsonSerializerOptions;
            var sud = new Serializer(null, jsonOptions);

            var cloudEvent = A.GetLoginEvent(typeof(B.UserLoggedIn).FullName!, 5432);
            var encodedCloudEvent = Formatter.New(jsonOptions).Encode(cloudEvent);

            //Act
            var deserializedObjects = sud.Deserialize(new ReadOnlyMemory<byte>(encodedCloudEvent),
                new List<Type>(new[] { typeof(B.UserLoggedIn) }));
            var deserializedObject = deserializedObjects.FirstOrDefault();

            //Assert
            B.VerifyCloudEvent(deserializedObject, cloudEvent, 5432);
        }

        [Fact]
        public void ToCloudEventWrappedWithoutMatchingMapThrows()
        {
            //Arrange
            var mapper = new Mapper();
            mapper.Map<B.UserLoggedIn>("MappedUserLoggedIn");
            var jsonOptions = CloudEventExtensions.CloudEventStandardJsonSerializerOptions;
            var sud = new Serializer(mapper, jsonOptions);

            var cloudEvent = A.GetLoginEvent("NoExistingType", 5432);
            cloudEvent.Type.Should().Be("NoExistingType");
            var encodedCloudEvent = Formatter.New(jsonOptions).Encode(cloudEvent);

            //Act / Assert
            sud.Invoking(y => y.Deserialize(new ReadOnlyMemory<byte>(encodedCloudEvent),
                    new List<Type>(new[] { typeof(A.UserLoggedIn) })))
                .Should().Throw<ArgumentException>()
                .WithMessage("Could not find any type-mapping for*");
        }

        [Fact]
        public void ToCloudEventUsingMapper()
        {
            //Arrange
            var mapper = new Mapper();
            mapper.Map<CloudEvent>("MappedUserLoggedIn");
            var jsonOptions = CloudEventExtensions.CloudEventStandardJsonSerializerOptions;
            var sud = new Serializer(mapper, jsonOptions);

            var cloudEvent = A.GetLoginEvent("MappedUserLoggedIn", 13234);
            var encodedCloudEvent = Formatter.New(jsonOptions).Encode(cloudEvent);

            //Act
            var deserializedObjects = sud.Deserialize(new ReadOnlyMemory<byte>(encodedCloudEvent), new List<Type>());
            var deserializedObject = deserializedObjects.FirstOrDefault() as CloudEvent;

            //Assert
            A.VerifyLoggedInUserCloudEvent(deserializedObject, cloudEvent, jsonOptions, 13234);
        }

        [Fact]
        public void ToCloudEventUsingMapperWithMultipleOptions()
        {
            //Arrange
            var mapper = new Mapper();
            mapper.Map<CloudEvent>("MappedUserLoggedIn");
            mapper.Map<CloudEvent>("MappedUserLoggedOut");
            var jsonOptions = CloudEventExtensions.CloudEventStandardJsonSerializerOptions;
            var sud = new Serializer(mapper, jsonOptions);

            var cloudEvent = A.GetLoginEvent("MappedUserLoggedIn", 13234);
            var cloudEvent2 = A.GetLogoutEvent("MappedUserLoggedOut", 13234);
            var encodedCloudEvent = Formatter.New(jsonOptions).Encode(cloudEvent);
            var encodedCloudEvent2 = Formatter.New(jsonOptions).Encode(cloudEvent2);

            //Act
            var deserializedObject =
                sud.Deserialize(encodedCloudEvent, new List<Type>()).FirstOrDefault() as CloudEvent;
            var deserializedObject2 =
                sud.Deserialize(encodedCloudEvent2, new List<Type>()).FirstOrDefault() as CloudEvent;

            //Assert
            A.VerifyLoggedInUserCloudEvent(deserializedObject, cloudEvent, jsonOptions, 13234);
            A.VerifyLoggedOutUserCloudEvent(deserializedObject2, cloudEvent2, jsonOptions, 13234);
        }
    }

    public static class A
    {
        public static CloudEvent GetLoginEvent(string type = "UserLoggedIn", int id = 1234)
        {
            return GetEvent(type, new UserLoggedIn(id));
        }

        public static async Task VerifyLoggedInUserCloudEvent(MemoryStream target, CloudEvent source,
            string mappedType,
            JsonSerializerOptions jsonOptions, int id = 1234)
        {
            target.Position = 0;
            var cloudEvent = await new JsonEventFormatter().DecodeStructuredModeMessageAsync(target, null, null);
            cloudEvent.Type.Should().Be(mappedType);
            cloudEvent.DataContentType.Should().Be(source.DataContentType);
            cloudEvent.Time.Should().Be(source.Time);
            cloudEvent.Subject.Should().Be(source.Subject);
            cloudEvent.DataSchema.Should().Be(source.DataSchema);
            cloudEvent.Source.Should().Be(source.Source);
            cloudEvent.Id.Should().Be(source.Id);
            var data = ((JsonElement)cloudEvent.Data!).Deserialize<UserLoggedIn>(jsonOptions);
            Assert.NotNull(data);
            data.Id.Should().Be(id);
        }

        public static void VerifyLoggedInUserCloudEvent(CloudEvent? target, CloudEvent source,
            JsonSerializerOptions jsonOptions, int id = 1234)
        {
            Assert.NotNull(target);
            target.Id.Should().Be(source.Id);
            target.Source.Should().Be(source.Source);
            target.DataSchema.Should().Be(source.DataSchema);
            target.DataContentType.Should().Be(source.DataContentType);
            target.Time.Should().Be(source.Time);
            target.Type.Should().Be(source.Type);
            target.Subject.Should().Be(source.Subject);
            var data = ((JsonElement)target.Data!).Deserialize<UserLoggedIn>(jsonOptions);
            Assert.NotNull(data);
            data.Id.Should().Be(id);
        }


        public static CloudEvent GetLogoutEvent(string type = "UserLoggedOut", int id = 1234)
        {
            return GetEvent(type, new UserLoggedOut(id, DateTimeOffset.UtcNow));
        }

        public static void VerifyLoggedOutUserCloudEvent(CloudEvent? deserializedObject, CloudEvent cloudEvent,
            JsonSerializerOptions jsonOptions, int id = 1234)
        {
            Assert.NotNull(deserializedObject);
            deserializedObject.Id.Should().Be(cloudEvent.Id);
            deserializedObject.Source.Should().Be(cloudEvent.Source);
            deserializedObject.DataSchema.Should().Be(cloudEvent.DataSchema);
            deserializedObject.DataContentType.Should().Be(cloudEvent.DataContentType);
            deserializedObject.Time.Should().Be(cloudEvent.Time);
            deserializedObject.Type.Should().Be(cloudEvent.Type);
            deserializedObject.Subject.Should().Be(cloudEvent.Subject);
            var data = ((JsonElement)deserializedObject.Data!).Deserialize<UserLoggedOut>(jsonOptions);
            Assert.NotNull(data);
            data.Id.Should().Be(id);
            data.Time.Should().BeBefore(DateTimeOffset.UtcNow);
        }

        private static CloudEvent GetEvent(string type, object data)
        {
            return new CloudEvent(CloudEventsSpecVersion.V1_0)
            {
                Id = Guid.NewGuid().ToString(),
                Source = new Uri("http://localhost"),
                DataSchema = new Uri("ns://my.data.schema"),
                Subject = "subjectos",
                Time = DateTimeOffset.UtcNow,
                Type = type,
                Data = data,
                DataContentType = "application/json"
            };
        }

        public record UserLoggedIn(int Id);

        public record UserLoggedOut(int Id, DateTimeOffset Time);
    }

    public static class B
    {
        public static void VerifyCloudEvent(object? target, CloudEvent source,
            int id)
        {
            Assert.NotNull(target);
            target.Should().BeOfType<UserLoggedIn>();
            var userLoggedIn = (UserLoggedIn)target;
            userLoggedIn.Id.Should().Be(source.Id);
            userLoggedIn.Source.Should().Be(source.Source);
            userLoggedIn.DataSchema.Should().Be(source.DataSchema);
            userLoggedIn.DataContentType.Should().Be(source.DataContentType);
            userLoggedIn.Time.Should().Be(source.Time);
            userLoggedIn.Type.Should().Be(source.Type);
            userLoggedIn.Subject.Should().Be(source.Subject);
            userLoggedIn.Data!.Id.Should().Be(id);
        }

        public class UserLoggedIn : CloudEvent<A.UserLoggedIn>
        {
            public UserLoggedIn(CloudEvent cloudEvent) : base(cloudEvent)
            {
            }

            public UserLoggedIn(A.UserLoggedIn userLoggedIn) : base(userLoggedIn)
            {
                Subject = $"user/{userLoggedIn.Id}/loggedIn";
            }
        }
    }
}