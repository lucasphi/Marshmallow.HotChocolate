﻿using System;

namespace Marshmallow.HotChocolate.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class JoinAttribute : Attribute
    {
        public string ColumnName { get; private set; }

        public string TableName { get; private set; }

        public JoinAttribute(string columnName, string tableName)
        {
            ColumnName = columnName;
            TableName = tableName;
        }
    }
}
