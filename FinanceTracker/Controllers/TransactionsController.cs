using FinanceTracker.Data;
using FinanceTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceTracker.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionsController(AuthDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User); // Get current logged-in user's ID

            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .Include(t=>t.User)
                .ToListAsync();

            return View(transactions);
        }


        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            // Get the currently logged-in user's ID
            var userId = _userManager.GetUserId(User);

            // Pass the category list for dropdown
            ViewData["CategoryId"] = new SelectList(_context.TransactionCategories, "Id", "Id");

            // Instead of a dropdown for UserId, just pass the user ID directly
            var model = new Transaction
            {
                UserId = userId // pre-fill the form field
            };

            return View(model);
        }


        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Amount,Date,CategoryId,UserId")] Transaction transaction)
        {
            transaction.UserId = _userManager.GetUserId(User);
            Console.WriteLine(transaction.UserId);
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                foreach (var error in errors)
                {
                    Console.WriteLine(error);

                }
            }

            if (ModelState.IsValid)
            {
                
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.TransactionCategories, "Id", "Id", transaction.CategoryId);
            ViewData["UserId"]= _userManager.GetUserId(User);
            return View(transaction);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound();

            // Load dropdown for categories
            ViewData["CategoryId"] = new SelectList(_context.TransactionCategories, "Id", "Id", transaction.CategoryId);

            // Optional: Set userId again from logged-in user
            transaction.UserId = _userManager.GetUserId(User);

            return View(transaction);
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Amount,Date,CategoryId,UserId")] Transaction transaction)
        {
            if (id != transaction.Id)
                return NotFound();
            transaction.UserId = _userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                try
                {

                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Transactions.Any(e => e.Id == transaction.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.TransactionCategories, "Id", "Id", transaction.CategoryId);
            return View(transaction);
        }



        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }

        [Authorize]
        public async Task<IActionResult> Summary()
        {
            var userId = _userManager.GetUserId(User);

            var summaryData = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .GroupBy(t => new
                {
                    t.Date.Year,
                    t.Date.Month,
                    Category = t.Category.Name
                })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Category = g.Key.Category,
                    Total = (decimal)g.Sum(x => (double)x.Amount) // SQLite workaround
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            ViewBag.SummaryData = summaryData;

            return View();
        }



    }
}
