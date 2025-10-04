namespace Idler.Helpers
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    public class SortComparer : IComparer
    {
        private readonly SortDescriptionCollection sortDescriptions;

        public SortComparer(SortDescriptionCollection sortDescriptions)
        {
            this.sortDescriptions = sortDescriptions;
        }

        public int Compare(object x, object y)
        {
            if (this.sortDescriptions.Count == 0)
            {
                return 0;
            }

            foreach (var description in sortDescriptions)
            {
                var propertyA = x.GetType().GetProperty(description.PropertyName);
                var propertyB = y.GetType().GetProperty(description.PropertyName);

                if (propertyA == null || propertyB == null)
                {
                    continue;
                }

                IComparable valueA = propertyA.GetValue(x) as IComparable;
                IComparable valueB = propertyB.GetValue(y) as IComparable;

                if (valueA == null || valueB == null)
                {
                    continue;
                }

                int comparisonResult = valueA.CompareTo(valueB);
                if (description.Direction == ListSortDirection.Descending)
                {
                    comparisonResult = -comparisonResult;
                }

                if (comparisonResult != 0)
                {
                    return comparisonResult;
                }
            }

            return 0;
        }
    }
}
