using HZC.MyOrm.Commons;
using HZC.MyOrm.Reflections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace HZC.MyOrm.DbParameters
{
    public class MyDbParameters
    {
        private readonly List<MyDbParameter> _dict = new List<MyDbParameter>();

        private readonly string _prefix = "@";

        public MyDbParameters()
        { }

        public MyDbParameters(string prefix)
        {
            _prefix = prefix;
        }

        public SqlParameter[] Parameters
        {
            get
            {
                if (_dict.Count == 0)
                {
                    return new SqlParameter[]{};
                }

                var list = new List<SqlParameter>();
                foreach (var item in _dict)
                {
                    var param = new SqlParameter(item.Name, ResolveParameterValue(item.Value));
                    if (item.Direction != null)
                    {
                        param.Direction = item.Direction.Value;
                    }
                    list.Add(param);
                }

                return list.ToArray();
            }
        }

        public void Add(string parameterName, object value)
        {
            parameterName = parameterName.StartsWith(_prefix) ? parameterName : _prefix + parameterName;
            var item = _dict.FirstOrDefault(d => d.Name == parameterName);
            var newItem = new MyDbParameter {Name = parameterName, Value = ResolveParameterValue(value) };

            if (item == null)
            {
                _dict.Add(newItem);
            }
            else
            {
                item = newItem;
            }
        }

        public void Add(string parameterName, object value, ParameterDirection direction)
        {
            parameterName = parameterName.StartsWith(_prefix) ? parameterName : _prefix + parameterName;
            var item = _dict.FirstOrDefault(d => d.Name == parameterName);
            var newItem = new MyDbParameter { Name = parameterName, Value = ResolveParameterValue(value), Direction = direction};

            if (item == null)
            {
                _dict.Add(newItem);
            }
            else
            {
                item = newItem;
            }
        }

        public void Add(object obj)
        {
            if (obj is MyDbParameter parameter)
            {
                _dict.Add(parameter);
            }
            else if(obj is IEntity)
            {
                var entityInfo = MyEntityContainer.Get(obj.GetType());
                foreach(var property in entityInfo.Properties.Where(p => p.IsMap))
                {
                    Add(_prefix + property.Name, property.PropertyInfo.GetValue(obj));
                }
            }
            else
            {
                var properties = obj.GetType().GetProperties();
                foreach (var property in properties.Where(p => p.PropertyType.IsValueType || 
                                                               p.PropertyType == typeof(string)))
                {
                    Add(_prefix + property.Name, ResolveParameterValue(property.GetValue(obj)));
                }
            }
        }

        public void AddParameters(MyDbParameters parameters)
        {
            foreach (var item in parameters._dict)
            {
                Add(item);
            }
        }

        public void Add(SqlParameter parameter)
        {
            Add(parameter.ParameterName, ResolveParameterValue(parameter.Value));
        }

        public void Add(SqlParameter[] parameters)
        {
            foreach(var item in parameters)
            {
                Add(item);
            }
        }

        private object ResolveParameterValue(object val)
        {
            if(val == null)
            {
                return DBNull.Value;
            }
            return val;
        }
    }
}
