using Marshmallow.HotChocolate.Core.Attributes;
using Marshmallow.HotChocolate.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Marshmallow.HotChocolate.Core
{
    public class Transformer
    {
        /// <summary>
        /// Maps a source object to a target.
        /// </summary>
        /// <typeparam name="TDestiny">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        public static TDestiny Transform<TDestiny>(object source)
            where TDestiny : class, new()
        {
            if (source == null)
                return null;

            var transformer = new Transformer();
            return transformer.InjectFrom(typeof(TDestiny), source) as TDestiny;
        }

        protected Transformer()
        { }

        #region Injection Handler
        /// <summary>
        /// Maps a source object to a target type.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <param name="source">The source object.</param>
        /// <returns>The mapped object.</returns>
        protected virtual object InjectFrom(Type targetType, object source)
        {
            object target;

            // Checks to see if its mapping a list
            if (targetType.IsGenericCollection())
            {
                if (targetType.IsInterface)
                {
                    targetType = typeof(List<>).MakeGenericType(targetType.GenericTypeArguments.First());
                }

                target = Activator.CreateInstance(targetType);
                InjectCollection(source, target, targetType);
            }
            else
            {
                target = Activator.CreateInstance(targetType);
                InjectMember(source, target, targetType);
            }

            return target;
        }
        #endregion

        #region Property Injecter
        /// <summary>
        /// Maps a property.
        /// </summary>
        protected virtual void InjectMember(object source, object target, Type targetType)
        {
            var propertyLookup = new PropertyLookup(targetType);
            var properties = propertyLookup.GetAllProperties();
            InjectMemberProperties(source, target, properties);
        }

        private void InjectMemberProperties(object source, object target, IEnumerable<PropertyInfo> targetMembers)
        {
            var sourceType = source.GetType();

            foreach (var targetMember in targetMembers)
            {
                object sourceValue = source;
                PropertyInfo sourceMember;

                var joinAttr = targetMember.GetCustomAttribute<JoinAttribute>();
                if (joinAttr != null)
                {
                    var innerProperty = sourceType.GetProperty(joinAttr.PropertyName);
                    sourceMember = innerProperty.PropertyType.GetProperty(targetMember.Name);
                    sourceValue = innerProperty.GetValue(source);
                }
                else
                {
                    sourceMember = sourceType.GetProperty(targetMember.Name);                    
                }

                if (sourceMember != null)
                {
                    SetValue(sourceValue, target, sourceMember, targetMember);
                }
            }
        }

        private void SetValue(object source, object target, PropertyInfo sourceMember, PropertyInfo targetMember)
        {
            var targetType = targetMember.PropertyType;
            if (!targetType.IsPrimitive())
            {
                var complexObject = sourceMember.GetValue(source);
                if (complexObject != null)
                {
                    var targetMemberValue = InjectFrom(targetType, complexObject);
                    targetMember.SetValue(target, targetMemberValue);
                }
            }
            else
            {
                var val = sourceMember.GetValue(source);
                targetMember.SetValue(target, val);
            }
        }

        #endregion

        #region CollectionInjecter
        /// <summary>
        /// Maps a collection.
        /// </summary>
        protected virtual void InjectCollection(object source, object target, Type targetType)
        {
            if (source is IEnumerable sourceList)
            {
                if (_listType.IsAssignableFrom(targetType))
                {
                    var targetList = target as IList;
                    InjectCollectionItems(sourceList, targetList, targetType);
                }
            }
        }

        private void InjectCollectionItems(IEnumerable source, IList target, Type targetType)
        {
            var tmpList = target as IList;
            var itemType = target.GetType().GetGenericArguments().First();

            // Maps the collection items.
            foreach (var item in source as IEnumerable)
            {
                var newItem = InjectFrom(itemType, item);
                tmpList.Add(newItem);
            }
        }
        #endregion

        #region Fields
        private static readonly Type _listType = typeof(IList);
        #endregion
    }
}
