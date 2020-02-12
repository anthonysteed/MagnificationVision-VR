using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class BufferedLogger
{
    private readonly string _className;

    private StringBuilder _sb = new StringBuilder();

    private bool _isNewLine = true;

    public BufferedLogger(string className)
    {
        _className = className;
    }

    public void Append(string label, object data)
    {
        if (_isNewLine)
        {
            _sb.Clear();
            _sb.Append('"').Append(_className).Append('"').Append(":{");
            _isNewLine = false;
        }
        _sb.Append('"').Append(label).Append('"').Append(':');

        if (data is Vector3 v3)
        {
            _sb.Append("[").Append(v3.x + ",").Append(v3.y + ",").Append(v3.z + "]");
        }
        else if (data is Vector2 v2)
        {
            _sb.Append("[").Append(v2.x + ",").Append(v2.y).Append("]");
        }
        else if (data is float || data is int)
        {
            _sb.Append(data);
        }
        else
        {
            _sb.Append('"').Append(data).Append('"');
        }
        _sb.Append(',');
    }

    public void CommitLine()
    {
        if (_isNewLine)
        {
            return;
        }

        // remove trailing comma
        _sb.Remove(_sb.Length - 1, 1);
        _sb.Append("}");

        FileLogger.Instance.AppendLine(_sb.ToString());
        _isNewLine = true;
    }


}
