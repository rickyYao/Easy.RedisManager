﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Easy.Common;
using Easy.RedisManager.Entity.Redis;
using Easy.RedisManager.RedisAccess.Communication;

namespace Easy.RedisManager.RedisAccess
{
    public static class RedisExtensions
    {
        public static List<RedisEndpoint> ToRedisEndPoints(this IEnumerable<string> hosts)
        {
            if (hosts == null) return new List<RedisEndpoint>();

            var redisEndpoints = new List<RedisEndpoint>();
            foreach (var host in hosts)
            {
                RedisEndpoint endpoint;
                string[] hostParts;
                if (host.Contains("@"))
                {
                    hostParts = host.SplitOnLast('@');
                    var password = hostParts[0];
                    hostParts = hostParts[1].Split(':');
                    endpoint = GetRedisEndPoint(hostParts);
                    endpoint.Password = password;
                }
                else
                {
                    hostParts = host.Split(':');
                    endpoint = GetRedisEndPoint(hostParts);
                }
                redisEndpoints.Add(endpoint);
            }
            return redisEndpoints;
        }

        private static RedisEndpoint GetRedisEndPoint(string[] hostParts)
        {
            const int hostOrIpAddressIndex = 0;
            const int portIndex = 1;

            if (hostParts.Length == 0)
                throw new ArgumentException("'{0}' is not a valid Host or IP Address: e.g. '127.0.0.0[:11211]'");

            var port = (hostParts.Length == 1)
                           ? RedisSocketClient.DefaultPort
                           : Int32.Parse(hostParts[portIndex]);

            return new RedisEndpoint(hostParts[hostOrIpAddressIndex], port);
        }
    }

    internal static class RedisExtensionsInternal
    {
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }


        public static string[] GetIds(this IHasStringId[] itemsWithId)
        {
            var ids = new string[itemsWithId.Length];
            for (var i = 0; i < itemsWithId.Length; i++)
            {
                ids[i] = itemsWithId[i].Id;
            }
            return ids;
        }

        public static List<string> ToStringList(this byte[][] multiDataList)
        {
            if (multiDataList == null)
                return new List<string>();

            var results = new List<string>();
            foreach (var multiData in multiDataList)
            {
                results.Add(multiData.FromUtf8Bytes());
            }
            return results;
        }

        private static readonly NumberFormatInfo DoubleFormatProvider = new NumberFormatInfo
        {
            PositiveInfinitySymbol = "+inf",
            NegativeInfinitySymbol = "-inf"
        };

        public static byte[] ToFastUtf8Bytes(this double value)
        {
            return FastToUtf8Bytes(value.ToString("R", DoubleFormatProvider));
        }

        private static byte[] FastToUtf8Bytes(string strVal)
        {
            var bytes = new byte[strVal.Length];
            for (var i = 0; i < strVal.Length; i++)
                bytes[i] = (byte)strVal[i];

            return bytes;
        }

        public static byte[][] ToMultiByteArray(this string[] args)
        {
            var byteArgs = new byte[args.Length][];
            for (var i = 0; i < args.Length; ++i)
                byteArgs[i] = args[i].ToUtf8Bytes();
            return byteArgs;
        }

        public static byte[][] PrependByteArray(this byte[][] args, byte[] valueToPrepend)
        {
            var newArgs = new byte[args.Length + 1][];
            newArgs[0] = valueToPrepend;
            var i = 1;
            foreach (var arg in args)
                newArgs[i++] = arg;

            return newArgs;
        }
        public static byte[][] PrependInt(this byte[][] args, int valueToPrepend)
        {
            return args.PrependByteArray(valueToPrepend.ToUtf8Bytes());
        }
    }
}
