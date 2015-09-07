namespace Lib
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    public abstract class PatchRequestBase<TPatchTarget>
    {
        protected readonly List<Func<TPatchTarget, bool>> Updates;

        public PatchRequestBase()
        {
            Updates = new List<Func<TPatchTarget, bool>>();
        }

        public bool TryUpdateFromMe(TPatchTarget original)
        {
            if (Updates.Count == 0)
                return false;
            bool changesRequired = false;
            foreach (var update in Updates)
            {
                changesRequired |= update.Invoke(original);
            }

            return changesRequired;
        }

        protected void RecordUpdate<TPropertyType>(Expression<Func<TPatchTarget, TPropertyType>> prop, TPropertyType value)
        {
            string propertyName = GetPropertyInfo(prop).Name;
            Updates.Add(data => UpdateOnlyIfChanged(data, propertyName, value));
        }

        protected void RecordUpdateWithCustomEqualityCheck<TPropertyType>(
            Func<TPatchTarget, TPropertyType, bool> customEqualityCheck,
            TPropertyType value,
            Action<TPatchTarget, TPropertyType> setAction)
        {
            Updates.Add((data) =>
            {
                if (customEqualityCheck(data, value))
                {
                    setAction(data, value);
                    return true;
                }
                return false;
            });
        }

        protected void RecordChildPropertyUpdate<TChildType, TPropertyType>(Func<TPatchTarget, TChildType> child, TPropertyType value) where TPropertyType : PatchRequestBase<TChildType>
        {
            Updates.Add(parentPatchTarget => value.TryUpdateFromMe(child(parentPatchTarget)));
        }

        private bool UpdateOnlyIfChanged<TPropertyType>(TPatchTarget data, string dataObjectPropertyName, TPropertyType value)
        {

            return UpdateOnlyIfChangedExpression<TPropertyType>(dataObjectPropertyName).Invoke(data, value);
        }

        private Func<TPatchTarget, TPropertyValue, bool> UpdateOnlyIfChangedExpression<TPropertyValue>(string dataObjectPropertyName, Expression equalityCheck = null)
        {
            ParameterExpression newValue = Expression.Parameter(typeof(TPropertyValue), "value");
            ParameterExpression originalData = Expression.Parameter(typeof(TPatchTarget), "data");
            MemberExpression propertyToUpdateIfChanged = Expression.Property(originalData, dataObjectPropertyName);
            LabelTarget returnStatement = Expression.Label(typeof(bool));

            BlockExpression block = Expression.Block
                (
                    Expression.IfThenElse
                        (
                            Expression.NotEqual(newValue, propertyToUpdateIfChanged),
                            Expression.Block
                                (
                                    Expression.Assign(propertyToUpdateIfChanged, newValue),
                                    Expression.Return(returnStatement, Expression.Constant(true, typeof(bool)))
                                ),
                            Expression.Return(returnStatement, Expression.Constant(false, typeof(bool)))
                        ),
                    Expression.Label(returnStatement, Expression.Constant(false))
                );

            return Expression.Lambda<Func<TPatchTarget, TPropertyValue, bool>>(block, originalData, newValue).Compile();
        }

        private PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expresion '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }
    }
}
