﻿using System;

namespace MyOrm
{
    public class MyDbConfiguration
    {
        private static string _defaultConnectionString;

        private static string _prefix;

        private static bool _hasInit;

        public static void Init(string connectionString, string prefix = "@")
        {
            if (_hasInit)
            {
                throw new Exception("MyOrm只能初始化一次");
            }

            _defaultConnectionString = connectionString;
            _prefix = prefix;
            _hasInit = true;
        }

        public static string GetConnectionString()
        {
            return _defaultConnectionString;
        }

        public static string GetPrefix()
        {
            return _prefix;
        }
    }
}