using QxFramework.Core;
using QxFramework.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ExpressionUtil : Singleton<ExpressionUtil>
{
    private Dictionary<Type, object> _tableFuncGroup;

    private ExpressionFunctions _functions;

    public ExpressionUtil()
    {
        _tableFuncGroup = new Dictionary<Type, object>();
        _functions = new ExpressionFunctions();
    }

    public bool RunTableExpression<T>(string table, string key1, string key2, T context)
    {
        string itemName = table + '.' + key2;


        if (!_tableFuncGroup.ContainsKey(context.GetType()))
        {
            _tableFuncGroup.Add(context.GetType(), new ExpressionParser.TableFuncGroup<T>(_functions));
        }

        ExpressionParser.TableFuncGroup<T> funcGroup = (ExpressionParser.TableFuncGroup<T>)_tableFuncGroup[context.GetType()];
        funcGroup.Parse(itemName, int.Parse(key1), Data.Instance.TableAgent.GetString(table, key1, key2));

        return funcGroup.Run(itemName, int.Parse(key1), context);
    }
}