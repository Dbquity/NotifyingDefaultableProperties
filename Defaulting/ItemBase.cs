﻿using Dbquity;
using Dbquity.Implementation;
using System;
using System.ComponentModel;

namespace Defaulting.TestItems {

    abstract class ItemBase : IPropertyOwner {
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
        void IImplementPropertyOwner.NotifyChanging(params string[] propertyNames) =>
            this.NotifyChanging(PropertyChanging, propertyNames);
        void IImplementPropertyOwner.NotifyChanged(params string[] propertyNames) =>
            this.NotifyChanged(PropertyChanged, propertyNames);
        public bool HasProperty(string propertyName) {
            switch (propertyName) {
                case nameof(Item.Label):
                case nameof(Item.Cost):
                case nameof(Item.Name):
                case nameof(Item.Price):
                case nameof(Item.LabelIsDefaulted):
                case nameof(Item.CostIsDefaulted):
                    return true;
            }
            return false;
        }
        public const string LabelDefault = "<label it>";
        public const Decimal CostDefault = 43L;
        public abstract object this[string propertyName] { get; set; }
    }
}