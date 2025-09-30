namespace WinZoPNG
{
  public abstract class ListViewItemComparer(int col, SortOrder so) : IComparer<ListViewItem>
  {
    protected int _column = col;
    protected SortOrder _sortOrder = so;

    public int Column { get => _column; }
    public SortOrder Sorting { get => _sortOrder; }

    public abstract int Compare(ListViewItem? x, ListViewItem? y);
  }

  public class ListViewItemComparerInt(int col, SortOrder so) : ListViewItemComparer(col, so)
  {
    /// <summary>
    /// compare text of x and y as long integer.
    /// when cloud not parse text as long integer, compares as string.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>
    ///   Negative Number(<0): x is smaller than y, or x is null and y is not null.<br>
    ///   0: x equals y, or both of x and y are null.<br>
    ///   Plus Number(>0): x is bigger than y, or x is not null and y is null.
    /// </returns>
    public override int Compare(ListViewItem? x, ListViewItem? y)
    {
      if (null == x && null == y) return 0;
      if (null == x) return -1;
      if (null == y) return 1;
      if (_sortOrder.Equals(SortOrder.Descending)) (y, x) = (x, y); // Reverse order if Descending Sort

      // remove comma due to TryParse could not recognize comma.
      string sx = x.SubItems[_column].Text.Trim().Replace(",", "");
      string sy = y.SubItems[_column].Text.Trim().Replace(",", "");

      // set "0" if zero-length text
      if (sx.Length == 0) sx = "0";
      if (sy.Length == 0) sx = "0";

      if (long.TryParse(sx, out long lx) && long.TryParse(sy, out long ly))
      {
        // Succeeded parse as long integer. now compare as long
        // do NOT not subtraction due to risk of overflow.
        return lx < ly ? -1 : lx == ly ? 0 : 1;
      }

      // compare as string when parse failed.
      return sx.CompareTo(sy);
    }

  }
  public class ListViewItemComparerStr(int col, SortOrder so) : ListViewItemComparer(col, so)
  {
    public override int Compare(ListViewItem? x, ListViewItem? y)
    {
      if (null == x && null == y) return 0;
      if (null == x) return -1;
      if (null == y) return 1;
      if (_sortOrder.Equals(SortOrder.Descending)) (y, x) = (x, y); // Reverse order if Descending Sort

      return x.SubItems[_column].Text.CompareTo(y.SubItems[_column].Text);
    }
  }

}