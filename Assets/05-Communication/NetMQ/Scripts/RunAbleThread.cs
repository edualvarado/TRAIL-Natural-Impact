/****************************************************
 * File: RunAbleThread.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/10/2020
   * Project: Foot2Trail
   * Last update: 20/02/2023
*****************************************************/

using System.Threading;

/// <summary>
///     The superclass that you should derive from. It provides Start() and Stop() method and Running property.
///     It will start the thread to run Run() when you call Start().
/// </summary>
public abstract class RunAbleThread
{
    #region Read-only & Static Fields

    private readonly Thread _runnerThread;

    #endregion

    protected RunAbleThread()
    {
        // We need to create a thread instead of calling Run() directly because it would block Unity from doing other tasks like drawing game scenes
        _runnerThread = new Thread(Run);
    }

    protected bool Running { get; private set; }

    /// <summary>
    /// This method will get called when you call Start(). Programmer must implement this method while making sure that this method terminates in a finite time. 
    /// You can use Running property (which will be set to false when Stop() is called) to determine when you should stop the method.
    /// </summary>
    protected abstract void Run();

    public void Start()
    {
        Running = true;
        _runnerThread.Start();
    }

    public void Stop()
    {
        Running = false;
        // Block main thread, wait for _runnerThread to finish its job first, so we can be sure that _runnerThread will end before main thread end
        _runnerThread.Join();
    }
}