using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace WinZoPNG
{
  public partial class FrmMain : Form
  {
    public FrmMain()
    {
      InitializeComponent();

      // Environment initialize
      NumThreads.Maximum = Environment.ProcessorCount;
      NumThreads.Value = GetRecommendThreadCount();
      CmbPriority.SelectedIndex = 0;
      CmbOptLv.SelectedIndex = 0;

      chiFilename = ChFilename.Index;
      chiSizeBefore = ChSizeBefore.Index;
      chiSizeAfter = ChSizeAfter.Index;
      chiReduction = ChReduction.Index;
      chiProgress = ChProgress.Index;

      // Initialize que list manager
      lmgr = new(LvTarget);
      lmgr.OnChangeCount += LvTarget_ChangeCount;

      // Add version number to Titlebar
      Text += Application.ProductVersion;

      LoadSettings();
      Application.DoEvents();

      // see args
      if (1 < Environment.GetCommandLineArgs().Length)
        _ = AddFiles([.. Environment.GetCommandLineArgs().Skip(1)]);
    }

    #region PropertiesAndInternalVariables

    /// <summary>constant strings for status enum (like).</summary>
    internal static class FILE_STATUS
    {
      public const string WAITING = "";
      public const string RUNNING = "Runnnig...";
      public const string FINISHED = "Finished.";
      public const string ERROR = "Error";
    }
    public const string NOTREDUCED = "(none)";

    /// <summary>Que listviewitems manager</summary>
    protected readonly ListManager lmgr;

    /// <summary><see langword="true"/>:Running optimization process, <see langword="false"/>:Waiting to begin optimize.</summary>
    protected bool _running;

    /// <summary>Total reduced size(bytes)</summary>
    protected long _reducedSize;
    /// <summary>File count of already optimized.</summary>
    protected int _cntOptAlready;
    /// <summary>File count of failed, not included _cntOptAlready.</summary>
    protected int _cntFail;
    /// <summary>File count of optimize succeeded.</summary>
    protected int _cntSuccess;
    /// <summary>Sum of File size of originals.</summary>
    protected long _totalOrigSize;
    /// <summary>Sum of File size of originals that has zopflipnged.</summary>
    protected long _totalProceedSize;
    /// <summary>optimization start time.</summary>
    protected DateTime _beginDate = DateTime.MinValue;

    /// <summary><see langword="true"/>:user clicked cancel button. <see langword="false"/>:not yet.</summary>
    protected bool _cancelRequested;

    /// <summary>ColumnHeader Index cache of ChFilename</summary>
    protected readonly int chiFilename;
    /// <summary>ColumnHeader Index cache of ChSizeBefore</summary>
    protected readonly int chiSizeBefore;
    /// <summary>ColumnHeader Index cache of ChSizeAfter</summary>
    protected readonly int chiSizeAfter;
    /// <summary>ColumnHeader Index cache of ChReduction</summary>
    protected readonly int chiReduction;
    /// <summary>ColumnHeader Index cache of ChProgress</summary>
    protected readonly int chiProgress;

    /// <summary>Parallel running manager</summary>
    private WorkerManager? workerManager;

    /// <summary>Tick in <see cref="DateTime.Ticks"/> last display progress of adding files/directories.</summary>
    protected long AddingProgressLast;

    /// <summary>Interval for update progress on adding files/directories in Ticks. <seealso cref="TimeSpan.TicksPerMillisecond"/></summary>
    protected const long AddingProgressInterval = 8000000; // 1ms=10,000Ticks

    #endregion PropertiesAndInternalVariables

    #region settings-io
    /// <summary>
    /// check <paramref name="_in"/> is in range or out of it.<br />
    /// return <paramref name="_in"/> if <paramref name="_min"/>&lt;=<paramref name="_in"/>&lt;=<paramref name="_max"/> else <paramref name="_default"/>.
    /// </summary>
    /// <param name="_in">input value</param>
    /// <param name="_min">min value, <paramref name="_in"/> must be &gt;=<paramref name="_min"/></param>
    /// <param name="_max">max value, <paramref name="_in"/> must be &lt;=<paramref name="_max"/></param>
    /// <param name="_default">default value, to return if it's out of range.</param>
    /// <returns><paramref name="_in"/> in range <paramref name="_min"/>&lt;<paramref name="_in"/>&lt;<paramref name="_max"/>, otherwise <paramref name="_default"/></returns>
    private protected static int RangeCheck(int _in, int _min, int _max, int _default)
    {
      return _in < _min || _max < _in ? _default : _in;
    }

    /// <summary>
    /// Load app settings from properties.
    /// </summary>
    protected void LoadSettings()
    {
      // default setting
      Properties.Settings d = Properties.Settings.Default;
      if (d.frmMain_Size.Width == 0) d.Upgrade(); // try load settings
      if (d.frmMain_Size.Width == 0) return; // load failed, skip apply loaded settings at First-Run

      // window size/position
      Location = d.frmMain_Location;
      Size = d.frmMain_Size;

      // optimize settings
      NumThreads.Value = RangeCheck(d.MaxThreads, 1, Convert.ToInt32(NumThreads.Maximum), Convert.ToInt32(NumThreads.Value));
      CmbOptLv.SelectedIndex = RangeCheck(d.OptimizeLevel, 0, CmbOptLv.Items.Count - 1, 0);
      CmbPriority.SelectedIndex = RangeCheck(d.Priority, 0, CmbPriority.Items.Count - 1, 0);
      ChkKeepTimestamp.Checked = d.KeepTimestamp;

      // listview columns width
      {
        string[] wsl = d.frmMain_lvTarget_ColWidth.Trim().Split(",");
        for (int i = 0, len = Math.Min(LvTarget.Columns.Count, wsl.Length); i < len; i++)
        {
          if (!int.TryParse(wsl[i], out int iw)) continue;
          LvTarget.Columns[i].Width = iw;
        }
      }

      if (d.frmMain_IsMaximized) WindowState = FormWindowState.Maximized;
    }

    protected void SaveSettings()
    {
      Properties.Settings d = Properties.Settings.Default;

      d.frmMain_IsMaximized = FormWindowState.Maximized == WindowState;
      Hide(); // run remain save-config processes in background
      WindowState = FormWindowState.Normal;
      d.frmMain_Location = this.Location;
      d.frmMain_Size = this.Size;
      d.MaxThreads = Convert.ToInt32(NumThreads.Value);
      d.Priority = CmbPriority.SelectedIndex;
      d.OptimizeLevel = CmbOptLv.SelectedIndex;
      d.KeepTimestamp = ChkKeepTimestamp.Checked;

      d.frmMain_lvTarget_ColWidth = new Func<string>(() =>
      {
        int l = LvTarget.Columns.Count;
        string[] il = new string[l];
        for (int i = 0; i < l; i++)
        {
          il[i] = LvTarget.Columns[i].Width.ToString();
        }
        return string.Join(",", il);
      })();

      d.Save();
    }

    #endregion settings-io

    /// <summary>set or reset enabled/visible/cursor properties</summary>
    /// <param name="IsRun">true to start action. false to finish.</param>
    /// <param name="IsCancellable">true:starting cancellable action, false(default): else</param>
    protected void SetRunStatus(bool IsRun, bool IsCancellable = false)
    {
      _running =
      Application.UseWaitCursor =
        IsRun;

      BtnAddFiles.Enabled =
      BtnAddDir.Enabled =
      BtnRm.Enabled =
      BtnExec.Enabled =
      BtnAbout.Enabled =
      BtnQuit.Enabled =
      CmbOptLv.Enabled =
      CmbPriority.Enabled =
      ChkKeepTimestamp.Enabled =
      TmRemove.Enabled =
        !IsRun;

      BtnCancel.Visible =
      BtnCancel.Enabled =
        IsRun && IsCancellable;

      if (!IsRun) NumThreads.ReadOnly = false;

      Update();
      Application.DoEvents(); // for repaint
    }

    /// <summary>
    /// command line option of ZopfliPNG compress levels.
    /// 0..3 -&gt; default(low) to insane(high).
    /// </summary>
    protected static readonly string[] optLevelToCmdopt = [
      "",
      "-m --filters=0me ",
      "-m --filters=01234me ",
      "-m --filters=01234mepb ",
    ];

    /// <summary>Indexed ProcessPriorityClass, except <see cref="ProcessPriorityClass.RealTime"/> due to it is very dangerous.</summary>
    protected static readonly ProcessPriorityClass[] priorities = [
      ProcessPriorityClass.Idle,
      ProcessPriorityClass.BelowNormal,
      ProcessPriorityClass.Normal,
      ProcessPriorityClass.AboveNormal,
      ProcessPriorityClass.High,
    ];

    /// <summary>
    /// Run Tasks. Called only from <see cref="BtnExec_Click(object, EventArgs)"/>
    /// </summary>
    protected void Run()
    {
      // Configures
      Color fgc = LvTarget.ForeColor;
      Color bgc = LvTarget.BackColor;
      bool keepTimestamp = ChkKeepTimestamp.Checked;
      string cmdOpt = optLevelToCmdopt[Properties.Settings.Default.OptimizeLevel];

      // build zopflipng path and check exists
      string zoppath = (Path.GetDirectoryName(Application.ExecutablePath) ?? ".") + "\\zopflipng.exe";
      if (!File.Exists(zoppath))
      {
        _ = MessageBox.Show($"Zopflipng.exe does not found at: {zoppath}\nPlease set it.", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      // Methods for Invoke from parallel run sub threads.
      void SetStatus(ListViewItem item, string status, Color? fgcolor = null, Color? bgcolor = null)
      {
        item.SubItems[chiProgress].Text = status;
        item.SubItems[chiProgress].ForeColor = fgcolor ?? fgc;
        item.SubItems[chiProgress].BackColor = bgcolor ?? bgc;
        int index = item.Index;
        if (0 <= index && item.ListView?.TopItem?.Index <= index) item.ListView.RedrawItems(index, index, false);
      }

      void SetResults(ListViewItem item, long szAfter, long szReduced)
      {
        item.SubItems[chiSizeAfter].Text = $"{szAfter:#,##0}";
        item.SubItems[chiReduction].Text = (szReduced <= 0) ? NOTREDUCED : $"{szReduced:#,##0}";
        item.SubItems[chiProgress].Text = FILE_STATUS.FINISHED;
        item.SubItems[chiProgress].ForeColor = SystemColors.GrayText;
        int index = item.Index;
        if (0 <= index) item.ListView?.RedrawItems(index, index, false);
      }

      void OnFinish(object? sender, EventArgs args)
      {
        Invoke(DisplayFinish);
      }

      // Count Total file size
      _totalOrigSize = 0;
      _totalProceedSize = 0;
      for (int i = 0, j = lmgr.Count; i < j; i++)
      {
        ListViewItem item = lmgr[i];
        try
        {
          _totalOrigSize += long.Parse(item.SubItems[chiSizeBefore].Text.Replace(",", ""));
        }
        catch { } // Through this exception, no major impacts.
      }

      // Initialize WorkerManager
      workerManager = new(Convert.ToInt32(NumThreads.Value));
      workerManager.OnAllThreadsGone += OnFinish;
      workerManager.PriorityOfThread = (ThreadPriority)CmbPriority.SelectedIndex;


      ProcessPriorityClass processPriority = priorities[CmbPriority.SelectedIndex];

      ///  Exec a file in worker thread, NOT main thread
      void ExecFileWorker()
      {
        while (!_cancelRequested && _running)
        {
          // Thread count manipulation, exit thread when runningThread count is greater than requested
          if (workerManager.ShouldIExitThread()) return;

          // pop a next item to process
          ListViewItem? item = (ListViewItem?)Invoke(lmgr.PopNext, chiProgress, FILE_STATUS.RUNNING, SystemColors.Highlight, bgc);
          if (item is null) return; // No more queue, finished.

          string orgFile = "";
          string tmpFile = "";
          string args = "";
          string msg = "";
          int r = -1;
          long szBefore = 0;
          try
          {
            orgFile = item.Text;
            tmpFile = GetTmpFile(orgFile);

            // Store Create/LastModified time
            FileInfo fb = new(orgFile);
            DateTime fbCre = fb.CreationTimeUtc;
            DateTime fbMod = fb.LastWriteTimeUtc;
            szBefore = fb.Length;

            // zopflipng [options]... infile.png outfile.png
            // -y: do not ask about overwriting files.
            //     set -y due to GetTmpFile created a file.
            args = $" -y {cmdOpt} \"{orgFile}\" \"{tmpFile}\"";

            r = StartProcess(zoppath, args, processPriority, out msg);
            if (r != 0)
            {
              _ = Invoke(SetStatus, item, $"error: code:{r}", SystemColors.HotTrack, null);
              _cntFail++;
              continue;
            }

            FileInfo fa = new(tmpFile);
            long szAfter = fa.Length;
            long szReduced = 0;
            if (0 < szAfter && szAfter < szBefore)
            { // optimize (reduce size) succeeded
              fa.MoveTo(orgFile, true);

              szReduced = szBefore - szAfter;
              _reducedSize += szReduced;
              _cntSuccess++;

              if (keepTimestamp)
              { // timestamp keeping operation
                fa.CreationTimeUtc = fbCre;
                fa.LastWriteTimeUtc = fbMod;
              }
            }
            else
            { // already optimized
              File.Delete(tmpFile);
              szAfter = szBefore;
              _cntOptAlready++;
            }

            _ = Invoke(SetResults, item, szAfter, szReduced);
          }
          catch (Exception ex)
          {
            _cntFail++;
            _ = Invoke(SetStatus, item, $"error: {ex}", SystemColors.HotTrack, null);
          }
          finally
          {
            // When temporary file exists, try to remove it
            if (0 < tmpFile.Length && File.Exists(tmpFile))
            {
              try { File.Delete(tmpFile); } catch { } // TODO: Catch and warn
            }
            _totalProceedSize += szBefore;
          }
        } // while (!_cancelRequested && _running)
      } // void ExecFileWorker()

      workerManager.Run(new Action(ExecFileWorker));

      GC.Collect();
    }

    #region Progress-Result
    /// <summary>
    /// Make H:MM:SS format from Ticks, used to Elapse
    /// </summary>
    /// <param name="ElaTicks">Elapse in 100 nano seconds</param>
    /// <returns>formatted string</returns>
    /// <see cref="DateTime.Ticks"/>
    public static string FormatElapse(long ElaTicks)
    {
      (long h, long hRem) = Math.DivRem(ElaTicks, TimeSpan.TicksPerHour);
      (long m, long mRem) = Math.DivRem(hRem, TimeSpan.TicksPerMinute);
      long s = (mRem + (TimeSpan.TicksPerSecond / 2)) / TimeSpan.TicksPerSecond;

      return $"{h}:{m:00}:{s:00}";
    }

    /// <summary>
    /// Show progress on status bar.
    /// </summary>
    protected void DisplayProgress()
    {
      if (!_running)
      {
        Invoke(DisplayFinish);
        return;
      }

      int cntAll = lmgr.Count;
      int cntRun = workerManager?.GetRunnningThreadCount() ?? 0;
      int cntFin = _cntOptAlready + _cntFail + _cntSuccess;
      int cntErr = _cntFail;
      int cntWat = cntAll - cntRun - cntFin - cntErr;

      double progRate = _totalProceedSize * 1.0 / _totalOrigSize; // finished_bytes / total_bytes as float;


      DateTime now = DateTime.Now;
      long b = _beginDate.Ticks; /// Begin Time in Ticks
      long e = now.Ticks - b;    /// Elapsed Ticks

      string strEta = "";
      if (0 < progRate)
      {
        DateTime eta = new(b + Convert.ToInt64(e / progRate));
        strEta = "ETA " + (eta.DayOfYear == now.DayOfYear ? eta.ToString("HH:mm:ss") : eta.ToString("MM/dd HH:mm:ss")) + "; ";
      }

      double compressRate = 0 == _totalProceedSize ? 0 : _reducedSize * 100.0 / _totalProceedSize;

      stLabel.Text =
        $"{now:HH':'mm':'ss}; " +
        $"{(!_cancelRequested ? "" : "Cancelling...; ")}Elapse {FormatElapse(e)}; " +
        $"{progRate:P2} ({cntFin:#,##0}/{cntAll:#,##0}) done; " +
        $"{strEta}"+
        $"{cntRun:#,##0} file{(1 == cntRun ? " is" : "s are")} Running; " +
        $"Remain {cntWat:#,##0} file(s); " +
        $"Total Reduced {ToReadableSize(_reducedSize)} ({_reducedSize:#,##0}Bytes {compressRate:0.##}%)" +
        "";
    }

    /// <summary>
    /// Show results on status bar.
    /// </summary>
    protected void DisplayFinish()
    {
      TimProgress.Stop();
      TimProgress.Enabled = false;
      SetRunStatus(false);

      int cntFin = lmgr.GetCountByText(chiProgress, FILE_STATUS.FINISHED);
      int cntErr = lmgr.GetCountByText(chiProgress, FILE_STATUS.ERROR);
      int cntNoR = lmgr.GetCountByText(chiReduction, NOTREDUCED);
      int cntWai = lmgr.GetCountByText(chiProgress, FILE_STATUS.WAITING);

      DateTime dtNow = DateTime.Now;
      string strNow = dtNow.ToString("HH':'mm':'ss");
      string elapse = FormatElapse(dtNow.Ticks - _beginDate.Ticks);

      double compressRate = _reducedSize * 100.0 / _totalProceedSize; // lmgr.GetTotalCompressedRate(chiSizeBefore, chiSizeAfter) * 100.0;

      stLabel.Text =
        $"{strNow}; " +
        $"{(_cancelRequested && 0 < cntWai ? "Canceled ;" : "")}" +
        $"Elapse {elapse}; {cntFin:#,##0} files done; " +
        $"{(0 == cntErr ? "" : $"{cntErr:#,##0} file(s) were failed; ")}" +
        $"Not Reduced: {cntNoR:#,##0}; " +
        $"Total Reduced Size: {ToReadableSize(_reducedSize)} ({_reducedSize:#,##0}Bytes, {compressRate:0.##}%)" +
        "";
    }
    #endregion Progress-Result


    #region Static Methods
    /// <summary>returns recommend thread Count calculated by Processor Count(incl SMT)</summary>
    /// <returns>number of recommendation. <code>1&lt;=x&lt;=Environment.ProcessorCount</code></returns>
    /// <seealso cref="Environment.ProcessorCount"/>
    public static int GetRecommendThreadCount()
    {
      int pc = Environment.ProcessorCount;
      if (pc <= 3) return pc;
      if (pc <= 9) return pc - 1;
      return Convert.ToInt32(Math.Ceiling(pc * 0.9));
    }

    /// <summary>
    /// Show Error Message box dialog
    /// </summary>
    /// <param name="ex">Exception to display</param>
    public static void ShowExceptionBox(Exception ex)
    {
      _ = MessageBox.Show(null, $"an error occured. details:\r\n{ex}", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>Create tmp file with ext kept. eg) <code>hoge.txt</code> to <code>hoge.tmpXXXX.txt</code></summary>
    /// <param name="basefile">Fullpath with base name. eg) c:\tmp\hoge</param>
    /// <returns>Random generated temporary file full-exepath</returns>
    /// <exception cref="IOException"></exception>
    /// <exception cref="Exception"></exception>
    public static string GetTmpFile(string basefile)
    {
      /// <summary>Get random chars that length you specified. random chars from [0-9A-Za-z]</summary>
      /// <param name="len">Length for random chars.</param>
      /// <returns>Random chars as string</returns>
      static string GetRandomChars(int len = 4)
      {
        StringBuilder sb = new(len);
        for (int i = 0; i < len; i++)
        {
          // base62
          // 0-9 : 0x30-39 : 10 : 0- 9
          // A-Z : 0x41-5a : 26 :10-35
          // a-z : 0x61-7a : 26 :36-61
          int c, r;
          r = Random.Shared.Next(0, 62);
          if (r < 10)
          {
            c = r + 0x30;
          }
          else if (r < 36)
          {
            c = r + 0x41 - 10;
          }
          else
          {
            c = r + 0x61 - 36;
          }
          _ = sb.Append(char.ConvertFromUtf32(c));
        }
        return sb.ToString();
      }

      const int max_try = 10;
      string dir = Path.GetDirectoryName(basefile) ?? "";
      string bnm = Path.GetFileNameWithoutExtension(basefile) ?? "";
      string ext = Path.GetExtension(basefile) ?? ""; // if ext exisits, with dot-lead.

      for (int i = 0; i < max_try; i++)
      {
        string rnd = GetRandomChars();
        string tmpath = Path.Combine(dir, $"{bnm}.{rnd}{ext}");
        if (File.Exists(tmpath)) continue;
        FileStream? fs = null;
        try
        {
          fs = File.OpenWrite(tmpath);
          fs.Close();
          fs.Dispose();
          return tmpath;
        }
        catch
        {
          // 
          continue;
        }
        finally
        {
          try
          {
            if (null != fs)
            {
              fs.Close();
              fs.Dispose();
            }
          }
          catch
          {
            // through
          }
        }
      }
      throw new Exception($"Could not create tmp file over {max_try} times.");
    }

    /// <summary>
    /// start a process and get stdout/strerr
    /// </summary>
    /// <param name="exepath">path to executable file.</param>
    /// <param name="cmdopt">argument(s) to run.</param>
    /// <param name="priority">Priority of child process.</param>
    /// <param name="msg">stdout/err</param>
    /// <returns>return code of exe</returns>
    protected static int StartProcess(string exepath, string cmdopt, ProcessPriorityClass priority, out string msg)
    {
      Process? proc = null;
      try
      {
        proc = Process.Start(new ProcessStartInfo("\"" + exepath + "\"", cmdopt)
        {
          RedirectStandardError = true,
          RedirectStandardOutput = true,
          UseShellExecute = false,
          CreateNoWindow = true,
        }
        );
        if (null == proc) _ = new InvalidDataException("Could not start a process by unknown error, returned proc is null.");
        if (null == proc) { msg = ""; return -2; }
        proc.PriorityClass = priority;
        proc.PriorityBoostEnabled = false;
        proc.WaitForExit();
        string so = proc.StandardOutput.ReadToEnd();
        string se = proc.StandardError.ReadToEnd();
        msg = so + "\r\n" + se;
        return proc.ExitCode;
      }
      catch (Exception e)
      {
        msg = $"Could not start a process: {exepath} {cmdopt} {e.GetType().FullName} {e}";
        MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return -1;
      }
      finally
      {
        if (null != proc) try { proc.Close(); } catch { }
      }
    }

    /// <summary>Binary Prefix List such as <code>GiB</code>(Gibi-Byte), from B(2^0) until EiB(2^60), due to limitation of long((2^63)-1) range.</summary>
    protected static readonly string[] units = [ "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB" ];
    /// <summary>
    /// convert bytes to Human readable size with Binary-prefix. eg: 1048576 Bytes to 1MiB<br />
    /// not SI-Prefix(10^3=1000), Binary-Prefix (2^10=1024)<br />
    /// due to "long" (signed 64bit integer) limitation, support until (2^63)-1 BytesÅ‡8EiB-1Bytes.
    /// </summary>
    /// <param name="size">Size in byte. must be 0&lt;=</param>
    /// <param name="aboutprefix">prefix for "about".<br /><tt>1,048,57<i>6</i></tt> Bytes is exact equal to 1MiB, however, for example, <tt>1,048,57<i>5</i></tt> Bytes is "approx. 1MiB"</param>
    /// <returns>Human readable size formatted string. eg) "approx. 1MiB"</returns>
    /// <exception cref="ArgumentOutOfRangeException">throws when negative number in size argument.</exception>
    public static string ToReadableSize(long size, string aboutprefix = "approx.")
    {
      if (size < 0) throw new ArgumentOutOfRangeException(nameof(size), "must be 0<=");
      if (size == 0) return "0B";
      double x = (Math.Log(size) / Math.Log(1024)); // íÍÇÃïœä∑, size=1024^x ÇÃ x (sizeÇÕ1024ÇÃâΩèÊÇ©) ÇãÅÇﬂÇÈ

      int xi = Convert.ToInt32(Math.Truncate(x));
      if (units.Length < xi) xi = units.Length;

      double os = Math.Round(size / Math.Pow(1024, xi), 2);

      // format and removd 0 on tail. when exact integer, remove dot too.
      string ss = os.ToString("N");
      if (0 <= ss.IndexOf('.'))
      {
        ss = ss[..(ss.EndsWith(".00") ? ss.Length - 3 : ss.EndsWith('0') ? ss.Length - 1 : ss.Length)];
      }

      return (size != Convert.ToInt64(os * Math.Pow(1024, xi)) ? aboutprefix : "") + ss + units[xi];
    }

    #endregion Static Methods

    #region adding-files

    /// <summary>Display progress of adding files or searching directory</summary>
    /// <param name="position">Path of current searching position.</param>
    protected void AddingProgress(string position)
    {
      long n = DateTime.Now.Ticks;
      if (n < (AddingProgressLast + AddingProgressInterval)) return;
      stLabel.Text = "Search and adding: " + position;
      AddingProgressLast = n;
      StatusStrip1.Refresh();
    }

    /// <summary>Add a file to queue list</summary>
    /// <param name="file">Fullpath of file</param>
    /// <returns>added file count</returns>
    protected int AddFile(string file)
    {
      try
      {
        if (!File.Exists(file)) return 0;
        if (lmgr.HasInText(file)) return 0;

        FileStream? s = null;
        try
        {
          // try to open a file with write permission
          s = File.OpenWrite(file);
          s.Close();
          s.Dispose();
        }
        catch (Exception ex)
        {
          ShowExceptionBox(ex);
          return 0;
        }
        finally
        {
          if (s != null) try { s.Close(); s.Dispose(); } catch (Exception ex) { _ = ex; }
        }

        long flen = new FileInfo(file).Length;
        if (0 == flen) return 0;

        ListViewItem item = new(file);
        item.SubItems.Add($"{flen:#,##0}");
        item.SubItems.Add("");
        item.SubItems.Add("");
        item.SubItems.Add("");
        lmgr.Add(item);
        //lmgr.Add(new(new string[] { file, flen.ToString(), "", "", "" }));

        AddingProgress(file);

        return 1;
      }
      catch (Exception ex)
      {
        _ = ex;
      }
      return 0;
    }

    protected int AddFiles(string[] files)
    {
      int cnt = 0;
      foreach (string file in files)
      {
        if (File.Exists(file))
        {
          cnt += AddFile(file);
        }
        else if (Directory.Exists(file))
        {
          cnt += AddDir(file);
        }
      }
      return cnt;
    }

    protected int AddDir(string dir)
    {
      AddingProgress(dir);
      return (!Directory.Exists(dir)) ? 0 : AddFiles(Directory.GetFiles(dir, "*.png", SearchOption.AllDirectories));
    }

    #endregion adding-files

    #region EventHandlers

    #region FromEvents
    private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
    {
      SaveSettings();
    }

    private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (_running) e.Cancel = true;
    }

    private void FrmMain_DragEnter(object sender, DragEventArgs e)
    {
      e.Effect = DragDropEffects.All;
    }

    private void FrmMain_DragDrop(object sender, DragEventArgs e)
    {
      try
      {
        SetRunStatus(true);
        if (null == e || null == e.Data) return;
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files && 0 < files.Length)
        {
          int c = AddFiles(files);
          stLabel.Text = $"{c:#,##0} file(s) added. {lmgr.GetCountByText(chiProgress, FILE_STATUS.WAITING):#,##0} file(s) are wait to execution.";
        }
      }
      catch (Exception ex)
      {
        ShowExceptionBox(ex);
      }
      finally
      {
        SetRunStatus(false);
      }
    }

    #endregion FormEvents

    #region ListViewRelatedRegion
    #region ListViewVirtualItem
    private void LvTarget_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
    {
      e.Item = lmgr[e.ItemIndex];
    }

    private void LvTarget_ChangeCount(ListManager mgr)
    {
      LvTarget.VirtualListSize = mgr.Count;
    }
    #endregion ListViewVirtualItem


    // sort function
    protected ListViewItemComparer comparer = new ListViewItemComparerStr(0, SortOrder.None);

    private void LvTarget_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      SortOrder so = ((comparer.Column == e.Column) && comparer.Sorting == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
      comparer = e.Column switch
      {
        1 or 2 or 3 => new ListViewItemComparerInt(e.Column, so),
        _ => new ListViewItemComparerStr(e.Column, so),
      };
      lmgr.Sort(comparer);
    }
    private void LvTarget_MouseUp(object sender, MouseEventArgs e)
    {
      if (!e.Button.Equals(MouseButtons.Right)) return;
      contextMenuStrip1.Show(LvTarget, e.Location);
    }

    private void LvTarget_KeyDown(object sender, KeyEventArgs e)
    {
      // Implements of shortcut keys
      if (e.KeyData == (Keys.Control | Keys.A))
      {
        TmSelectAll_Click(sender, e);
      }
      else if (e.KeyData == (Keys.Control | Keys.I))
      {
        TmInvertSelection_Click(sender, e);
      }
      else if (e.KeyData == (Keys.Control | Keys.C))
      {
        TmCopyFull_Click(sender, e);
      }
      else if (e.KeyData == (Keys.Control | Keys.Shift | Keys.C))
      {
        TmCopyFilename_Click(sender, e);
      }
      else if (e.KeyData == Keys.Delete && LvTarget.Focused)
      {
        BtnRm_Click(sender, e);
      }
    }
    #endregion ListViewRelatedRegion

    #region CommandPaneEvents
    private void BtnAddFiles_Click(object sender, EventArgs e)
    {
      if (DialogResult.Cancel == OpenFileDlg.ShowDialog()) return;
      try
      {
        SetRunStatus(true);
        AddingProgressLast = DateTime.Now.Ticks;
        int c = AddFiles(OpenFileDlg.FileNames);
        stLabel.Text = $"{c:#,##0} file(s) added. {lmgr.GetCountByText(chiProgress, FILE_STATUS.WAITING):#,##0} file(s) are wait to execution.";
      }
      catch (Exception ex)
      {
        ShowExceptionBox(ex);
      }
      finally
      {
        SetRunStatus(false);
      }
    }

    private void BtnAddDir_Click(object sender, EventArgs e)
    {
      if (DialogResult.Cancel == FolderBrowserDlg.ShowDialog()) return;
      try
      {
        SetRunStatus(true);
        AddingProgressLast = DateTime.Now.Ticks;
        int c = AddDir(FolderBrowserDlg.SelectedPath);
        stLabel.Text = $"{c:#,##0} file(s) added. {lmgr.GetCountByText(chiProgress, FILE_STATUS.WAITING):#,##0} file(s) are wait to execution.";
      }
      catch (Exception ex)
      {
        ShowExceptionBox(ex);
      }
      finally
      {
        SetRunStatus(false);
      }
    }

    private void BtnRm_Click(object sender, EventArgs e)
    {
      LvTarget.BeginUpdate();
      try
      {
        SetRunStatus(true);
        BtnCancel.Visible = BtnCancel.Enabled = false;

        ListView.SelectedIndexCollection si = LvTarget.SelectedIndices;
        int c = si.Count;
        int[] indices = new int[c];
        si.CopyTo(indices, 0); // batch copy list due to VERY slow to refer SelectedIndices
        lmgr.Removes(indices);
        LvTarget.Refresh();
        stLabel.Text = $"{c:#,##0} file(s) deleted. {lmgr.GetCountByText(chiProgress, FILE_STATUS.WAITING):#,##0} file(s) are wait to execution.";
      }
      finally
      {
        LvTarget.EndUpdate();
        SetRunStatus(false);
      }
    }

    private void BtnExec_Click(object sender, EventArgs e)
    {
      SetRunStatus(true);

      Properties.Settings.Default.MaxThreads = Convert.ToInt32(NumThreads.Value);
      Properties.Settings.Default.OptimizeLevel = CmbOptLv.SelectedIndex;

      // init
      _reducedSize = 0;
      _cntOptAlready = 0;
      _cntFail = 0;
      _cntSuccess = 0;
      _beginDate = DateTime.Now;
      _cancelRequested = false;
      lmgr.InitQueue(chiProgress);

      // run it
      SetRunStatus(true, true);
      TimProgress.Start();
      Run();
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
      BtnCancel.Enabled = false;
      NumThreads.ReadOnly = true;
      _cancelRequested = true;
    }

    private void NumThreads_ValueChanged(object sender, EventArgs e)
    {
      if (_running && !_cancelRequested && workerManager is not null)
      {
        workerManager.Parallels = Convert.ToInt32(NumThreads.Value);
      }
    }


    private void BtnAbout_Click(object sender, EventArgs e)
    {
      _ = MessageBox.Show(null, ""
        + $"WinZoPNG by CatStorming\r\n"
        + $"https://github.com/CatStorming/WinZoPNG\r\n"
        + $"App Version\t: {Application.ProductVersion} {(Environment.Is64BitProcess ? "64" : "32")}bit\r\n"
        + $".Net Version\t: {Environment.Version}\r\n"
        + $"Running on\t: {Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "64" : "32")}bit\r\n"
        + $"Configfile\t:\n{System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath}"
        , $"About {Application.ProductName}", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnQuit_Click(object sender, EventArgs e)
    {
      Close();
    }
    #endregion CommandPaneEvents

    #region PopupMenuActions
    private void TmSelectAll_Click(object sender, EventArgs e)
    {
      LvTarget.SelectAll();
    }
    private void TmInvertSelection_Click(object sender, EventArgs e)
    {
      lmgr.InvertSelection();
    }
    private void TmCopyFilename_Click(object sender, EventArgs e)
    {
      try
      {
        Clipboard.Clear();
        Clipboard.SetText(lmgr.GetCopyText() ?? "");
      }
      catch (Exception ex)
      {
        ShowExceptionBox(ex);
      }
    }
    private void TmCopyFull_Click(object sender, EventArgs e)
    {
      try
      {
        Clipboard.Clear();
        Clipboard.SetText(lmgr.GetCopyFullText() ?? "");
      }
      catch (Exception ex)
      {
        ShowExceptionBox(ex);
      }
    }

    #endregion PopupMenuActions

    #region OtherEventHandlers
    private void StLabel_DoubleClick(object sender, EventArgs e)
    {
      // Copy text from status bar on double-click it.
      try
      {
        if (null == stLabel.Text) return;
        Clipboard.Clear();
        Clipboard.SetText(stLabel.Text);
      }
      catch (Exception ex)
      {
        ShowExceptionBox(ex);
      }
    }

    private void TimProgress_Tick(object sender, EventArgs e)
    {
      try
      {
        DisplayProgress();
      }
      catch
      {
        // through
      }
    }
    #endregion OtherEventHandlers

    #endregion EventHandlers

  }


  ////////////////////////////////////////////////////////////////////////
  // ListView with DoubleBuffered drawing
  public class BufferedListView : System.Windows.Forms.ListView
  {
    protected override bool DoubleBuffered { get => true; set { } }
    public void SelectAll()
    {
      for (int i = 0, j = Items.Count; i < j; i++)
      {
          Items[i].Selected = true;
      }
    }
  }
}