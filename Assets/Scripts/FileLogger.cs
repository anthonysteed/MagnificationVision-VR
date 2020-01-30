using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public enum LogHeading
{
    GAZE_POS
}

public class FileLogger : MonoBehaviour
{
    private const string FOLDER_PATH = "./logs/";

    public static FileLogger Instance { get; private set; }

    private StreamWriter _logWriter;

    private Queue<string> _logQ = new Queue<string>();

    private void OnEnable()
    {
        DirectoryInfo dir = new DirectoryInfo(FOLDER_PATH);
        if (!dir.Exists)
        {
            dir.Create();
        }

        _logWriter = File.CreateText(FOLDER_PATH + System.DateTime.Now.ToString("yyyy-MM-dd_THH-mm-ss") + ".txt");
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one FileLogger!!!!");
        }
        Instance = this;
    }

    public void AppendLine(string line)
    {
        _logQ.Enqueue(line);
    }

    private void LateUpdate()
    {
        _logWriter.WriteLine("# frame " + Time.frameCount + ", time " + Time.time);
        while (_logQ.Count > 0)
        {
            _logWriter.WriteLine(_logQ.Dequeue());
        }
        _logWriter.WriteLine();
    }

    private void OnDisable()
    {
        _logWriter.Dispose();
    }

}
