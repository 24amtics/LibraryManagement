namespace LibraryManagementMVC.Controllers;

public class LibraryController : Controller
{
    // Temporary in-memory sample data. It resets when the app restarts.
    private static readonly List<Book> Books = new()
    {
        new Book { BookId = 1, Title = "Clean Code", Author = "Robert C. Martin", Category = "Programming", Price = 650m, AvailableCopies = 4 },
        new Book { BookId = 2, Title = "Introduction to Algorithms", Author = "Thomas H. Cormen", Category = "Computer Science", Price = 900m, AvailableCopies = 2 },
        new Book { BookId = 3, Title = "The Pragmatic Programmer", Author = "Andrew Hunt", Category = "Programming", Price = 550m, AvailableCopies = 3 }
    };

    private static readonly List<LibraryMember> IssuedBooks = new()
    {
        new LibraryMember
        {
            MemberId = 1, MemberName = "Aarav Patel", Department = "CSE",
            Email = "aarav@example.com", BookId = 1,
            IssueDate = DateTime.Today.AddDays(-7),
            ReturnDate = DateTime.Today.AddDays(7),
            DueDate = DateTime.Today.AddDays(7)
        }
    };

    public IActionResult Index()
    {
        ViewBag.Books = Books;
        return View(IssuedBooks);
    }

    [HttpGet]
    public IActionResult IssueBook()
    {
        return View(new LibraryMember
        {
            IssueDate = DateTime.Today,
            ReturnDate = DateTime.Today.AddDays(14),
            DueDate = DateTime.Today.AddDays(14)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult IssueBook(LibraryMember member)
    {
        if (!Books.Any(b => b.BookId == member.BookId))
            ModelState.AddModelError(nameof(member.BookId), "That Book ID does not exist in the library.");

        var selectedBook = Books.FirstOrDefault(b => b.BookId == member.BookId);
        if (selectedBook is not null && selectedBook.AvailableCopies < 1)
            ModelState.AddModelError(nameof(member.BookId), "No copies of this book are currently available.");

        if (member.ReturnDate.HasValue && member.ReturnDate.Value.Date < member.IssueDate.Date)
            ModelState.AddModelError(nameof(member.ReturnDate), "Return date cannot be earlier than issue date.");

        if (!ModelState.IsValid)
            return View(member);

        member.MemberId = IssuedBooks.Count == 0 ? 1 : IssuedBooks.Max(m => m.MemberId) + 1;
        member.DueDate = member.ReturnDate ?? member.IssueDate.AddDays(14);
        IssuedBooks.Add(member);

        if (selectedBook is not null)
            selectedBook.AvailableCopies--;

        TempData["SuccessMessage"] = "Book issue record added successfully.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult BookDetails(int id)
    {
        var member = IssuedBooks.FirstOrDefault(m => m.MemberId == id);
        if (member is null)
            return NotFound();

        ViewBag.Book = Books.FirstOrDefault(b => b.BookId == member.BookId);
        return View(member);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarkReturned(int id)
    {
        var member = IssuedBooks.FirstOrDefault(m => m.MemberId == id);
        if (member is null)
            return NotFound();

        if (!member.ActualReturnDate.HasValue)
        {
            member.ActualReturnDate = DateTime.Today;
            member.ReturnDate = DateTime.Today;

            var book = Books.FirstOrDefault(b => b.BookId == member.BookId);
            if (book is not null)
                book.AvailableCopies++;

            TempData["SuccessMessage"] = "Book marked as returned.";
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult About() => View();
}
