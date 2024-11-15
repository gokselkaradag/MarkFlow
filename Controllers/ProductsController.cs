﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RabbitMQWeb.Watermark.Models;
using RabbitMQWeb.Watermark.Services;

namespace RabbitMQWeb.Watermark.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DataContext _context;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public ProductsController(DataContext context, RabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Stock,ImageName")] Product product, IFormFile ImageFile)
        {
            if (!ModelState.IsValid) return View(product);

            if (ImageFile is { Length: > 0 })
            {
                var randomImageName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", randomImageName);

                await using FileStream stream = new(path, FileMode.Create);

                await ImageFile.CopyToAsync(stream);

                _rabbitMQPublisher.Publish(new ProductImageCreatedEvent() { ImageName = randomImageName });

                product.ImageName = randomImageName;
            }

            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Stock,ImageName")] Product product, IFormFile ImageFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return View(product);

            try
            {
                if (ImageFile is { Length: > 0 })
                {
                    // Yeni bir resim yüklendiyse, eski resmi silmeyi düşünebilirsiniz
                    if (!string.IsNullOrEmpty(product.ImageName))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", product.ImageName);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Yeni resmi kaydet
                    var randomImageName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", randomImageName);

                    await using FileStream stream = new(path, FileMode.Create);
                    await ImageFile.CopyToAsync(stream);

                    // RabbitMQ'ya yeni resim bilgisi gönder
                    _rabbitMQPublisher.Publish(new ProductImageCreatedEvent() { ImageName = randomImageName });

                    // Yeni resim adını güncelle
                    product.ImageName = randomImageName;
                }

                // Ürünü güncelle
                _context.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
