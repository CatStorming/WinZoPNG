using System.Collections.Concurrent;

namespace WinZoPNG
{
  internal class WorkerManager
  {
    /// <summary>
    /// Minimum number of paralell run thread(s)
    /// </summary>
    public const int MIN_PARALLELS = 1;

    /// <summary>
    /// Collection of Threads(Task s)
    /// int: ID of Task
    /// </summary>
    protected ConcurrentDictionary<int, Task> tasks = new();

    /// <summary>
    /// Nullable. What to do on each thead(s).
    /// Set value during running. after finish all thread(s), re-set null to this var.
    /// </summary>
    protected Action? runningAction;

    /// <summary>
    /// Triggers when finish all thread(s). <see cref="EventArgs"/> is not null, but meaningless.
    /// </summary>
    public event EventHandler? OnAllThreadsGone;

    /// <summary>
    /// Priority of worker thread. Default is <see cref="ThreadPriority.Lowest"/>
    /// </summary>
    public ThreadPriority PriorityOfThread { get; set; } = ThreadPriority.Lowest;

    /// <summary></summary>
    /// <param name="Parallels">Number of simultaneous threads. must be greater than 0.</param>
    /// <param name="action">What do on each thread(s).</param>
    /// <exception cref="ArgumentOutOfRangeException">Raise if <paramref name="Parallels"/> is less than 1.</exception>
    public WorkerManager(int _Parallels)
    {
      if (_Parallels < MIN_PARALLELS) _ = new ArgumentOutOfRangeException(nameof(_Parallels), $"Invalid Number #{_Parallels} is less than #{MIN_PARALLELS}.");
      parallels = _Parallels;
    }

    /// <summary>
    /// variable of <seealso cref="Parallels"/>
    /// </summary>
    protected int parallels = MIN_PARALLELS;

    /// <summary>
    /// Number of simultaneous threads. must be greater equal than <seealso cref="MIN_PARALLELS"/>.<br />
    /// Add thread(s) when set greater than current running threads (see:<see cref="GetRunnningThreadCount"/>).
    /// </summary>
    public int Parallels
    {
      get => parallels;
      set
      {
        if (value < MIN_PARALLELS) _ = new ArgumentOutOfRangeException("Parallels", "Invalid Number " + value + $" is less than #{MIN_PARALLELS}.");
        parallels = value;

        // add thread(s) if not enough
        // deletion at polling time on each threads
        if (runningAction is not null)
        {
          for (int i = value - tasks.Count; 0 < i; i--)
          {
            AddThread();
          }
        }
      }
    }

    /// <summary>
    /// Wrap an action.
    /// 1. To change thread priority.
    /// 2. To catch finish of action. After finish <see cref="runningAction"/>, remove this Task from <see cref="tasks"/>.
    /// </summary>
    protected void ActionWrapper()
    {
      Thread.CurrentThread.Priority = PriorityOfThread;

      int? tid = Task.CurrentId;
      if (tid is null)
      {
        // CurrentId is null, try add a thread again.
        AddThread();
        return;
      }

      int rid = tid.Value;
      if (!tasks.ContainsKey(rid))
      {
        // TaskId duplicated (MS said it's rare-case), try add a thread again.
        AddThread();
        return;
      }

      try
      {
        if (runningAction is not null)
        {
          runningAction();
        }
        else
        {
          _ = new InvalidOperationException($"ActionWrapper() called without RunningTask.");
        }
      }
      finally
      {
        RemoveThread(rid);
      }
    }

    /// <summary>
    /// Add a thread (run a Task).
    /// </summary>
    protected void AddThread()
    {
      if (runningAction is null)
      {
        _ = new InvalidOperationException($"AddThread() called without RunningTask.");
      }
      else
      {
        Task task = new(ActionWrapper);
        if (tasks.ContainsKey(task.Id) || !tasks.TryAdd(task.Id, task))
        {
          // taskid dupulicated (MS said it's rare-case), try add an another thread.
          task.Dispose();
          AddThread();
          return;
        }
        task.Start();
      }
    }

    /// <summary>
    /// Remove a specified task from taskList.
    /// Trigger <see cref="OnAllThreadsGone"/> event if all threads deleted after delete specified task.
    /// </summary>
    /// <param name="Id">an ID of Task. <see cref="Task.Id"/> or <see cref="Task.CurrentId"/></param>
    protected void RemoveThread(int? Id)
    {
      if (Id is null) return;
      _ = tasks.Remove((int)Id, out Task? task);
      if (tasks.IsEmpty)
      {
        OnAllThreadsGone?.Invoke(this, new EventArgs());
        runningAction = null;
      }
    }

    /// <summary>
    /// Begin job(s).
    /// </summary>
    /// <param name="_action">What to do on each thread(s).</param>
    /// <exception cref="InvalidOperationException">Raises if already running.</exception>
    public void Run(Action _action)
    {
      int cnt = GetRunnningThreadCount();
      if (0 < cnt || runningAction is not null)
      {
        _ = new InvalidOperationException("Tried Run() in spite of there'is still runnning Thread(s)");
      }
      tasks.Clear();
      runningAction = _action;
      for (int i = 0; i < parallels; i++)
      {
        AddThread();
      }
    }

    /// <summary>
    /// Confirm threads' status and returns how many runnnig thread(s).
    /// </summary>
    /// <returns>Number of running task(s).</returns>
    public int GetRunnningThreadCount()
    {
      IEnumerator<KeyValuePair<int, Task>> enumerator = tasks.GetEnumerator();
      while (enumerator.MoveNext())
      {
        KeyValuePair<int, Task> taskPair = enumerator.Current;
        Task task = taskPair.Value;
        if (task.Status is not (TaskStatus.Running or TaskStatus.Created or TaskStatus.WaitingForActivation or TaskStatus.WaitingToRun))
        {
          // Not Runninng
          // Use Task() method instead of List.Remove()
          const int maxTry = 10;
          for (int i = 0; i < maxTry; i++)
          {
            if (tasks.TryRemove(taskPair)) { break; }
          }
          if (tasks.ContainsKey(task.Id)) { _ = new InvalidOperationException($"Failed tryRemove task #{maxTry} times."); }
        }
      }
      return tasks.Count;
    }

    /// <summary>
    /// May this function called by Consumer(Worker) Thread, to check continue processing loop or not.
    /// Return depends on only thread count configuration.
    /// </summary>
    /// <returns><see langword="true"/>returns when there're too many worker threads. <see langword="false"/> other wise.</returns>
    public bool ShouldIExitThread()
    {
      return parallels < GetRunnningThreadCount();
    }
  }
}
