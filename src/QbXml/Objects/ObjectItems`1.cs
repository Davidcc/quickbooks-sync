﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QbSync.QbXml.Objects
{
    internal class ObjectItems<U>
        where U : struct, IConvertible
    {
        private readonly object instance;
        private readonly string[] nameOrder;
        private readonly PropertyInfo itemsProperty;
        private readonly PropertyInfo itemsElementNameProperty;
        private readonly object[]? itemsValue;
        private readonly U[]? itemsElementNameValue;
        private List<ObjectItemValue> propertyList;

        public ObjectItems(object instance, string name, string[] nameOrder)
        {
            this.instance = instance;
            this.nameOrder = nameOrder;
            propertyList = new List<ObjectItemValue>();

            itemsProperty = instance.GetType().GetProperty(name) ?? throw new ArgumentException("The name must match to a property.", nameof(name));
            itemsElementNameProperty = instance.GetType().GetProperty(name + "ElementName") ?? throw new ArgumentException("The name must match to a property ending with \"ElementName\".", nameof(name));
            itemsValue = itemsProperty.GetValue(instance, null) as object[];
            itemsElementNameValue = itemsElementNameProperty.GetValue(instance, null) as U[];

            Initialize();
        }

        private void Initialize()
        {
            if (itemsValue != null && itemsElementNameValue != null)
            {
                for (var i = 0; i < itemsValue.Length; i++)
                {
                    propertyList.Add(new ObjectItemValue
                    {
                        Name = itemsElementNameValue[i].ToString(),
                        Value = itemsValue[i]
                    });
                }
            }
        }

        public void SetItem<T>(U name, T value)
        {
            RemoveItems(name);

            propertyList.Add(new ObjectItemValue
            {
                Name = name.ToString(),
                Value = value
            });

            SetItemsOnInstance();
        }

        public void SetItems<T>(U name, T[] values)
        {
            RemoveItems(name);

            for (var i = 0; i < values.Length; i++)
            {
                propertyList.Add(new ObjectItemValue
                {
                    Name = name.ToString(),
                    Value = values[i]
                });
            }

            SetItemsOnInstance();
        }

        public T GetItem<T>(U name)
        {
            return GetItems<T>(name).FirstOrDefault();
        }

        public IEnumerable<T> GetItems<T>(U name)
        {
            return propertyList.Where(m => m.Name == name.ToString()).Select(m => m.Value).Cast<T>();
        }

        private void RemoveItems(U name)
        {
            propertyList = propertyList.Where(m => m.Name != name.ToString()).ToList();
        }

        private void SetItemsOnInstance()
        {
            if (nameOrder != null)
            {
                propertyList = propertyList
                               .OrderBy(m => Array.FindIndex(nameOrder, n => n == m.Name))
                               .ToList();
            }

            var itemsValue = propertyList
                .Select(m => m.Value);

            if (itemsValue.Count() == 0)
            {
                itemsValue = null;
            }

            itemsProperty.SetValue(instance, itemsValue.ToArray(), null);

            var itemsElementNameValue = propertyList
                .Select(m => (U)Enum.Parse(typeof(U), m.Name!));

            if (itemsElementNameValue.Count() == 0)
            {
                itemsElementNameValue = null;
            }

            if (itemsElementNameValue != null)
            {
                itemsElementNameProperty.SetValue(instance, itemsElementNameValue.ToArray(), null);
            }
        }
    }
}
