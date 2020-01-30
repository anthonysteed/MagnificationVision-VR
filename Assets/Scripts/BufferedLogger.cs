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
            _sb.Append(_className).Append(':');
            _isNewLine = false;
        }
        _sb.Append(label).Append('=').Append(data).Append(',');
    }

    public void CommitLine()
    {
        if (_isNewLine)
        {
            return;
        }
        FileLogger.Instance.AppendLine(_sb.ToString());
        _isNewLine = true;
    }


}
