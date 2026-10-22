namespace LibraryManagementMVC.Extensions;

public static class LibraryExtensions
{
    public static string GetDueStatus(this LibraryMember member)
    {
        // ActualReturnDate indicates the book has been returned.
        if (member.ActualReturnDate.HasValue)
            return "Returned";

        if (!member.ReturnDate.HasValue)
            return "Issued";

        var daysRemaining = (member.ReturnDate.Value.Date - DateTime.Today).Days;

        if (daysRemaining < 0)
            return "Overdue";

        if (daysRemaining <= 3)
            return "Due Soon";

        return "Issued";
    }

    public static decimal CalculateFine(this LibraryMember member)
    {
        // Fine is based on overdue days after the expected return date.
        if (member.ActualReturnDate.HasValue || !member.ReturnDate.HasValue)
            return 0m;

        var overdueDays = (DateTime.Today.Date - member.ReturnDate.Value.Date).Days;
        return overdueDays > 0 ? overdueDays * 10m : 0m;
    }
}
