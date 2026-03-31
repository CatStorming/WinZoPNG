using System.Collections.Concurrent;
using System.Text;
using static System.Windows.Forms.ListView;

namespace WinZoPNG
{
  /// <summary>
  /// Manager for ListViewItem with Virtual-mode ListView.<br/>
  /// ListViewItem for 
  /// </summary>
  public class ListManager(ListView TargetListView)
  {
    /// <summary>
    /// </summary>
    /// <param name="mgr"></param>
    /// <param name="evType"></param>
    public delegate void ListManagerEvent(ListManager mgr);

    protected List<ListViewItem> innerList = [];
    public ListViewItem this[int index] => innerList[index];

    protected ListView _lvTarget = TargetListView;
    public ListView ListViewTarget => _lvTarget;

    public int Count => innerList.Count;

    public event ListManagerEvent? OnChangeCount;

    protected const int MinSubItemsCount = 5;

    /// <summary>
    /// Run an event handler that specified.
    /// </summary>
    /// <param name="targets">EventHandler innerList</param>
    /// <param name="eType">type of event</param>
    protected void TriggerEvent(List<ListManagerEvent> targets)
    {
      targets.ForEach(item =>
      {
        item(this);
      });
    }

    /// <summary>
    /// Add a ListViewItem to innerList
    /// </summary>
    /// <param name="item">item to add</param>
    public void Add(ListViewItem item)
    {
      for(int i = item.SubItems.Count; i < MinSubItemsCount; i++)
      {
        item.SubItems.Add("");
      }
      innerList.Add(item);
      OnChangeCount?.Invoke(this);
    }

    /// <summary>
    /// Synonym of Remove
    /// </summary>
    /// <param name="index">index to remove from innerList.</param>
    /// <see cref="Remove(int)"/>
    public void RemoveAt(int index)
    {
      innerList.RemoveAt(index);
      OnChangeCount?.Invoke(this);
    }

    /// <summary>
    /// Batch remove by array of indexies.
    /// </summary>
    /// <param name="Indices"></param>
    public void Removes(int[] indices)
    {
      if (indices.Length == 0) return;
      HashSet<int> removeSet = [.. indices];
      List<ListViewItem> newList = new(innerList.Count - indices.Length);
      for (int i = 0; i < innerList.Count; i++)
      {
        if (!removeSet.Contains(i))
          newList.Add(innerList[i]);
      }
      innerList = newList;
      _lvTarget.VirtualListSize = innerList.Count;
      OnChangeCount?.Invoke(this);
    }

    /// <summary>
    /// Count text in ListViewItem.ListViewSubItem.
    /// This function Counts equals or startsWith by text in argument.
    /// </summary>
    /// <param name="index"><see langword="ListViewSubItem"/> Index</param>
    /// <param name="text">Text to find in <see langword="ListViewSubItem"/></param>
    /// <returns>Count of match.</returns>
    /// <exception cref="ArgumentOutOfRangeException">index must be >=0, raises if index is negative number.</exception>
    public int GetCountByText(int index, string text)
    {
      if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "does not allow under 0 index value.");
      int cnt = innerList.Count(item =>
      {
        if (item.SubItems.Count <= index) return false;
        string t = item.SubItems[index].Text;
        return t.Equals(text) || t.StartsWith(text);
      });
      return cnt;
    }

    /// <summary>
    /// Check already text has been added.
    /// </summary>
    /// <param name="text">text to check, case insensitive.</param>
    /// <returns><see langword="true"/>:Already added, <see langword="false"/>:Not added yet.</returns>
    public bool HasInText(string text)
    {
      string lowText = text.ToLower();
      foreach(ListViewItem item in innerList)
      {
        if (item.Text.ToLower().Equals(lowText, StringComparison.Ordinal)) return true;
      }
      return false;
    }

    /// <summary>reverse all ListViewItems.Selected</summary>
    public void InvertSelection()
    {
      ListViewTarget.BeginUpdate();
      try
      {
        int cnt = innerList.Count;
        for (int i=0; i<cnt; i++)
        {
          ListViewItem item = this[i];
          item.Selected = !item.Selected;
        }
        ListViewTarget.Refresh();
      }
      finally
      {
        ListViewTarget.EndUpdate();
      }
    }

    /// <summary>
    /// make/return ListViewItem.text innerList separated by line-break from Selected ListViewItems to copy filenames function.
    /// </summary>
    /// <returns>Line separated string.</returns>
    public string GetCopyText()
    {
      StringBuilder sb = new();
      SelectedIndexCollection sels = _lvTarget.SelectedIndices;
      if (0 < sels.Count)
      {
        for (int i = 0, j = sels.Count; i < j; i++)
        {
          ListViewItem item = innerList[sels[i]];
          _ = sb.Append(item.Text).AppendLine();
        }
      }
      else
      {
        for (int i = 0, j = innerList.Count; i < j; i++)
        {
          ListViewItem item = innerList[i];
          _ = sb.Append(item.Text).AppendLine();
        }
      }
      return sb.ToString();
    }


    protected static void AddCopyHeaders(StringBuilder sb, ListView _lvTarget, char separator)
    {
      // Add header
      for (int i = 0, j = _lvTarget.Columns.Count; i < j; i++)
      {
        _ = sb.Append(_lvTarget.Columns[i].Text).Append(separator);
      }
      _ = sb.Remove(sb.Length - 1, 1);
      _ = sb.AppendLine();
    }

    /// <summary>
    /// make/return table string such as TSV(Tab Separated Values) from Selected ListViewItems to copy all function.
    /// </summary>
    /// <param name="separator">Char to separate column. be careful: this function does not escape strings in ListViewSubText even if use ',' as separator.</param>
    /// <returns>empty string: no items in innerList.</returns>
    public string GetCopyFullText(char separator = '\t')
    {
      int cnt = innerList.Count;
      if (0 == cnt) return "";

      StringBuilder sb = new(((cnt * 386 / 4096) + 1) * 4096); // acquire buffer by item count, rough calc
      AddCopyHeaders(sb, _lvTarget, separator);

      /// <summary>condition check function, return false -&gt; all</summary>
      [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
      static bool RetAll(ListViewItem item)
      {
        return false;
      }
      /// <summary>condition check function, return not selected -&gt; selected</summary>
      [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
      static bool RetSel(ListViewItem item)
      {
        return !item.Selected;
      }

      Func<ListViewItem, bool> fCond = 0 == _lvTarget.SelectedIndices.Count ? RetAll : RetSel;

      // Add Items
      for (int i = 0; i < cnt; i++)
      {
        ListViewItem item = this[i];
        if (fCond(item)) continue;
        for(int k = 0; k < item.SubItems.Count; k++)
        {
          _ = sb.Append(item.SubItems[k].Text).Append(separator);
        }
        _ = sb.Remove(sb.Length - 1, 1);
        _ = sb.AppendLine();
      }

      return sb.ToString();
    }

    /// <summary>
    /// Sort Items by comparer
    /// </summary>
    /// <param name="comparer">sort comparer</param>
    public void Sort(IComparer<ListViewItem> comparer)
    {
      _lvTarget.BeginUpdate();
      try
      {
        innerList.Sort(comparer);
        _lvTarget.Update();
        _lvTarget.Refresh();
      } finally {
        _lvTarget.EndUpdate();
      }
    }
    
    /// <summary>
    /// Queue to pop.
    /// </summary>
    /// <see cref="InitQueue(int)"/>
    protected BlockingCollection<ListViewItem> _queue = [];
    /// <summary>
    /// Returns how many items left in queue.
    /// </summary>
    public int QueueCount => _queue.Count;

    public void InitQueue(int statusColIndex)
    {
      _queue?.Dispose();
      _queue = new BlockingCollection<ListViewItem>(innerList.Count);
      for(int i = 0, j = innerList.Count; i < j; i++)
      {
        ListViewItem item = this[i];
        if (0 == item.SubItems[statusColIndex].Text.Length) _queue.Add(item);
      }
      _queue.CompleteAdding();
    }

    /// <summary>
    /// Pop a ListViewItem from queue. to use, call InitQueue first.
    /// text, colors are set to status column.
    /// </summary>
    /// <param name="columnIndex">index of status column header</param>
    /// <param name="textToSet">Status Text, eg: running...</param>
    /// <param name="fgColor">ForeGround Color of cell</param>
    /// <param name="bgColor">BackGround Color of cell</param>
    /// <returns>null: Queue is empty; ListViewItem: next item to process</returns>
    /// <see cref="InitQueue(int)"/>
    public ListViewItem? PopNext(int columnIndex, string textToSet, Color fgColor, Color bgColor)
    {
      if (0 == _queue.Count) return null;
      ListViewItem? item;
      try
      {
        item = _queue.Take();
      }
      catch
      {
        return null;
      }
      if (null == item) return null;

      ListViewItem.ListViewSubItem si = item.SubItems[columnIndex];
      si.Text = textToSet;
      si.ForeColor = fgColor;
      si.BackColor = bgColor;
      int index = item.Index;
      if (0 <= index && _lvTarget?.TopItem?.Index <= index) _lvTarget.RedrawItems(index, index, false);
      return item;
    }

  }

}