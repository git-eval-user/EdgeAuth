﻿using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace PhenixRTS.EdgeAuth
{
    /// <summary>
    /// Token builder helper class to create digest tokens that can be used with the Phenix platform.
    /// </summary>
    public sealed class TokenBuilder
    {
        public static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private const string FIELD_TYPE = "type";
        private const string FIELD_SESSION_ID = "sessionId";
        private const string FIELD_REMOTE_ADDRESS_ID = "remoteAddress";
        private const string FIELD_ORIGIN_STREAM_ID = "originStreamId";
        private const string FIELD_REQUIRED_TAG = "requiredTag";
        private const string FIELD_APPLY_TAGS = "applyTags";
        private const string FIELD_CAPABILITIES = "capabilities";

        private string _applicationId;
        private string _secret;
        private readonly JObject _tokenBuilder;
        private JArray _capabilitiesBuilder;
        private JArray _tagBuilder;

        /// <summary>
        /// Token Builder Constructor.
        /// </summary>
        public TokenBuilder()
        {
            _tokenBuilder = new JObject();
        }

        /// <summary>
        /// The application ID used to sign the token (required).
        /// </summary>
        /// <param name="uri">The application ID to sign the token</param>
        /// <returns>Itself</returns>
        public TokenBuilder WithUri(string uri)
        {
            if (uri == null)
            {
                throw new Exception("URI must not be null");
            }

            _tokenBuilder.Add(DigestTokens.FIELD_URI, uri);

            return this;
        }

        /// <summary>
        /// The application ID used to sign the token (required).
        /// </summary>
        /// <param name="applicationId">The application ID to sign the token</param>
        /// <returns>Itself</returns>
        public TokenBuilder WithApplicationId(string applicationId)
        {
            if (applicationId == null)
            {
                throw new Exception("Application ID must not be null");
            }

            _applicationId = applicationId;

            return this;
        }

        /// <summary>
        /// The secret used to sign the token (required).
        /// </summary>
        /// <param name="secret">The shared secret to sign the token</param>
        /// <returns>Itself</returns>
        public TokenBuilder WithSecret(string secret)
        {
            if (secret == null)
            {
                throw new Exception("Secret must not be null");
            }

            _secret = secret;

            return this;
        }

        /// <summary>
        /// Set a capability for the token, e.g. to publish a stream. (optional)
        /// </summary>
        /// <param name="capability">A valid capability</param>
        /// <returns>Itself</returns>
        public TokenBuilder WithCapability(string capability)
        {
            if (capability == null)
            {
                throw new Exception("Capability must not be null");
            }

            if (_capabilitiesBuilder == null)
            {
                _capabilitiesBuilder = new JArray();
            }

            _capabilitiesBuilder.Add(capability);

            return this;
        }

        /// <summary>
        /// Expires the token in the given time. NOTE: Your time must be synced with the atomic clock for expiration time to work properly.
        /// </summary>
        /// <param name="seconds">The time in seconds</param>
        /// <returns>Itself</returns>
        public TokenBuilder ExpiresInSeconds(long seconds)
        {
            _tokenBuilder.Add(DigestTokens.FIELD_EXPIRES, (long)DateTime.UtcNow.Subtract(TokenBuilder.UNIX_EPOCH).TotalMilliseconds + (seconds * 1000));

            return this;
        }

        /// <summary>
        /// Expires the token at the given date. NOTE: Your time must be synced with the atomic clock for expiration time to work properly.
        /// </summary>
        /// <param name="expirationDate">The expiration date</param>
        /// <returns>Itself</returns>
        public TokenBuilder ExpiresAt(DateTime expirationDate)
        {
            _tokenBuilder.Add(DigestTokens.FIELD_EXPIRES, (long)expirationDate.Subtract(TokenBuilder.UNIX_EPOCH).TotalMilliseconds);

            return this;
        }

        /// <summary>
        /// Limit the token to authentication only. (optional)
        /// </summary>
        /// <returns>Itself</returns>
        public TokenBuilder ForAuthenticationOnly()
        {
            _tokenBuilder.Add(FIELD_TYPE, "auth");

            return this;
        }

        /// <summary>
        /// Limit the token to streaming only. (optional)
        /// </summary>
        /// <returns>Itself</returns>
        public TokenBuilder ForStreamingOnly()
        {
            _tokenBuilder.Add(FIELD_TYPE, "stream");

            return this;
        }

        /// <summary>
        /// Limit the token to publishing only. (optional)
        /// </summary>
        /// <returns>Itself</returns>
        public TokenBuilder ForPublishingOnly()
        {
            _tokenBuilder.Add(FIELD_TYPE, "publish");

            return this;
        }

        /// <summary>
        /// Limit the token to the specified session ID. (optional)
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        /// <returns>Itself</returns>
        public TokenBuilder ForSession(string sessionId)
        {
            if (sessionId == null)
            {
                throw new Exception("Session ID must not be null");
            }

            _tokenBuilder.Add(FIELD_SESSION_ID, sessionId);

            return this;
        }

        /// <summary>
        /// Limit the token to the specified remote address. (optional)
        /// </summary>
        /// <param name="remoteAddress">The remote address</param>
        /// <returns>Itself</returns>
        public TokenBuilder ForRemoteAddress(string remoteAddress)
        {
            if (remoteAddress == null)
            {
                throw new Exception("Remote address must not be null");
            }

            _tokenBuilder.Add(FIELD_REMOTE_ADDRESS_ID, remoteAddress);

            return this;
        }

        /// <summary>
        /// Limit the token to the specified remote address. (optional)
        /// </summary>
        /// <param name="remoteAddress">The remote address</param>
        /// <returns>Itself</returns>
        public TokenBuilder ForRemoteAddress(IPAddress remoteAddress)
        {
            if (remoteAddress == null)
            {
                throw new Exception("Remote address must not be null");
            }

            return ForRemoteAddress(remoteAddress.ToString());
        }

        /// <summary>
        /// Limit the token to the specified origin stream ID. (optional)
        /// </summary>
        /// <param name="originStreamId">The origin stream ID</param>
        /// <returns>Itself</returns>
        public TokenBuilder ForOriginStream(string originStreamId)
        {
            if (originStreamId == null)
            {
                throw new Exception("Origin Stream ID must not be null");
            }

            _tokenBuilder.Add(FIELD_ORIGIN_STREAM_ID, originStreamId);

            return this;
        }

        /// <summary>
        /// Limit the token to the specified channel ID. (optional)
        /// </summary>
        /// <param name="channelId">The channel ID</param>
        /// <returns>Itself</returns>
        public TokenBuilder ForChannel(string channelId)
        {
            if (channelId == null)
            {
                throw new Exception("Channel ID must not be null");
            }

            return ForTag("channelId:" + channelId);
        }

        /// <summary>
        /// Limit the token to the specified channel alias. (optional)
        /// </summary>
        /// <param name="channelAlias">The channel alias</param>
        /// <returns>Itself</returns>
        public TokenBuilder ForChannelAlias(string channelAlias)
        {
            if (channelAlias == null)
            {
                throw new Exception("Channel alias must not be null");
            }

            return ForTag("channelAlias:" + channelAlias);
        }

        /// <summary>
        /// Limit the token to the specified room ID. (optional)
        /// </summary>
        /// <param name="roomId">The room ID</param>
        /// <returns>Itself</returns>
        public TokenBuilder ForRoom(string roomId)
        {
            if (roomId == null)
            {
                throw new Exception("Room ID must not be null");
            }

            return ForTag("roomId:" + roomId);
        }

        /// <summary>
        /// Limit the token to the specified room alias. (optional)
        /// </summary>
        /// <param name="roomAlias">The room alias</param>
        /// <returns>Itself</returns>
        public TokenBuilder ForRoomAlias(string roomAlias)
        {
            if (roomAlias == null)
            {
                throw new Exception("Room alias must not be null");
            }

            return ForTag("roomAlias:" + roomAlias);
        }

        /// <summary>
        /// Limit the token to the specified tag on the origin stream. (optional)
        /// </summary>
        /// <param name="tag">The tag required on the origin stream</param>
        /// <returns>Itself</returns>
        public TokenBuilder ForTag(string tag)
        {
            if (tag == null)
            {
                throw new Exception("Tag must not be null");
            }

            _tokenBuilder.Add(FIELD_REQUIRED_TAG, tag);

            return this;
        }

        /// <summary>
        /// Apply the tag to the stream when it is setup. (optional)
        /// </summary>
        /// <param name="tag">The tag added to the new stream</param>
        /// <returns>Itself</returns>
        public TokenBuilder ApplyTag(string tag)
        {
            if (tag == null)
            {
                throw new Exception("Tag must not be null");
            }

            if (_tagBuilder == null)
            {
                _tagBuilder = new JArray();
            }

            _tagBuilder.Add(tag);

            return this;
        }

        public string GetValue()
        {
            return _tokenBuilder.ToString();
        }

        /// <summary>
        /// Build the signed token.
        /// </summary>
        /// <returns>The signed token that can be used with the Phenix platform</returns>
        public string Build()
        {
            DigestTokens digestTokens = new DigestTokens();

            if (_capabilitiesBuilder != null)
            {
                _tokenBuilder.Add(FIELD_CAPABILITIES, _capabilitiesBuilder);
            }

            if (_tagBuilder != null)
            {
                _tokenBuilder.Add(FIELD_APPLY_TAGS, _tagBuilder);
            }

            return digestTokens.SignAndEncode(_applicationId, _secret, _tokenBuilder);
        }
    }
}