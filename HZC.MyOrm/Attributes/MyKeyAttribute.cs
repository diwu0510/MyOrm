﻿using System;

namespace HZC.MyOrm.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MyKeyAttribute : Attribute
    {
        public bool IsIncrement { get; set; } = true;

        public string FieldName { get; set; }
    }
}
