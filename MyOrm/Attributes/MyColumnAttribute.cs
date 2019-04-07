﻿using System;

namespace MyOrm.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class MyColumnAttribute : Attribute
    {
        /// <summary>
        /// 对应数据表中的字段名
        /// </summary>
        public string ColumnName { get; set; }

        public bool Ignore { get; set; }

        public bool InsertIgnore { get; set; }

        public bool UpdateIgnore { get; set; }

        public bool IsMap { get; set; } = true;
    }
}
